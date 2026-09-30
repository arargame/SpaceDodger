# Player Health-Driven Sprite & Grid Animation Specification

## 1. Overview
In Space Dodger, the player spaceship visual state dynamically reflects the ship's operational health (`Player.Lives`). When the ship has maximum armor/shields (`Lives > 2`, i.e. 3+ lives), it operates in **Boosted Mode** featuring an animated plasma propulsion engine trail. When degraded to normal or critical health (`Lives <= 2`), the ship sheds its booster modules and displays the classic compact single-engine fighter craft.

---

## 2. Asset Specifications

| Characteristic | Normal / Damaged Ship | Boosted Ship |
| :--- | :--- | :--- |
| **Texture Path** | `sprites/player.png` | `sprites/player_boosted.png` |
| **Sheet Dimensions** | 40 x 14 px (Horizontal strip) | 140 x 34 px (Grid layout) |
| **Grid Layout** | 2 columns x 1 row | 4 columns x 2 rows |
| **Frame Size** | 20 x 14 px | 35 x 17 px (20% scaled down) |
| **Frame Count** | 2 frames | 8 frames |
| **Frame Rate** | 10 FPS | 18 FPS |
| **Trigger Condition** | `Lives <= 2` | `Lives > 2` |

---

## 3. Shmup Hitbox & Fair Play Geometry

In shoot 'em ups, aesthetic thruster trails extending behind the ship must never register incoming projectile hits (hurtbox clipping).

### Boosted Mode (35 x 17 px)
- **Local Origin:** `(17.5, 8.5)` centered on `Player.Position`.
- **Exhaust Plume:** Extends backwards from `Position.X - 17.5` to `Position.X` relative to center. **Hurtbox is excluded here.**
- **Fuselage & Wings:** Spans `Position.X` to `Position.X + 17`.
- **Active Hurtbox:** `Rectangle((int)(Position.X + 2), (int)(Position.Y - 4), 13, 8)`
  - Protects the player from false-positive deaths when dodging bullets through narrow gaps while trailing booster flames.

### Classic Mode (20 x 14 px)
- **Active Hurtbox:** `CenteredRect(20, 14).Inflate(-5, -4)` yielding a 10 x 6 px core hit area.

---

## 4. Alignment & Projectile Coordination

### Muzzle Position
The projectile spawn position dynamically adjusts to the active frame's leading edge:
$$\text{MuzzlePosition} = \left( \text{Position.X} + \frac{\text{CurrentAnimation.FrameWidth}}{2}, \text{Position.Y} \right)$$
- **Normal Ship:** Bullets spawn at $\text{Position.X} + 10$.
- **Boosted Ship:** Bullets spawn at $\text{Position.X} + 17.5$ directly at the forward laser cannons.

### Shield Bubble Centering
The energy shield orb (26 x 26 px) centers over the cockpit:
- When `Lives > 2`, shield offset is shifted forward by $+8.5\text{ px}$ on the X axis to cover the main cabin rather than the rear exhaust nozzle.

---

## 5. Architectural Implementation

### Grid Atlas Engine (`SpaceDodger.Shared/Graphics/Animation.cs`)
The `Animation` class was extended to support arbitrary 2D grid sheets:
```csharp
public Animation(Texture2D texture, int frameCount, int columns, int rows, float fps, bool loop = true)
{
    Texture = texture;
    FrameCount = frameCount;
    Columns = columns > 0 ? columns : frameCount;
    Rows = rows > 0 ? rows : 1;
    FrameWidth = texture.Width / Columns;
    FrameHeight = texture.Height / Rows;
    FramesPerSecond = fps;
    Loop = loop;
}

public Rectangle FrameRect(int index)
{
    int col = index % Columns;
    int row = (index / Columns) % Rows;
    return new Rectangle(col * FrameWidth, row * FrameHeight, FrameWidth, FrameHeight);
}
```

### State Switching (`SpaceDodger.Shared/Entities/Player.cs`)
```csharp
public bool IsBoosted => Lives > 2 && _boostedAnimation != null;
public Animation CurrentAnimation => IsBoosted ? _boostedAnimation : _normalAnimation;

private void SyncAnimation()
{
    var target = CurrentAnimation;
    if (_player.Animation != target)
    {
        _player.Play(target);
    }
}
```
State synchronization is invoked on `Reset`, `Revive`, `RestoreProgress`, `AddLife`, `TakeHit`, and in `Update`.

---

## 6. Supply Magnetic Capture Field

Adapted from the magnetic capture physics in the **Blocked** project (`Blocked.Shared/Entities/Supplies/Supply.cs`), pickups in Space Dodger actively draw toward the player ship when in range.

### Physics Tuning (`SpaceDodger.Shared/Core/GameConfig.cs`)
- **Capture Radius (`SupplyMagnetRadius`):** $60\text{ px}$ (~33% of the virtual vertical resolution of 180px).
- **Attraction Acceleration (`SupplyMagnetAccel`):** $240\text{ px/s}^2$ directed toward the center of the player ship.
- **Terminal Speed (`SupplyMagnetMaxSpeed`):** $120\text{ px/s}$ ensuring supplies catch up even during aggressive evasive maneuvers.
- **Damping Rate (`SupplyMagnetDamping`):** $3.5\text{ s}^{-1}$ smoothly relaxing the trajectory back to natural leftward drift ($-30\text{ px/s}$) if the ship darts out of range.

### Visual Feedback
When captured in the tractor field (`IsBeingPulled = true`), the supply pulses with a high-frequency cyan radiance shimmer, providing instant audiovisual confirmation to the pilot.
