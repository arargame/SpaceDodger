# AdMob Banner Integration

## 1. Overview & AdMob Console Status

- **App Status:** `com.arargames.spacedodger` is registered in Google AdMob and currently shows **"Requires review"**. This is the standard behavior for apps that are either in development, closed test, or internal testing. Once the app is published publicly on Google Play, linking it via *AdMob Console -> App settings -> App store details* will complete the review.
- **Application ID:** `ca-app-pub-3062759184051966~6191370725` (set in `AndroidManifest.xml`).
- **Ad Unit IDs (Debug vs Release):**
  - **Debug (Test Mode):** Uses Google's official **Test Banner Ad Unit ID** (`ca-app-pub-3940256099942544/6300978111`) to prevent invalid traffic penalties during development and testing.
  - **Release (Production):** Uses Space Dodger's production **Banner Ad Unit ID** (`ca-app-pub-3062759184051966/8797177752`).
  - Switched automatically at compile time via `#if DEBUG` in `MainActivity.cs`.

## 2. Architecture & Platform Abstraction (SOLID)

Following the Dependency Inversion Principle (DIP):
- **`IPlatformServices`:** Declares platform-agnostic methods `ShowBannerAd(int x, int y, int width, int height)` and `HideBannerAd()`.
- **Desktop (`SpaceDodger.Desktop`):** Implements default no-op methods without referencing any Android or Google Play Services libraries.
- **Android (`SpaceDodger.Android`):** Implements concrete banner loading, dynamic container placement, and lifecycle management.

## 3. Positioning & Virtual-to-Physical Coordinate Mapping

The banner is placed in the in-game HUD top bar between the **Score/Combo** readout on the left and the **Level Number (`LVN`)** centered at the screen middle:

| Space | Virtual Coordinates (320x180) | Physical Conversion |
| --- | --- | --- |
| **Score + Combo** | X: 3 .. 66, Y: 1 | Left bound |
| **Banner Ad Area** | **X: 74, Y: 1, Width: 68, Height: 10** | `Context.Screen.ToPhysical(virtualRect)` |
| **Level Indicator** | X: 148 .. 172 (Center: 160), Y: 1 | Right bound |

This layout leaves a comfortable padding margin of ~8 virtual pixels after the combo text and ~6 virtual pixels before the level indicator.

## 4. Android Layout & Auto-Scaling

- `MainActivity` wraps the game `SurfaceView` inside a `FrameLayout` (`_rootLayout`).
- `_bannerContainer` is positioned at the exact physical coordinates determined by `VirtualScreen.ToPhysical`.
- `BannerAdListener` monitors `OnAdLoaded` and scales down the `AdView` (`ScaleX` / `ScaleY`) if physical dimensions are smaller than standard banner pixels, preventing any clipping, overlap, or touch interception.
- Banner is loaded asynchronously via background thread `MobileAds.Initialize` to prevent ANR.

## 5. Screen Lifecycle Integration

- **Shown:** During active `GameplayScreen` when `Context.Platform.IsMobile == true` and `Context.Save.Data.AdsRemoved == false`.
- **Hidden:** Automatically hidden in `GameplayScreen.Unload()` when exiting to Main Menu (`MenuScreen`), High Scores, Level Select, or Game Over (`GameOverScreen`).
- **IAP Suppression:** Purchasing "Remove Ads" immediately hides the banner overlay and disables future banner requests.
