using System;
using System.IO;
using System.Diagnostics;
using SpaceDodger.Core;
using SpaceDodger.Graphics;
using SpaceDodger.Input;

namespace SpaceDodger.Desktop
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
                "SpaceDodger");

        public IInputProvider CreateInputProvider(VirtualScreen screen) =>
            new KeyboardInputProvider(screen);

        public void OpenUrl(string url) =>
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
