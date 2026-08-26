using System;
using SpaceDodger.Core;

namespace SpaceDodger.Desktop
{
    /// <summary>Desktop (Windows/Linux/macOS via DesktopGL) entry point.</summary>
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            using (var game = new SpaceDodgerGame(new DesktopPlatform()))
            {
                game.Run();
            }
        }
    }
}
