using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using SpaceImpact.Core;

namespace SpaceImpact.Droid
{
    /// <summary>Android entry point hosting the shared MonoGame game.</summary>
    [Activity(
        Label = "Space Impact",
        MainLauncher = true,
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        ConfigurationChanges =
            ConfigChanges.Orientation | ConfigChanges.Keyboard |
            ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class MainActivity : AndroidGameActivity
    {
        private SpaceImpactGame _game;
        private View _view;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _game = new SpaceImpactGame(new AndroidPlatform(this));
            _view = _game.Services.GetService(typeof(View)) as View;

            SetContentView(_view);
            _game.Run();
        }
    }
}
