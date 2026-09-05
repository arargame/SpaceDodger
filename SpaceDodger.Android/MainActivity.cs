using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
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

        private SpaceDodgerGame _game;
        private AndroidPlatform _platform;
        private View _view;

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
                SetContentView(_view);
                _view.Focusable = true;
                _view.FocusableInTouchMode = true;
                _view.RequestFocus();
            }

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

        protected override void OnDestroy()
        {
            base.OnDestroy();

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
}
