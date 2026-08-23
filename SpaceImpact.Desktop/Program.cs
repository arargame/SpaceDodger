using System;
using SpaceImpact.Core;

namespace SpaceImpact.Desktop
{
    /// <summary>Desktop (Windows/Linux/macOS via DesktopGL) entry point.</summary>
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            using (var game = new SpaceImpactGame(new DesktopPlatform()))
            {
                game.Run();
            }
        }
    }
}
