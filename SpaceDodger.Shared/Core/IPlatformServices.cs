using SpaceDodger.Graphics;
using SpaceDodger.Input;

namespace SpaceDodger.Core
{
    /// <summary>
    /// Abstraction over platform differences (Dependency Inversion Principle).
    /// The shared game never references a concrete platform; Desktop and Android
    /// each supply their own implementation at the composition root.
    /// </summary>
    public interface IPlatformServices
    {
        bool IsMobile { get; }

        /// <summary>Writable directory for save files on this platform.</summary>
        string SaveDirectory { get; }

        IInputProvider CreateInputProvider(VirtualScreen screen);

        IGameServices CreateGameServices() => NullGameServices.Instance;

        void OpenUrl(string url) { }
        void PurchaseConsumable(string productId) { }
        void PurchaseNonConsumable(string productId) { }
        void RestorePurchases() { }
        void ExitGame() { }
        bool IsInterstitialAdReady() => false;
        void ShowInterstitialAd(System.Action onClosed) { onClosed?.Invoke(); }
    }
}
