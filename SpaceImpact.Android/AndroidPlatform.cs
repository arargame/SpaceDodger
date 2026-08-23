using Android.Content;
using Android.Net;
using SpaceImpact.Core;
using SpaceImpact.Graphics;
using SpaceImpact.Input;

namespace SpaceImpact.Droid
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

        public void RequestBack() => _input?.RequestBack();

        public void OpenUrl(string url)
        {
            var intent = new Intent(Intent.ActionView, Uri.Parse(url));
            intent.AddFlags(ActivityFlags.NewTask);
            _context.StartActivity(intent);
        }
    }
}
