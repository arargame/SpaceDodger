# AdMob Rewarded Ad & Run Revival Integration

## 1. Overview & Ad Unit IDs

- **Feature:** When the player loses their last life in `GameplayScreen`, mobile players are offered a "Second Chance" to watch a Google AdMob Rewarded Video ad to revive with **2 lives** and continue their current run right where they left off.
- **Platform Constraint:** Desktop builds (`SpaceDodger.Desktop`) contain **NO** ad systems or ad UI whatsoever (`Context.Platform.IsMobile == false`).
- **Ad Unit IDs (Debug vs Release):**
  - **Debug (Test Mode):** Official Google Test Rewarded Ad Unit ID (`ca-app-pub-3940256099942544/5224354917`).
  - **Release (Production - Second Chance):** Space Dodger Dedicated Second Chance Ad Unit ID (`ca-app-pub-3062759184051966/9033505530`).
  - **Release (Production - General Rewarded):** Space Dodger General Rewarded Ad Unit ID (`ca-app-pub-3062759184051966/5568379185`).
  - Switched automatically at compile time via `#if DEBUG` in `MainActivity.cs`.

## 1.1 AdMob Policy Compliance Note
- **No "Watch ad to help / Support developer"**: Google AdMob strictly prohibits asking users to watch ads to "help" or "support" the developer without a tangible in-app reward (violates AdMob *Encouraging Clicks / Views* policy). Rewarded ads must grant concrete gameplay rewards (e.g., revive lives, starting boost).

## 2. Architecture & Design Patterns (SOLID / DIP / OCP)

Following Dependency Inversion and Single Responsibility Principles:
- **`IPlatformServices`:**
  - `bool IsRewardedAdReady()`: Returns whether a rewarded ad is cached in memory.
  - `void LoadRewardedAd()`: Preloads a rewarded ad in the background.
  - `void ShowRewardedAd(Action onRewardEarned, Action onClosed)`: Presents the rewarded video ad with thread-safe callbacks.
- **`DesktopPlatform`:**
  - Uses default interface methods (no-op, `IsRewardedAdReady() => false`). No references to Google Play Services or Android SDK.
- **`AndroidPlatform`:**
  - Forwards calls to `MainActivity.Instance`.
- **`SecondChanceScreen`:**
  - Overlay screen (`IsOverlay => true`) keeping the frozen `GameplayScreen` visible underneath.
  - Reuses `MenuList` for tap/touch and gamepad navigation.
  - Handles ad lifecycle states: Loading, Ad Not Completed, Ad Failed/Unavailable, and Successful Reward.
- **`Player.Revive(int lives, float invulnerabilityDuration = 3.5f)`:**
  - Restores player active state with specified lives.
  - Applies a 3.5-second mercy invulnerability window (blinking sprite).
  - Clamps player position to valid playfield bounds.

## 3. Revival Flow & Fair Play Balancing

1. **Trigger:** When player lives reach 0, after the death explosion delay (`GameOverDelay = 1.8f`), if `Context.Platform.IsMobile && !_hasUsedRevive`, `GameplayScreen` pushes `SecondChanceScreen`.
2. **One Revive Per Run:** `_hasUsedRevive` flag ensures players can only revive once per run, preventing infinite ad exploitation.
3. **Smart Bullet Clear:** On successful revival, `_factory.EnemyBullets.ReleaseAll()` clears all hostile bullets currently on the screen. This prevents unfair instant deaths upon respawning.
4. **Visual & Audio Feedback:** A white screen flash (`_bombFlash = 0.4f`), audio chime, and a HUD pickup message (`"REVIVED! +2 LIVES"`) notify the player of their renewed status.
5. **High Score Integrity:** Score and level progression remain continuous. Scores are only recorded to the high score table upon final game over (`GameOverScreen`).

## 4. Android Implementation Details (JNI Bridge)

- Uses custom `RewardedCallback` inheriting from `RewardedAdLoadCallback` with explicit JNI registration (`onAdLoaded(Lcom/google/android/gms/ads/rewarded/RewardedAd;)V`), resolving the well-known Xamarin / .NET for Android binding virtual dispatch bug.
- Implements background preloading in `OnCreate` immediately after `MobileAds.Initialize()`.
- Automatically initiates background preloading for the next ad upon ad dismissal or failure.
