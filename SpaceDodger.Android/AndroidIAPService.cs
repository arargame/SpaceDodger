using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.BillingClient.Api;
using ArarGames.Core.Applications;
using SpaceDodger.Core;

namespace SpaceDodger.Droid
{
    /// <summary>
    /// Google Play Billing Library v8 wrapper for Space Dodger.
    /// Handles in-app purchases (Remove Ads, Buy Coffee) and purchase restoration.
    /// Ported and refined from the battle-tested Blocked / PaintTrek architecture.
    /// </summary>
#pragma warning disable CS0618
    [global::Android.Runtime.Preserve(AllMembers = true)]
    public class AndroidIAPService : Java.Lang.Object, IPurchasesUpdatedListener, IBillingClientStateListener
    {
        private static readonly string[] AllProductIds = new[]
        {
            ArarGamesApplications.RemoveAdsProductId,
            ArarGamesApplications.CoffeeProductId
        };

        private readonly MainActivity _activity;
        private BillingClient _billingClient;
        private TaskCompletionSource<bool> _connectionTcs;
        private TaskCompletionSource<bool> _purchaseTcs;
        private readonly ConcurrentDictionary<string, ProductDetails> _productCache = new();
        private int _purchaseInFlight; // 0 = idle, 1 = in progress

        private bool IsConnected => _billingClient?.IsReady ?? false;

        public AndroidIAPService(MainActivity activity)
        {
            _activity = activity;
            Log("AndroidIAPService created. Initializing BillingClient...");
            InitBillingClient();
        }

        private void InitBillingClient()
        {
            try
            {
                var context = _activity ?? global::Android.App.Application.Context;
                var pendingParams = PendingPurchasesParams.NewBuilder()
                    .EnableOneTimeProducts()
                    .Build();

                _billingClient = BillingClient.NewBuilder(context)
                    .SetListener(this)
                    .EnablePendingPurchases(pendingParams)
                    .Build();

                Log("BillingClient built successfully. Starting connection...");
                _connectionTcs = new TaskCompletionSource<bool>();
                _billingClient.StartConnection(this);
            }
            catch (Exception ex)
            {
                LogError($"InitBillingClient exception: {ex.Message}");
            }
        }

        // ── IBillingClientStateListener ─────────────────────────────────────────────

        public void OnBillingSetupFinished(BillingResult billingResult)
        {
            if (billingResult.ResponseCode == BillingResponseCode.Ok)
            {
                Log("OnBillingSetupFinished: OK. Pre-querying product details...");
                _connectionTcs?.TrySetResult(true);
                _ = QueryProductDetailsAsync();
            }
            else
            {
                LogError($"OnBillingSetupFinished FAILED: Code={billingResult.ResponseCode}, Msg={billingResult.DebugMessage}");
                _connectionTcs?.TrySetResult(false);
            }
        }

        public void OnBillingServiceDisconnected()
        {
            LogWarn("OnBillingServiceDisconnected. Billing service disconnected.");
            _connectionTcs?.TrySetResult(false);
        }

        private async Task<bool> EnsureConnectedAsync()
        {
            if (IsConnected) return true;

            Log("EnsureConnected: Starting connection...");
            _connectionTcs = new TaskCompletionSource<bool>();
            _billingClient.StartConnection(this);

            var timeout = Task.Delay(10000);
            var completed = await Task.WhenAny(_connectionTcs.Task, timeout);
            if (completed == timeout)
            {
                LogError("EnsureConnected: Connection timeout (10s).");
                return false;
            }

            return await _connectionTcs.Task;
        }

        // ── Product Details Cache ───────────────────────────────────────────────────

        private readonly object _queryGate = new object();
        private Task _productQueryTask;

        private Task QueryProductDetailsAsync()
        {
            lock (_queryGate)
            {
                if (_productQueryTask != null && !_productQueryTask.IsCompleted)
                    return _productQueryTask;

                _productQueryTask = QueryProductDetailsCoreAsync();
                return _productQueryTask;
            }
        }

        private async Task QueryProductDetailsCoreAsync()
        {
            try
            {
                var products = AllProductIds.Select(id =>
                    QueryProductDetailsParams.Product.NewBuilder()
                        .SetProductId(id)
                        .SetProductType(BillingClient.ProductType.Inapp)
                        .Build()
                ).ToArray();

                var tcs = new TaskCompletionSource<bool>();
                var queryParams = QueryProductDetailsParams.NewBuilder()
                    .SetProductList(products)
                    .Build();

                _billingClient.QueryProductDetails(queryParams, new ProductDetailsListener(this, tcs));

                var timeout = Task.Delay(10000);
                var completed = await Task.WhenAny(tcs.Task, timeout);
                if (completed == timeout)
                {
                    LogError("QueryProductDetails: Timeout.");
                }
                else
                {
                    Log($"QueryProductDetails: Done. Cached {_productCache.Count} product(s).");
                }
            }
            catch (Exception ex)
            {
                LogError($"QueryProductDetailsCoreAsync exception: {ex.Message}");
            }
        }

        // ── Purchase Initiation ─────────────────────────────────────────────────────

        public async Task<bool> BuyProductAsync(string productId)
        {
            Log($"BuyProductAsync START | productId='{productId}'");

            if (System.Threading.Interlocked.CompareExchange(ref _purchaseInFlight, 1, 0) != 0)
            {
                LogWarn("BuyProductAsync: Another purchase is already in flight. Ignored.");
                return false;
            }

            bool purchaseSuccess = false;

            try
            {
                if (!await EnsureConnectedAsync())
                {
                    LogError("BuyProductAsync: Failed to connect to Billing service.");
                    return false;
                }

                if (!_productCache.ContainsKey(productId))
                {
                    Log($"Product '{productId}' not in cache, fetching details...");
                    await QueryProductDetailsAsync();
                }

                if (!_productCache.TryGetValue(productId, out var details))
                {
                    LogError($"BuyProductAsync: ProductDetails not found for '{productId}'. Make sure it is created and active in Google Play Console.");
                    return false;
                }

                var paramsBuilder = BillingFlowParams.ProductDetailsParams.NewBuilder()
                    .SetProductDetails(details)
                    .Build();

                var flowParams = BillingFlowParams.NewBuilder()
                    .SetProductDetailsParamsList(new List<BillingFlowParams.ProductDetailsParams> { paramsBuilder })
                    .Build();

                var activity = _activity ?? MainActivity.Instance;
                if (activity == null)
                {
                    LogError("BuyProductAsync: Activity is null, cannot launch flow.");
                    return false;
                }

                _purchaseTcs = new TaskCompletionSource<bool>();

                Log($"Launching billing flow for '{productId}'...");
                BillingResult launchResult = _billingClient.LaunchBillingFlow(activity, flowParams);
                Log($"LaunchBillingFlow result: Code={launchResult.ResponseCode}, Msg={launchResult.DebugMessage}");

                if (launchResult.ResponseCode == BillingResponseCode.ItemAlreadyOwned)
                {
                    LogWarn($"LaunchBillingFlow: ITEM_ALREADY_OWNED for '{productId}' - restoring entitlement.");
                    return await RestorePurchasesAsync();
                }

                if (launchResult.ResponseCode != BillingResponseCode.Ok)
                {
                    LogError($"LaunchBillingFlow failed with code {launchResult.ResponseCode}");
                    return false;
                }

                var timeout = Task.Delay(120000); // 2 minute timeout
                var completed = await Task.WhenAny(_purchaseTcs.Task, timeout);
                if (completed == timeout)
                {
                    LogError("BuyProductAsync: Purchase flow timed out (2 min).");
                    return false;
                }

                purchaseSuccess = await _purchaseTcs.Task;
            }
            catch (Exception ex)
            {
                LogError($"BuyProductAsync exception: {ex.Message}");
            }
            finally
            {
                _purchaseTcs = null;
                System.Threading.Interlocked.Exchange(ref _purchaseInFlight, 0);
                Log($"BuyProductAsync END | result={purchaseSuccess}");
            }

            return purchaseSuccess;
        }

        // ── IPurchasesUpdatedListener ───────────────────────────────────────────────

        public void OnPurchasesUpdated(BillingResult result, IList<Purchase> purchases)
        {
            try
            {
                Log($"OnPurchasesUpdated: Code={result.ResponseCode}, Count={purchases?.Count ?? 0}");

                if (result.ResponseCode == BillingResponseCode.Ok && purchases != null)
                {
                    foreach (var purchase in purchases)
                    {
                        HandlePurchase(purchase);
                    }
                }
                else if (result.ResponseCode == BillingResponseCode.UserCancelled)
                {
                    Log("OnPurchasesUpdated: User cancelled.");
                    _purchaseTcs?.TrySetResult(false);
                }
                else if (result.ResponseCode == BillingResponseCode.ItemAlreadyOwned)
                {
                    LogWarn("OnPurchasesUpdated: ITEM_ALREADY_OWNED, restoring entitlement.");
                    _ = RestorePurchasesAsync();
                    _purchaseTcs?.TrySetResult(true);
                }
                else
                {
                    LogError($"OnPurchasesUpdated: Error code={result.ResponseCode}, msg={result.DebugMessage}");
                    _purchaseTcs?.TrySetResult(false);
                }
            }
            catch (Exception ex)
            {
                LogError($"OnPurchasesUpdated exception: {ex.Message}");
                _purchaseTcs?.TrySetResult(false);
            }
        }

        private void HandlePurchase(Purchase purchase)
        {
            if (purchase.PurchaseState != PurchaseState.Purchased)
            {
                LogWarn($"HandlePurchase: PurchaseState is {purchase.PurchaseState}, skipping.");
                return;
            }

            var products = purchase.Products ?? new List<string>();
            Log($"HandlePurchase: Products=[{string.Join(", ", products)}], Token={purchase.PurchaseToken}");

            bool isRemoveAds = products.Contains(ArarGamesApplications.RemoveAdsProductId);
            bool isCoffee = products.Contains(ArarGamesApplications.CoffeeProductId);

            if (isRemoveAds)
            {
                ApplyRemoveAds();

                if (!purchase.IsAcknowledged)
                {
                    var ackParams = AcknowledgePurchaseParams.NewBuilder()
                        .SetPurchaseToken(purchase.PurchaseToken)
                        .Build();
                    Log("Acknowledging Remove Ads purchase...");
                    _billingClient.AcknowledgePurchase(ackParams, new AcknowledgeListener(this));
                }
                else
                {
                    _purchaseTcs?.TrySetResult(true);
                }
            }
            else if (isCoffee)
            {
                Log("Consuming Buy Coffee purchase token...");
                var consumeParams = ConsumeParams.NewBuilder()
                    .SetPurchaseToken(purchase.PurchaseToken)
                    .Build();
                _billingClient.Consume(consumeParams, new ConsumeListener(this));
            }
            else
            {
                _purchaseTcs?.TrySetResult(true);
            }
        }

        private void ApplyRemoveAds()
        {
            try
            {
                var activity = _activity ?? MainActivity.Instance;
                activity?.RunOnUiThread(() =>
                {
                    try
                    {
                        var context = activity.GameContext;
                        if (context != null)
                        {
                            context.Save.Data.AdsRemoved = true;
                            context.Save.Save();
                        }
                        activity.HideBannerAd();
                        Log("ApplyRemoveAds: AdsRemoved set to true and banner hidden.");
                    }
                    catch (Exception ex)
                    {
                        LogError($"ApplyRemoveAds inner exception: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                LogError($"ApplyRemoveAds exception: {ex.Message}");
            }
        }

        // ── Restore Purchases ───────────────────────────────────────────────────────

        public async Task<bool> RestorePurchasesAsync()
        {
            try
            {
                if (!IsConnected && !await EnsureConnectedAsync())
                {
                    LogWarn("RestorePurchases: Billing not connected.");
                    return false;
                }

                var tcs = new TaskCompletionSource<List<Purchase>>();
                var queryParams = QueryPurchasesParams.NewBuilder()
                    .SetProductType(BillingClient.ProductType.Inapp)
                    .Build();

                _billingClient.QueryPurchases(queryParams, new RestoreListener(tcs));

                var completed = await Task.WhenAny(tcs.Task, Task.Delay(8000));
                if (completed != tcs.Task)
                {
                    LogWarn("RestorePurchases: Query timeout.");
                    return false;
                }

                var purchases = await tcs.Task;
                if (purchases == null || purchases.Count == 0)
                {
                    Log("RestorePurchases: No active purchases found.");
                    return false;
                }

                bool restored = false;

                foreach (var purchase in purchases)
                {
                    if (purchase.PurchaseState != PurchaseState.Purchased) continue;

                    var products = purchase.Products ?? new List<string>();
                    if (products.Contains(ArarGamesApplications.RemoveAdsProductId))
                    {
                        ApplyRemoveAds();
                        restored = true;
                        Log("RestorePurchases: Remove Ads entitlement successfully restored.");

                        if (!purchase.IsAcknowledged)
                        {
                            var ackParams = AcknowledgePurchaseParams.NewBuilder()
                                .SetPurchaseToken(purchase.PurchaseToken)
                                .Build();
                            _billingClient.AcknowledgePurchase(ackParams, new AcknowledgeListener(this));
                        }
                    }
                }

                return restored;
            }
            catch (Exception ex)
            {
                LogError($"RestorePurchasesAsync exception: {ex.Message}");
                return false;
            }
        }

        // ── Listener Implementations ────────────────────────────────────────────────

        [global::Android.Runtime.Preserve(AllMembers = true)]
        private class ProductDetailsListener : Java.Lang.Object, IProductDetailsResponseListener
        {
            private readonly AndroidIAPService _service;
            private readonly TaskCompletionSource<bool> _tcs;

            public ProductDetailsListener(AndroidIAPService service, TaskCompletionSource<bool> tcs)
            {
                _service = service;
                _tcs = tcs;
            }

            public void OnProductDetailsResponse(BillingResult billingResult, QueryProductDetailsResult result)
            {
                try
                {
                    int count = result?.ProductDetailsList?.Count ?? 0;
                    _service.Log($"ProductDetailsListener: Code={billingResult.ResponseCode}, Count={count}");

                    if (billingResult.ResponseCode == BillingResponseCode.Ok && result?.ProductDetailsList != null)
                    {
                        foreach (var detail in result.ProductDetailsList)
                        {
                            _service._productCache[detail.ProductId] = detail;
                            _service.Log($"  Cached product: '{detail.ProductId}'");
                        }
                        _tcs.TrySetResult(true);
                    }
                    else
                    {
                        _service.LogError($"ProductDetailsListener FAILED: Code={billingResult.ResponseCode}, Msg={billingResult.DebugMessage}");
                        _tcs.TrySetResult(false);
                    }
                }
                catch (Exception ex)
                {
                    _service.LogError($"ProductDetailsListener exception: {ex.Message}");
                    _tcs.TrySetResult(false);
                }
            }
        }

        [global::Android.Runtime.Preserve(AllMembers = true)]
        private class AcknowledgeListener : Java.Lang.Object, IAcknowledgePurchaseResponseListener
        {
            private readonly AndroidIAPService _service;

            public AcknowledgeListener(AndroidIAPService service) => _service = service;

            public void OnAcknowledgePurchaseResponse(BillingResult billingResult)
            {
                try
                {
                    _service.Log($"AcknowledgeListener: Code={billingResult.ResponseCode}, Msg={billingResult.DebugMessage}");
                    _service._purchaseTcs?.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    _service.LogError($"AcknowledgeListener exception: {ex.Message}");
                    _service._purchaseTcs?.TrySetResult(true);
                }
            }
        }

        [global::Android.Runtime.Preserve(AllMembers = true)]
        private class ConsumeListener : Java.Lang.Object, IConsumeResponseListener
        {
            private readonly AndroidIAPService _service;

            public ConsumeListener(AndroidIAPService service) => _service = service;

            public void OnConsumeResponse(BillingResult billingResult, string purchaseToken)
            {
                try
                {
                    _service.Log($"ConsumeListener: Code={billingResult.ResponseCode}");
                    _service._purchaseTcs?.TrySetResult(billingResult.ResponseCode == BillingResponseCode.Ok);
                }
                catch (Exception ex)
                {
                    _service.LogError($"ConsumeListener exception: {ex.Message}");
                    _service._purchaseTcs?.TrySetResult(true);
                }
            }
        }

        [global::Android.Runtime.Preserve(AllMembers = true)]
        private class RestoreListener : Java.Lang.Object, IPurchasesResponseListener
        {
            private readonly TaskCompletionSource<List<Purchase>> _tcs;

            public RestoreListener(TaskCompletionSource<List<Purchase>> tcs) => _tcs = tcs;

            public void OnQueryPurchasesResponse(BillingResult result, IList<Purchase> purchases)
            {
                try
                {
                    var list = new List<Purchase>();
                    if (result.ResponseCode == BillingResponseCode.Ok && purchases != null)
                        list.AddRange(purchases);

                    _tcs.TrySetResult(list);
                }
                catch (Exception ex)
                {
                    global::Android.Util.Log.Error("IAP", $"RestoreListener exception: {ex.Message}");
                    _tcs.TrySetResult(new List<Purchase>());
                }
            }
        }

        private void Log(string msg) => global::Android.Util.Log.Info("IAP", $"[SpaceDodger - IAP] {msg}");
        private void LogWarn(string msg) => global::Android.Util.Log.Warn("IAP", $"[SpaceDodger - IAP] {msg}");
        private void LogError(string msg) => global::Android.Util.Log.Error("IAP", $"[SpaceDodger - IAP] {msg}");
    }
#pragma warning restore CS0618
}
