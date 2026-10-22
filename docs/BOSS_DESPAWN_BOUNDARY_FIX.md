# Boss Premature Despawn & Movement Boundary Fix

## 1. Bug Description & Symptoms
- **Reported Issue:** On **Level 60** and **Level 90**, bosses appeared on screen and suddenly vanished/died within seconds or after a couple of shots without taking fatal damage, instantly granting the player a stage victory and advancing the level.
- **Root Cause Analysis:**
  1. In `SpaceDodger.Shared/Entities/Enemy.cs`, line 149 contained a generic boundary check intended for standard fodder waves:
     ```csharp
     if (Position.X < _world.Left - _animation.FrameWidth ||
         Position.Y > _world.Bottom + _animation.FrameHeight)
         Deactivate();
     ```
  2. Level 60 (`boss_sentinel`) uses `DriftMovement.cs`. Its mathematical amplitude was configured as:
     ```csharp
     float driftY = 120f * (float)Math.Sin(enemy.Age * 0.5f) + 60f * (float)Math.Sin(enemy.Age * 0.3f);
     ```
     This produced a peak `driftY` of up to `180px`. In the game's `320x180` native resolution (playfield height: 168px, center Y: 96px), `96 + 180 = 276px`. This drove the boss far below `_world.Bottom + FrameHeight` (210px), calling `Deactivate()`.
  3. Level 90 (`boss_phantom`) uses `OrbitMovement.cs`. Its vertical orbital radius was set to `orbitRadiusY = 150f`. In a 180px-high screen, `96 + 150 = 246px`, which again crossed the bottom despawn threshold `_world.Bottom + FrameHeight` (210px) around ~3.3 seconds into the fight, triggering `Deactivate()`.
  4. Once `Deactivate()` was invoked, `_factory.Enemies.CountActive` dropped to 0, which immediately satisfied `WaveSpawner.IsCleared == true` and triggered `GameplayScreen`'s level victory sequence.

## 2. Implemented Fixes

### A. Defensive Boundary Guard (`Enemy.cs`)
Boss entities must NEVER be despawned by off-screen exit checks under any circumstances. They can only be defeated when their health reaches zero (`Health <= 0`):
```csharp
// Standard waves leave through the left edge; procedural dive and
// flock enemies leave below the playfield. Both paths award no score.
// Bosses must NEVER be despawned by off-screen boundary checks.
if (!IsBoss && (Position.X < _world.Left - _animation.FrameWidth ||
    Position.Y > _world.Bottom + _animation.FrameHeight))
{
    Deactivate();
}
```

### B. Scaled Multi-Harmonic Drift Pattern (`DriftMovement.cs`)
- Reduced `ParkOffset` from 180px to 70px (parks near X = 250).
- Scaled `driftX` and `driftY` harmonics to fit within native playfield coordinates without clipping.
- Added `MathHelper.Clamp` ensuring `enemy.Position` strictly stays within visible, shootable bounds.

### C. Scaled Elliptical Orbit Pattern (`OrbitMovement.cs`)
- Reduced `ParkOffset` from 150px to 72px.
- Adjusted `orbitRadiusX = 36f` and `orbitRadiusY = 46f` (keeps vertical sweep comfortably within Y: 48..144).
- Added `MathHelper.Clamp` enforcing safe playfield bounds at all times.
