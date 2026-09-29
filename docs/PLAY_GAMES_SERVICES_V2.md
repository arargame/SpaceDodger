# Google Play Games Services V2 & Quad Weapon Technical Documentation

## 1. Google Play Console V1 Sunset Warning Analysis

### The Warning
In Google Play Console under **Play Games Services > Configuration**, developers see the banner:
> *"Games v1 SDK and APIs will soon be removed. Play Games Services V1 will shut down in May 2027. Migrate to V2 to prevent your game from being affected."*

### Blocked & Paint Trek Status
- Both `Blocked.Android` and `PaintTrek.Android` are **already built on Play Games Services V2**:
  - NuGet: `Xamarin.GooglePlayServices.Games.V2 (119.0.0)`
  - Initialization: `PlayGamesSdk.Initialize(activity);`
  - Authentication: 0-click automatic background sign-in via `PlayGames.GetGamesSignInClient(activity).IsAuthenticated()`
  - Leaderboards: `PlayGames.GetLeaderboardsClient(activity).SubmitScoreImmediate()` and `GetAllLeaderboardsIntent()`
- **Conclusion:** There is **no breaking code change needed** in Blocked or Paint Trek. The warning in the console is informational; once the checklist step *"Add the Play Games Services SDK to your APK to use the APIs"* is verified by Google Play Console on production/internal release tracks, it satisfies all V2 requirements well ahead of May 2027.

---

## 2. Space Dodger Play Games Services V2 Integration

### Architecture (SOLID & Seam Isolation)
1. **Core Seam (`SpaceDodger.Core.IGameServices`):**
   - Clean interface shared across Desktop and Android.
   - `SubmitHighScore(long score)`
   - `SubmitHighestLevel(int level)`
   - `ShowLeaderboards()`
2. **Desktop Platform:**
   - Uses `NullGameServices.Instance` ensuring 0 Android/Google dependencies on PC.
3. **Android Platform (`SpaceDodger.Droid.AndroidPlayGamesService`):**
   - Implements `IGameServices` using `Xamarin.GooglePlayServices.Games.V2 (119.0.0)`.
   - Automatic 0-click Play Games initialization on startup.
   - Immediate score submission with automatic fallback to offline caching (`SubmitScore`).
   - Opens the official Google Play Games Leaderboards UI overlay.

### Configuration & Identifiers
- **Manifest Metadata (`SpaceDodger.Android/Properties/AndroidManifest.xml`):**
  ```xml
  <meta-data
      android:name="com.google.android.gms.games.APP_ID"
      android:value="@string/app_id" />
  ```
- **Strings (`SpaceDodger.Android/Resources/values/strings.xml`):**
  ```xml
  <string name="app_name">Space Dodger</string>
  <string name="app_id">82944435164</string>
  ```
- **Leaderboard ID (`SpaceDodger.Droid.AndroidPlayGamesService`):**
  - Production configured: `CgkI3Mf-_rQCEAIQAQ` (Global High Score).

### User Flow
1. **In-Game Scoring (`GameplayScreen.cs`):**
   - Points tracked in the top-left HUD (`_score.Score`).
   - Submitted to Google Play Games whenever a local best run is updated AND definitively upon `GameOver` / `Victory`.
2. **Leaderboard Viewing (`HighScoreScreen.cs`):**
   - On the High Scores screen, tapping **"WORLD RANKING"** calls `Context.Games.ShowLeaderboards()`.
   - Opens Google Play Games native overlay displaying the global player rankings.

---

## 3. Quad Enemy Weapon System (50% Chance)

### Mechanics
- Added `EnemyWeapon.Quad` to `EnemyDefinition`.
- Configured `spinner` enemy to use `EnemyWeapon.Quad`.
- In `EntityFactory.SpawnEnemyShot`:
  - When firing, rolls `_random.NextDouble() < 0.5`:
    - **50% Chance:** Fires 4 fanned projectiles at angles `[-0.42 rad, -0.14 rad, +0.14 rad, +0.42 rad]`.
    - **50% Chance:** Fires 1 straight aimed projectile.
- Maintains balanced difficulty alongside the 50% 3-spread raiders and turrets.
