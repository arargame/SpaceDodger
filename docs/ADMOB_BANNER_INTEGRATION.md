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

The banner is placed in the in-game HUD top bar centered in the gap between the **Score** readout on the left and the **Level Number (`LVN`)** centered at the screen middle:

| Space | Virtual Coordinates (320x180) | Physical Conversion |
| --- | --- | --- |
| **Score Readout** | X: 3 .. 45, Y: 3 | Left bound (7 digits, 42px width, vertically centered) |
| **Banner Ad Area** | **X: 58, Y: 2, Width: 80, Height: 10** | `Context.Screen.ToPhysical(virtualRect)` (Vertically centered inside 14px header) |
| **Level Indicator** | X: 148 .. 172 (Center: 160), Y: 3 | Right bound (vertically centered) |
| **Combo Streak** | Mobile: X: 3, Y: 16 / Desktop: X: 48, Y: 3 | Below score on mobile to avoid overlap |

This layout features:
- **14 Virtual Pixels Header Strip:** Tinted dynamically to match the current level's ambient cosmic nebula color (`Color.Lerp(CurrentCosmicColor, Color.Black, 0.40f)`), with a subtle 1px dividing border at Y: 13.
- **Vertical Centering:** Banner is positioned at `Y: 2` with `Height: 10`, leaving 2 pixels of symmetric padding above and below inside the 14px header bar, eliminating top-edge crowding.
- **Horizontal Centering:** An 80 virtual pixel width centered at `X: 58` (spans 58..138), perfectly flanked by 13 virtual pixels of symmetric padding between the Score and Level indicators.

## 4. Android Layout & Exact Scaling

- `MainActivity` wraps the game `SurfaceView` inside a `FrameLayout` (`_rootLayout`).
- `_bannerContainer` is positioned at the exact physical coordinates determined by `VirtualScreen.ToPhysical`.
- `_bannerContainer.SetClipChildren(false)` and `SetClipToPadding(false)` are set to prevent Android view clipping.
- `AdView` is initialized with standard `AdSize.Banner` (320x50 dp) and assigned its native measured layout parameters (`wPixels x hPixels`).
- Exact scaling is applied using top-left pivot (`PivotX = 0f`, `PivotY = 0f`):
  - `ScaleX = (float)targetWidth / wPixels`
  - `ScaleY = (float)targetHeight / hPixels`
- This ensures the banner fits the designated slot exactly from start to end without vertical drift, centering squashing, or lateral dead zones, while Android automatically maps touch dispatch correctly.

## 5. Screen Lifecycle Integration

- **Shown:** During active `GameplayScreen` when `Context.Platform.IsMobile == true` and `Context.Save.Data.AdsRemoved == false`.
- **Hidden:** Automatically hidden:
  - During `PauseScreen` (both in `PauseScreen.Load()` and `GameplayScreen` pause transition).
  - During Fullscreen Rewarded Ads (`MainActivity.ShowRewardedAd`).
  - During Player death / `Phase.GameOver` / `SecondChanceScreen`.
  - When exiting `GameplayScreen.Unload()` to Main Menu (`MenuScreen`), High Scores, Level Select, or Game Over (`GameOverScreen`).
- **Resumed:** Automatically restored when resuming back into active gameplay from pause or revive.
- **IAP Suppression:** Purchasing "Remove Ads" immediately hides the banner overlay and disables future banner requests.
