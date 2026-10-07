using Android.Content;
using Android.Net;
using SpaceDodger.Core;
using SpaceDodger.Graphics;
using SpaceDodger.Input;

namespace SpaceDodger.Droid
{
    /// <summary>
    /// Android implementation of <see cref="IPlatformServices"/>.
    /// Provides touch input and an app-private save directory.
    /// </summary>
    public sealed class AndroidPlatform : IPlatformServices
    {
        private readonly Context _context;
        private TouchInputProvider _input;

        public AndroidPlatform(Context context) => _context = context;

        public bool IsMobile => true;

        public string SaveDirectory => _context.FilesDir.AbsolutePath;

        public IInputProvider CreateInputProvider(VirtualScreen screen) =>
            _input = new TouchInputProvider(screen);

        private AndroidPlayGamesService _gameServices;

        public IGameServices CreateGameServices()
        {
            if (_gameServices == null)
            {
                var activity = _context as Android.App.Activity ?? MainActivity.Instance;
                if (activity != null)
                    _gameServices = new AndroidPlayGamesService(activity);
            }
            return _gameServices ?? (IGameServices)NullGameServices.Instance;
        }

        public void RequestBack() => _input?.RequestBack();

        public void OpenUrl(string url)
        {
            var intent = new Intent(Intent.ActionView, Uri.Parse(url));
            intent.AddFlags(ActivityFlags.NewTask);
            _context.StartActivity(intent);
        }

        public void ExitGame() => (_context as MainActivity ?? MainActivity.Instance)?.SafeExit();

        public void PurchaseConsumable(string productId) =>
            (_context as MainActivity ?? MainActivity.Instance)?.PurchaseProduct(productId, isConsumable: true);

        public void PurchaseNonConsumable(string productId) =>
            (_context as MainActivity ?? MainActivity.Instance)?.PurchaseProduct(productId, isConsumable: false);

        public void RestorePurchases() =>
            (_context as MainActivity ?? MainActivity.Instance)?.RestorePurchases();

        public bool IsInterstitialAdReady() =>
            (_context as MainActivity ?? MainActivity.Instance)?.IsInterstitialReady() ?? false;

        public void ShowInterstitialAd(System.Action onClosed)
        {
            var activity = _context as MainActivity ?? MainActivity.Instance;
            if (activity != null && activity.IsInterstitialReady())
                activity.ShowInterstitialAd(onClosed);
            else
                onClosed?.Invoke();
        }

        public void ShowBannerAd(int x, int y, int width, int height) =>
            (_context as MainActivity ?? MainActivity.Instance)?.ShowBannerAd(x, y, width, height);

        public void HideBannerAd() =>
            (_context as MainActivity ?? MainActivity.Instance)?.HideBannerAd();

        public bool IsRewardedAdReady() =>
            (_context as MainActivity ?? MainActivity.Instance)?.IsRewardedAdReady() ?? false;

        public void LoadRewardedAd() =>
            (_context as MainActivity ?? MainActivity.Instance)?.LoadRewardedAd();

        public void ShowRewardedAd(System.Action onRewardEarned, System.Action onClosed = null)
        {
            var activity = _context as MainActivity ?? MainActivity.Instance;
            if (activity != null)
                activity.ShowRewardedAd(onRewardEarned, onClosed);
            else
                onClosed?.Invoke();
        }

        public bool IsSecondChanceAdReady() =>
            (_context as MainActivity ?? MainActivity.Instance)?.IsSecondChanceAdReady() ?? false;

        public void LoadSecondChanceAd() =>
            (_context as MainActivity ?? MainActivity.Instance)?.LoadSecondChanceAd();

        public void ShowSecondChanceAd(System.Action onRewardEarned, System.Action onClosed = null)
        {
            var activity = _context as MainActivity ?? MainActivity.Instance;
            if (activity != null)
                activity.ShowSecondChanceAd(onRewardEarned, onClosed);
            else
                onClosed?.Invoke();
        }
    }
}
