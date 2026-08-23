using Android.Content;
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

        public AndroidPlatform(Context context) => _context = context;

        public bool IsMobile => true;

        public string SaveDirectory => _context.FilesDir.AbsolutePath;

        public IInputProvider CreateInputProvider(VirtualScreen screen) =>
            new TouchInputProvider(screen);
    }
}
