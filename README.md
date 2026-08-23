# Space Impact

A pixel-art horizontal space shooter in the spirit of the Nokia classic, built with
MonoGame. One shared codebase, two heads: **Desktop (DesktopGL)** and **Android**.

```
SpaceImpact/
├── SpaceImpact.sln
├── Content/                     # raw assets, shared by both heads
│   ├── sprites/*.png            # pixel art sprite sheets + bitmap font
│   └── levels/level01..50.json  # data-driven level definitions
├── SpaceImpact.Shared/          # ALL game logic (shared project, .shproj)
├── SpaceImpact.Desktop/         # Windows/Linux/macOS entry point
└── SpaceImpact.Android/         # Android entry point
```

## Running

```bash
# Desktop
dotnet run --project SpaceImpact.Desktop

# Android (device or emulator attached)
dotnet build SpaceImpact.Android -t:Run
```

Requires the .NET 8 SDK. The Android head additionally needs the
`android` workload: `dotnet workload install android`.

## Controls

| Action | Desktop | Android |
|---|---|---|
| Move | Arrows / WASD | Drag on the left half |
| Fire | Space / J | Hold on the right half |
| Confirm | Enter / Space | Tap |
| Pause / Back | Esc / P | Hardware back button |

## Architecture

Everything below lives in `SpaceImpact.Shared` and is referenced by both heads via
the shared project (`.projitems`), so there is exactly one copy of the game.

### Platform abstraction (Dependency Inversion)

`IPlatformServices` is the only seam between shared code and the outside world:

```csharp
public interface IPlatformServices
{
    bool IsMobile { get; }
    string SaveDirectory { get; }
    IInputProvider CreateInputProvider(VirtualScreen screen);
}
```

`DesktopPlatform` and `AndroidPlatform` implement it. `SpaceImpactGame` is the
composition root — it builds every service once and injects them through
`GameContext`. No service locator, no statics holding state.

### Screen management (State pattern)

`ScreenManager` is a stack of `IScreen`. Only the top screen updates; screens
flagged `IsOverlay` (the pause menu) let the screen beneath them keep drawing.

```
MenuScreen ─┬─> GameplayScreen ──> GameOverScreen
            ├─> LevelSelectScreen        │
            └─> HighScoreScreen          └─> PauseScreen (overlay)
```

`MenuList` is the one reusable menu widget — keyboard navigation and tap targets
in a single place, so no screen re-implements it.

### Object pooling

`ObjectPool<T>` recycles anything implementing `IPoolable`. `EntityPool<T>`
extends it to drive `Update`/`Draw` and auto-reclaim entities that deactivated
themselves this frame. Bullets, enemies, explosions and powerups are all pooled
and preallocated, so a play session allocates essentially nothing per frame.

`EntityFactory` owns all five pools and is the only place that spawns things —
gameplay code says "a bullet here", never `new`.

### Strategy patterns

Enemy movement and player weapons are both strategies selected by name/level:

| Movement | Behaviour |
|---|---|
| `straight` | constant leftward drift |
| `sine` | smooth vertical wave |
| `zigzag` | sharp triangle wave |
| `chase` | homes vertically on the player |
| `boss` | enters, parks near the right edge, sweeps |

| Weapon tier | Pattern |
|---|---|
| 1 | `SingleShot` — one bolt |
| 2 | `DoubleShot` — twin parallel bolts |
| 3 | `SpreadShot` — 3-way fan |
| 4 | `HeavySpread` — 2 parallel plasma + 2 angled, double damage |
| 5 | `StormShot` — 5-way plasma fan, double damage |

Tiers rise with `Weapon` pickups and drop by one on each life lost, which keeps
a bad run from spiralling.

Strategies are stateless singletons in `MovementRegistry` / `WeaponRegistry`
(Flyweight) — per-enemy state lives on the enemy. Adding a pattern is one new
class plus one registry line; nothing existing gets modified (Open/Closed).

### Events (Observer)

`EventBus` decouples systems. `ScoreTracker` awards points by subscribing to
`EnemyDestroyedEvent` — the enemy, the spawner and the HUD never call it.
Entities expose their own C# events (`Enemy.Destroyed`, `Player.Fired`) which
pooled objects clear on release so recycled instances never leak listeners.

### Data-driven content

Enemy species live in `EnemyCatalog`; levels are pure JSON:

```json
{
  "number": 5, "name": "WARDEN",
  "waves": [
    { "startTime": 16.0, "enemy": "boss", "movement": "boss",
      "formation": "Column", "count": 1, "interval": 1.0 }
  ]
}
```

`WaveSpawner` reads that schedule and decides what spawns, when and in which
formation (`Line`, `Diagonal`, `Scatter`, `Column`). Tuning difficulty means
editing JSON, not code.

### File management & saving

`ISaveGameService` → `JsonSaveGameService` → `IStorageProvider`. Storage is
abstracted so the service is testable and platform-free; each head just supplies
a writable directory. Persisted data is the high-score table plus
`maxUnlockedLevel`.

Both save data and level/sprite loading use **manual** `System.Text.Json`
(`JsonDocument` / `Utf8JsonWriter`) rather than reflection-based serialization,
which keeps it trimming- and AOT-safe on Android. Corrupt or missing files
degrade gracefully instead of crashing.

Assets load through `TitleContainer`, which resolves from the output folder on
desktop and from APK assets on Android — the same `Content/...` path works on
both, and no MGCB content pipeline is needed.

### Rendering

The game renders into a 320×180 `RenderTarget2D` (`VirtualScreen`) and scales it
up with `SamplerState.PointClamp` and letterboxing, so pixel art stays crisp at
any resolution and touch coordinates map back cleanly via `ToVirtual`.

## Content

**13 enemy species + 5 bosses.** Species unlock gradually across the campaign,
so something new appears every few levels:

| Unlocks at | Species | Role |
|---|---|---|
| 1 / 2 / 3 | drone, scout, fighter | fodder, fast darts, first shooters |
| 5 / 6 / 7 | wasp, mine, bomber | erratic strikers, drifting hazards, heavy shells |
| 9 / 12 / 14 | seeker, lancer, shielder | homing, high-speed rammers, armoured |
| 17 / 21 / 25 | spinner, raider, turret | spread-fire hazards, 3-way raiders, gun platforms |
| 31 | hulk | 18 HP brute |

**6 pickups:** weapon tier, extra life, shield bubble, smart bomb (clears the
screen and all enemy fire), rapid fire, and a score bonus.

**50 levels**, one boss every 10:

| Level | Boss |
|---|---|
| 10 | WARDEN |
| 20 | HYDRA |
| 30 | TITAN |
| 40 | CORE BREACH |
| 50 | NEMESIS |

Health and speed multipliers, wave count and spawn density all scale with the
level number; bosses arrive mid-level with escort waves continuing around them.

## Regenerating assets

Everything in `Content/` is produced by Python + Pillow scripts in `tools/`:

| Script | Output |
|---|---|
| `gen_font.py` (+ `font_data.py`) | `sprites/font.png` |
| `gen_sprites.py` | all ship, bullet, effect and pickup sheets |
| `gen_levels.py` | `levels/level01..50.json` |

The font is hand-authored in `font_data.py` as 5x7 character grids (ASCII
32..126) rendered into 6x8 cells — the one-pixel margin is the letter spacing.

Sprites are authored as **silhouettes only** and shaded automatically. Hand-
painting shading onto a vertically mirrored sprite makes everything look like a
glowing blob, because the mirror duplicates the highlight into the centre.
Instead each shape is lit from the top — highlight along the upper edge of every
column, falling off to dark at the bottom, wrapped in a near-black outline. That
gives one consistent light direction across the whole game and lets the artwork
focus on silhouette, which is what actually makes enemies readable at 320x180.

To use your own art instead, replace the PNGs keeping the same horizontal
frame-strip layout and update the frame count in `EnemyCatalog`.

## Extending

- **New enemy**: add an entry to `EnemyCatalog` + a sprite sheet. No new class.
- **New movement**: implement `IMovementStrategy`, register it, use it in JSON.
- **New weapon tier**: implement `IWeaponPattern`, append to `WeaponRegistry`.
- **New screen**: subclass `Screen`, push it onto the `ScreenManager`.
