# Space Dodger - Firebase Analytics & Telemetry Specification

## 1. Overview & Architecture

Space Dodger implements a clean, decoupled, high-performance telemetry pipeline designed to capture actionable player behavior without impacting 60 FPS gameplay or increasing cold-start times.

### Architectural Separation (SOLID - Dependency Inversion)
- **`SpaceDodger.Shared/Analytics/IAnalyticsService.cs`**: The domain contract defining telemetry operations. The game engine never references the Android or Firebase namespaces directly.
- **`SpaceDodger.Shared/Analytics/NullAnalyticsService.cs`**: Null Object pattern implementation. Used on Desktop and during local testing; ensures game code runs seamlessly without null checks.
- **`SpaceDodger.Shared/Analytics/AnalyticsEvents.cs`**: Single source of truth (DRY) for all GA4 event names, parameter keys, user property keys, and standard values.
- **`SpaceDodger.Shared/Analytics/AnalyticsManager.cs`**: Facade providing typed, validated methods. Employs a thread-safe parameter pool to eliminate heap garbage allocations during intense gameplay.
- **`SpaceDodger.Android/Services/FirebaseAnalyticsService.cs`**: Concrete Android adapter wrapping Google's `FirebaseAnalytics` SDK.

### Performance & Threading Principles
1. **Zero Cold-Start Lag:** Firebase SDK initialization occurs on a background thread (`Task.Run`) inside `FirebaseAnalyticsService.Attach(Context)`. The main thread and MonoGame's `Run()` launch without waiting.
2. **Zero In-Game Stutter:** Parameter bundles are populated via an internal static dictionary pool (`_paramPool`). No heap allocations occur when frequent items (like supplies) are picked up.
3. **Crash-Free Guarantee:** Every telemetry call is encapsulated in defensive exception handlers. Telemetry issues can never crash or interrupt gameplay.

---

## 2. Firebase Project & `google-services.json` Setup

To connect Space Dodger to your Firebase project:
1. Go to [Firebase Console](https://console.firebase.google.com/).
2. Select your project (or create a new one, e.g., `space-dodger-app`).
3. Add an Android app with Package Name:
   ```text
   com.arargames.spacedodger
   ```
4. App Nickname: `Space Dodger`
5. Download `google-services.json` and place it directly into the project directory:
   ```text
   c:\Users\ararg\source\AIRepos\SpaceImpact\SpaceDodger.Android\google-services.json
   ```
6. The MSBuild system is configured with `Condition="Exists('google-services.json')"`:
   - If the file exists, it is automatically processed and compiled into Android resources.
   - If absent during development, the solution still compiles cleanly with `NullAnalyticsService`.

---

## 3. Telemetry Event Catalog

All event names conform to GA4 limits (max 40 characters, alphanumeric + underscores).

| Event Name | Trigger Location | Parameters | Purpose |
| :--- | :--- | :--- | :--- |
| **`screen_view`** | `ScreenManager.Push` | `screen_name`<br>`screen_class` | Automatically tracks user navigation across every screen (`MenuScreen`, `GameplayScreen`, `PauseScreen`, `GameOverScreen`, `SecondChanceScreen`, etc.). |
| **`level_started`** | `GameplayScreen.StartLevel` | `level_number`<br>`lives_remaining`<br>`weapon_level`<br>`is_boss_level` | Measures session depth, drop-off rates, and initial loadout per level. |
| **`level_completed`** | `GameplayScreen.UpdatePlaying` | `level_number`<br>`score`<br>`duration_seconds`<br>`lives_remaining`<br>`combo_max` | Progression metrics, level completion difficulty, and scoring distributions. |
| **`level_failed`** | `GameplayScreen.OnPlayerDied` | `level_number`<br>`score`<br>`duration_seconds`<br>`killed_by`<br>`weapon_level` | Identifies difficulty spikes, player choke points, and causes of death. |
| **`level_5_reached`** | `GameplayScreen.UpdatePlaying` | `score`<br>`duration_seconds` | **Primary Google Ads Conversion Milestone.** Triggers when Level 5 is completed / Level 6 is unlocked. Optimized for App Campaigns (tCPA). |
| **`supply_collected`** | `GameplayScreen.CollectPowerUps` | `supply_type`<br>`level_number`<br>`lives_remaining`<br>`score`<br>`is_boosted_skin` | Measures utility & crate pickup frequency, magnetic collection efficacy, and health states. |
| **`weapon_collected`** | `GameplayScreen.CollectPowerUps` | `weapon_type`<br>`level_number` | Tracks specific weapon upgrades (e.g. `Weapon3`, `Wave`, `ChainLightning`, `SweepLaser`). |
| **`boss_fight_result`** | `GameplayScreen.OnEnemyDestroyed` / `OnPlayerDied` | `boss_level`<br>`boss_name`<br>`is_victory`<br>`duration_seconds`<br>`damage_taken` | Measures boss difficulty, battle duration, and win/loss ratios (LV10, LV20, LV30...). |
| **`second_chance_offered`** | `SecondChanceScreen.Load` | `level_number`<br>`score` | Tracks how often players are presented with a rewarded ad revive opportunity. |
| **`second_chance_used`** | `SecondChanceScreen.WatchAd` / `GiveUp` | `level_number`<br>`revive_accepted` | Rewarded ad conversion and player retention metrics. |

---

## 4. Firebase Console Custom Definitions (Özel Tanımlar)

To visualize these parameters in Firebase Analytics reports, funnel explorations, and BigQuery, register them in:
**Firebase Console > Analytics > Custom Definitions (Özel Tanımlar)**

### 4.1 Custom Dimensions (Özel Boyutlar)

| Dimension Name (Boyut Adı) | Kapsam (Scope) | Açıklama | Event Parameter / Property |
| :--- | :--- | :--- | :--- |
| **Screen Name** | Event | Ziyaret edilen oyun ekranı | `screen_name` |
| **Screen Class** | Event | Ekran sınıfı tipi | `screen_class` |
| **Killed By** | Event | Oyuncunun ölüm sebebi (`enemy_bullet`, `boss_collision`, vb.) | `killed_by` |
| **Supply Type** | Event | Toplanan erzak türü (`Health`, `Shield`, `Bomb`, vb.) | `supply_type` |
| **Weapon Type** | Event | Kuşanılan özel silah (`Spread`, `Laser`, `Tesla`, vb.) | `weapon_type` |
| **Boss Name** | Event | Karşılaşılan boss adı | `boss_name` |
| **Is Boosted Skin** | Event | Sandık alındığında yeni güçlendirilmiş gemi devrede miydi | `is_boosted_skin` |
| **Has Passed Level 5** | User | Kullanıcı Level 5'i geçti mi (Reklam hedef kitleleri için) | `has_passed_level_5` |
| **Max Level Reached** | User | Kullanıcının ulaştığı en yüksek seviye | `max_level_reached` |

### 4.2 Custom Metrics (Özel Metrikler)

| Metric Name (Metrik Adı) | Kapsam (Scope) | Ölçü Birimi (Unit) | Event Parameter |
| :--- | :--- | :--- | :--- |
| **Level Number** | Event | Standard (Sayı) | `level_number` |
| **Boss Level** | Event | Standard (Sayı) | `boss_level` |
| **Duration Seconds** | Event | Seconds (Saniye) | `duration_seconds` |
| **Remaining Lives** | Event | Standard (Sayı) | `lives_remaining` |
| **Weapon Level** | Event | Standard (Sayı) | `weapon_level` |
| **Max Combo** | Event | Standard (Sayı) | `combo_max` |

---

## 5. Testing on Samsung S22 (Debug vs Release & DebugView)

### 5.1 Debug vs. Release Modu Karşılaştırması

| Kriter | Debug Modu (Önerilen İlk Test) | Release Modu (Son Doğrulama) |
| :--- | :--- | :--- |
| **Amaç** | Anlık telemetri akışını ve event parametrelerini doğrulamak | Linker (`AndroidLinkSkip`) ve R8 shrinker'ın SDK'yı kırpmadığını doğrulamak |
| **LogCat Çıktısı** | Tam ayrıntılı (`[Analytics] Firebase Analytics ready...`) | Sadece uyarılarsa log düşer |
| **DebugView Uyumluluğu** | ✅ Tam destek | ✅ Tam destek |
| **Hata Tespiti** | Hata anında tam stack trace verir | Hızlı ve optimize çalışır |

> [!TIP]
> **Öneri:** Samsung S22 cihazınızda ilk testi **`Debug` modunda** yapınız. Event akışını Firebase DebugView üzerinden doğruladıktan sonra mağaza yayını öncesi **`Release` modunda** bir kez doğrulayınız.

---

### 5.2 Realtime Canlı İzleme (Firebase DebugView)

Normal şartlarda Firebase Analytics, mobil cihazın pilini ve ağını korumak için event'leri **1 saate kadar toplu (batch) bekletip** gönderir.
Test sırasında event'lerin anında (1-2 saniye içinde) konsola akması için **DebugView** açılmalıdır.

#### Adım 1: Samsung S22 Cihazını USB ile Bağlayın (USB Hata Ayıklama Açık)
Terminal veya CMD'den cihazın bağlı olduğunu doğrulayın:
```bash
adb devices
```

#### Adım 2: DebugView Modunu Etkinleştirin
Proje içerisindeki hazır betiği çalıştırabilir veya terminalden şu komutu verebilirsiniz:
```bash
c:\Users\ararg\source\AIRepos\SpaceImpact\SpaceDodger.Android\DeployAndFix\4_ENABLE_FIREBASE_DEBUGVIEW.bat
```
*(Arka planda çalıştırdığı komut: `adb shell setprop debug.firebase.analytics.app com.arargames.spacedodger`)*

#### Adım 3: Firebase Konsolundan Canlı İzleyin
1. Tarayıcınızda **Firebase Console**'u açın.
2. Sol menüden **Analytics > DebugView** sayfasına gidin.
3. Cihaz listesinden Samsung S22 cihazınızı seçin.
4. Oyunu açtığınızda ekranda anlık olarak:
   - `screen_view (MenuScreen)`
   - `level_started (level_number: 1, lives: 3)`
   - `supply_collected (supply_type: Health)`
   - `level_completed (level_number: 1, duration_seconds: 24.3)`
   akmaya başlayacaktır.

#### Adım 4: Test Bittiğinde DebugView'ı Kapatın
```bash
c:\Users\ararg\source\AIRepos\SpaceImpact\SpaceDodger.Android\DeployAndFix\5_DISABLE_FIREBASE_DEBUGVIEW.bat
```
*(Arka planda çalıştırdığı komut: `adb shell setprop debug.firebase.analytics.app .none.`)*
