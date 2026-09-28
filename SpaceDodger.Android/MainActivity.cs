using Android.App;
using Android.Content.PM;
using Android.Gms.Ads;
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
#else
        private const string BannerAdUnitId = "ca-app-pub-3062759184051966/8797177752"; // Space Dodger Production Banner ID
#endif

        private SpaceDodgerGame _game;
        private AndroidPlatform _platform;
        private View _view;
        private FrameLayout _rootLayout;
        private FrameLayout _bannerContainer;
        private AdView _adView;
        private bool _isBannerVisible;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Instance = this;

            // Ekranı oyun sırasında açık tut
            Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
            EnableImmersiveMode();

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
            RunOnUiThread(() =>
            {
                if (productId == ArarGames.Core.Applications.ArarGamesApplications.RemoveAdsProductId)
                {
                    try
                    {
                        var context = _game?.Context;
                        if (context != null)
                        {
                            context.Save.Data.AdsRemoved = true;
                            context.Save.Save();
                            HideBannerAd();
                        }
                    }
                    catch { }
                }
            });
        }

        public void RestorePurchases()
        {
            RunOnUiThread(() =>
            {
                try
                {
                    var context = _game?.Context;
                    if (context != null)
                    {
                        context.Save.Save();
                    }
                }
                catch { }
            });
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
}
