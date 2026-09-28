using Android.App;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Gms.Ads.Rewarded;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.Xna.Framework;
using SpaceDodger.Core;

namespace SpaceDodger.Droid
{
    /// <summary>Android entry point hosting the shared MonoGame game.</summary>
    [Activity(
        Label = "Space Dodger",
        Icon = "@mipmap/spacedodgerico",
        MainLauncher = true,
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        ConfigurationChanges =
            ConfigChanges.Orientation | ConfigChanges.Keyboard |
            ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class MainActivity : AndroidGameActivity
    {
        public static MainActivity Instance { get; private set; }

#if DEBUG
        private const string BannerAdUnitId = "ca-app-pub-3940256099942544/6300978111"; // Google Official Test Banner ID
        private const string RewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; // Google Official Test Rewarded ID
#else
        private const string BannerAdUnitId = "ca-app-pub-3062759184051966/8797177752"; // Space Dodger Production Banner ID
        private const string RewardedAdUnitId = "ca-app-pub-3062759184051966/5568379185"; // Space Dodger Production Rewarded ID
#endif

        private SpaceDodgerGame _game;
        private AndroidPlatform _platform;
        private View _view;
        private FrameLayout _rootLayout;
        private FrameLayout _bannerContainer;
        private AdView _adView;
        private bool _isBannerVisible;
        private RewardedAd _rewardedAd;
        private bool _isLoadingRewarded;
        private AndroidIAPService _iapService;

        public GameContext GameContext => _game?.Context;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Instance = this;

            // Ekranı oyun sırasında açık tut
            Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
            EnableImmersiveMode();

            _iapService = new AndroidIAPService(this);
            _platform = new AndroidPlatform(this);
            _game = new SpaceDodgerGame(_platform);
            _view = _game.Services.GetService(typeof(View)) as View;

            if (_view != null)
            {
                _rootLayout = new FrameLayout(this);
                _rootLayout.AddView(_view, new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
                SetContentView(_rootLayout);
                _view.Focusable = true;
                _view.FocusableInTouchMode = true;
                _view.RequestFocus();
            }

            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    MobileAds.Initialize(this);
                    LoadRewardedAd();
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdMob] MobileAds.Initialize error: {ex.Message}");
                }
            });

            _game.Run();
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (hasFocus)
            {
                EnableImmersiveMode();
            }
        }

        public override void OnBackPressed()
        {
            _platform?.RequestBack();
        }

        /// <summary>
        /// Safely exits the application, terminates the activity and cleans up the OS process.
        /// Resolves both the Visual Studio hanging session and the OpenGL black-texture artifact on relaunch.
        /// </summary>
        public void SafeExit()
        {
            RunOnUiThread(() =>
            {
                try
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        FinishAndRemoveTask();
                    }
                    else
                    {
                        Finish();
                    }

                    new System.Threading.Thread(() =>
                    {
                        try
                        {
                            System.Threading.Thread.Sleep(250);
                            Process.KillProcess(Process.MyPid());
                        }
                        catch { }
                    }).Start();
                }
                catch (System.Exception)
                {
                    try { Process.KillProcess(Process.MyPid()); }
                    catch { }
                }
            });
        }

        public bool IsInterstitialReady() => false;

        public void ShowInterstitialAd(System.Action onClosed)
        {
            RunOnUiThread(() =>
            {
                onClosed?.Invoke();
            });
        }

        public bool IsRewardedAdReady() => _rewardedAd != null;

        public void LoadRewardedAd()
        {
            if (_rewardedAd != null || _isLoadingRewarded) return;
            _isLoadingRewarded = true;

            RunOnUiThread(() =>
            {
                try
                {
                    var adRequest = new AdRequest.Builder().Build();
                    var callback = new MyRewardedAdLoadCallback(
                        onLoaded: (ad) =>
                        {
                            _rewardedAd = ad;
                            _isLoadingRewarded = false;
                            System.Diagnostics.Debug.WriteLine("[AdMob] Rewarded ad preloaded successfully.");
                        },
                        onFailed: (error) =>
                        {
                            _rewardedAd = null;
                            _isLoadingRewarded = false;
                            System.Diagnostics.Debug.WriteLine($"[AdMob] Rewarded ad failed to preload: {error?.Message}");
                        });

                    RewardedAd.Load(this, RewardedAdUnitId, adRequest, callback);
                }
                catch (System.Exception ex)
                {
                    _isLoadingRewarded = false;
                    System.Diagnostics.Debug.WriteLine($"[AdMob] LoadRewardedAd exception: {ex.Message}");
                }
            });
        }

        public void ShowRewardedAd(System.Action onRewardEarned, System.Action onClosed = null)
        {
            RunOnUiThread(() =>
            {
                object lockObj = new object();
                bool earnedCalled = false;
                bool closedCalled = false;

                System.Action safeOnRewardEarned = () =>
                {
                    lock (lockObj)
                    {
                        if (earnedCalled) return;
                        earnedCalled = true;
                    }
                    onRewardEarned?.Invoke();
                };

                System.Action safeOnClosed = () =>
                {
                    lock (lockObj)
                    {
                        if (closedCalled) return;
                        closedCalled = true;
                    }
                    LoadRewardedAd();
                    onClosed?.Invoke();
                };

                try
                {
                    if (_rewardedAd != null)
                    {
                        var adToShow = _rewardedAd;
                        _rewardedAd = null;

                        adToShow.FullScreenContentCallback = new MyRewardedFullScreenCallback(safeOnClosed);
                        var rewardListener = new MyOnUserEarnedRewardListener(safeOnRewardEarned);
                        adToShow.Show(this, rewardListener);
                    }
                    else
                    {
                        var adRequest = new AdRequest.Builder().Build();
                        var callback = new MyRewardedAdLoadCallback(
                            onLoaded: (ad) =>
                            {
                                RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        ad.FullScreenContentCallback = new MyRewardedFullScreenCallback(safeOnClosed);
                                        var rewardListener = new MyOnUserEarnedRewardListener(safeOnRewardEarned);
                                        ad.Show(this, rewardListener);
                                    }
                                    catch (System.Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"[AdMob] Show on loaded ad failed: {ex.Message}");
                                        safeOnClosed();
                                    }
                                });
                            },
                            onFailed: (error) =>
                            {
                                System.Diagnostics.Debug.WriteLine($"[AdMob] On-demand rewarded ad failed to load: {error?.Message}");
                                safeOnClosed();
                            });

                        RewardedAd.Load(this, RewardedAdUnitId, adRequest, callback);
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdMob] ShowRewardedAd outer exception: {ex.Message}");
                    safeOnClosed();
                }
            });
        }

        public void ShowBannerAd(int x, int y, int width, int height)
        {
            RunOnUiThread(() =>
            {
                try
                {
                    if (_game?.Context?.Save?.Data?.AdsRemoved == true)
                        return;

                    if (_bannerContainer == null)
                    {
                        _bannerContainer = new FrameLayout(this)
                        {
                            Clickable = false,
                            Focusable = false
                        };
                        _bannerContainer.SetBackgroundColor(global::Android.Graphics.Color.Transparent);

                        _adView = new AdView(this)
                        {
                            AdUnitId = BannerAdUnitId,
                            AdSize = AdSize.Banner
                        };

                        var adLp = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                        {
                            Gravity = GravityFlags.Center
                        };
                        _bannerContainer.AddView(_adView, adLp);

                        var lp = new FrameLayout.LayoutParams(width, height)
                        {
                            LeftMargin = x,
                            TopMargin = y,
                            Gravity = GravityFlags.Top | GravityFlags.Left
                        };

                        _rootLayout?.AddView(_bannerContainer, lp);

                        _adView.AdListener = new BannerAdListener(_adView, width, height);

                        var adRequest = new AdRequest.Builder().Build();
                        _adView.LoadAd(adRequest);
                        _isBannerVisible = true;
                    }
                    else
                    {
                        var lp = new FrameLayout.LayoutParams(width, height)
                        {
                            LeftMargin = x,
                            TopMargin = y,
                            Gravity = GravityFlags.Top | GravityFlags.Left
                        };
                        _bannerContainer.LayoutParameters = lp;

                        if (!_isBannerVisible)
                        {
                            _bannerContainer.Visibility = ViewStates.Visible;
                            try { _adView?.Resume(); } catch { }
                            _isBannerVisible = true;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdMob] ShowBannerAd error: {ex.Message}");
                }
            });
        }

        public void HideBannerAd()
        {
            RunOnUiThread(() =>
            {
                try
                {
                    if (_bannerContainer != null && _isBannerVisible)
                    {
                        _bannerContainer.Visibility = ViewStates.Gone;
                        try { _adView?.Pause(); } catch { }
                        _isBannerVisible = false;
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdMob] HideBannerAd error: {ex.Message}");
                }
            });
        }

        public void PurchaseProduct(string productId, bool isConsumable)
        {
            _ = _iapService?.BuyProductAsync(productId);
        }

        public void RestorePurchases()
        {
            _ = _iapService?.RestorePurchasesAsync();
        }

        protected override void OnPause()
        {
            base.OnPause();
            try { _adView?.Pause(); } catch { }
        }

        protected override void OnResume()
        {
            base.OnResume();
            try { _adView?.Resume(); } catch { }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            try
            {
                _adView?.Destroy();
            }
            catch { }

            try
            {
                _game?.Dispose();
            }
            catch { }

            if (IsFinishing)
            {
                new System.Threading.Thread(() =>
                {
                    try
                    {
                        System.Threading.Thread.Sleep(200);
                        Process.KillProcess(Process.MyPid());
                    }
                    catch { }
                }).Start();
            }
        }

        private void EnableImmersiveMode()
        {
            try
            {
                if (Window == null) return;

#pragma warning disable CA1416
                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    var controller = Window.InsetsController;
                    if (controller != null)
                    {
                        controller.Hide(WindowInsets.Type.StatusBars() | WindowInsets.Type.NavigationBars());
                        controller.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
                    }
                }
                else if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
                {
#pragma warning disable CS0618
                    var decorView = Window.DecorView;
                    if (decorView != null)
                    {
                        decorView.SystemUiVisibility = (StatusBarVisibility)(
                            SystemUiFlags.LayoutStable |
                            SystemUiFlags.LayoutHideNavigation |
                            SystemUiFlags.LayoutFullscreen |
                            SystemUiFlags.HideNavigation |
                            SystemUiFlags.Fullscreen |
                            SystemUiFlags.ImmersiveSticky);
                    }
#pragma warning restore CS0618
                }
#pragma warning restore CA1416
            }
            catch { }
        }
    }

    internal sealed class BannerAdListener : AdListener
    {
        private readonly AdView _adView;
        private readonly int _targetWidth;
        private readonly int _targetHeight;

        public BannerAdListener(AdView adView, int targetWidth, int targetHeight)
        {
            _adView = adView;
            _targetWidth = targetWidth;
            _targetHeight = targetHeight;
        }

        public override void OnAdLoaded()
        {
            base.OnAdLoaded();
            try
            {
                var context = _adView.Context;
                if (context != null)
                {
                    int wPixels = _adView.AdSize.GetWidthInPixels(context);
                    int hPixels = _adView.AdSize.GetHeightInPixels(context);
                    if (wPixels > 0 && hPixels > 0)
                    {
                        float scaleX = (float)_targetWidth / wPixels;
                        float scaleY = (float)_targetHeight / hPixels;
                        float scale = System.Math.Min(scaleX, scaleY);
                        if (scale < 1.0f)
                        {
                            _adView.ScaleX = scale;
                            _adView.ScaleY = scale;
                            _adView.PivotX = wPixels / 2f;
                            _adView.PivotY = hPixels / 2f;
                        }
                    }
                }
            }
            catch { }
        }

        public override void OnAdFailedToLoad(LoadAdError error)
        {
            base.OnAdFailedToLoad(error);
            System.Diagnostics.Debug.WriteLine($"[AdMob] Banner failed to load: {error?.Message}");
        }
    }

#pragma warning disable CS0618
    [global::Android.Runtime.Preserve(AllMembers = true)]
    [global::Android.Runtime.Register("com/google/android/gms/ads/rewarded/RewardedAdLoadCallback", DoNotGenerateAcw = true)]
    public abstract class RewardedCallback : RewardedAdLoadCallback
    {
        protected RewardedCallback(System.IntPtr handle, global::Android.Runtime.JniHandleOwnership transfer)
            : base(handle, transfer)
        {
        }

        protected RewardedCallback()
        {
        }

        [global::Android.Runtime.Register("onAdLoaded", "(Lcom/google/android/gms/ads/rewarded/RewardedAd;)V", "GetOnAdLoadedHandler")]
        public virtual void OnAdLoaded(RewardedAd rewardedAd) 
        { 
        }

        private static System.Delegate cb_onAdLoaded;
        private static System.Delegate GetOnAdLoadedHandler()
        {
            if (cb_onAdLoaded == null)
                cb_onAdLoaded = global::Android.Runtime.JNINativeWrapper.CreateDelegate((System.Action<System.IntPtr, System.IntPtr, System.IntPtr>)n_onAdLoaded);
            return cb_onAdLoaded;
        }

        private static void n_onAdLoaded(System.IntPtr jnienv, System.IntPtr native__this, System.IntPtr native_p0)
        {
            var thisobject = global::Java.Lang.Object.GetObject<RewardedCallback>(jnienv, native__this, global::Android.Runtime.JniHandleOwnership.DoNotTransfer);
            var resultobject = global::Java.Lang.Object.GetObject<RewardedAd>(native_p0, global::Android.Runtime.JniHandleOwnership.DoNotTransfer);
            if (thisobject != null)
            {
                thisobject.OnAdLoaded(resultobject);
            }
        }
    }

    [global::Android.Runtime.Preserve(AllMembers = true)]
    public class MyRewardedAdLoadCallback : RewardedCallback
    {
        private readonly System.Action<RewardedAd> _onLoaded;
        private readonly System.Action<LoadAdError> _onFailed;

        public MyRewardedAdLoadCallback(System.IntPtr handle, global::Android.Runtime.JniHandleOwnership transfer)
            : base(handle, transfer)
        {
        }

        public MyRewardedAdLoadCallback(System.Action<RewardedAd> onLoaded, System.Action<LoadAdError> onFailed)
        {
            _onLoaded = onLoaded;
            _onFailed = onFailed;
        }

        public override void OnAdLoaded(RewardedAd rewardedAd)
        {
            base.OnAdLoaded(rewardedAd);
            _onLoaded?.Invoke(rewardedAd);
        }

        public override void OnAdFailedToLoad(LoadAdError error)
        {
            base.OnAdFailedToLoad(error);
            _onFailed?.Invoke(error);
        }
    }

    [global::Android.Runtime.Preserve(AllMembers = true)]
    public class MyOnUserEarnedRewardListener : Java.Lang.Object, IOnUserEarnedRewardListener
    {
        private readonly System.Action _onEarned;

        public MyOnUserEarnedRewardListener(System.IntPtr handle, global::Android.Runtime.JniHandleOwnership transfer)
            : base(handle, transfer)
        {
        }

        public MyOnUserEarnedRewardListener(System.Action onEarned)
        {
            _onEarned = onEarned;
        }

        public void OnUserEarnedReward(IRewardItem rewardItem)
        {
            _onEarned?.Invoke();
        }
    }

    [global::Android.Runtime.Preserve(AllMembers = true)]
    public class MyRewardedFullScreenCallback : FullScreenContentCallback
    {
        private readonly System.Action _onDismissed;
        private bool _called;

        public MyRewardedFullScreenCallback(System.IntPtr handle, global::Android.Runtime.JniHandleOwnership transfer)
            : base(handle, transfer)
        {
        }

        public MyRewardedFullScreenCallback(System.Action onDismissed)
        {
            _onDismissed = onDismissed;
        }

        public override void OnAdDismissedFullScreenContent()
        {
            base.OnAdDismissedFullScreenContent();
            Cleanup();
        }

        public override void OnAdFailedToShowFullScreenContent(AdError error)
        {
            base.OnAdFailedToShowFullScreenContent(error);
            Cleanup();
        }

        private void Cleanup()
        {
            if (!_called)
            {
                _called = true;
                _onDismissed?.Invoke();
            }
        }
    }
#pragma warning restore CS0618
}
