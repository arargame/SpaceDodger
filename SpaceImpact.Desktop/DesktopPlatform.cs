using System;
using System.IO;
using System.Diagnostics;
using SpaceImpact.Core;
using SpaceImpact.Graphics;
using SpaceImpact.Input;

namespace SpaceImpact.Desktop
{
    /// <summary>
    /// Desktop implementation of <see cref="IPlatformServices"/>.
    /// Provides keyboard+mouse input and a save directory under AppData.
    /// </summary>
    public sealed class DesktopPlatform : IPlatformServices
    {
        public bool IsMobile => false;

        public string SaveDirectory =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SpaceImpact");

        public IInputProvider CreateInputProvider(VirtualScreen screen) =>
            new KeyboardInputProvider(screen);

        public void OpenUrl(string url) =>
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
