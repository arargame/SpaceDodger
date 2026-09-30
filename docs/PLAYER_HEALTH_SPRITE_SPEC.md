# Player Health-Driven Sprite & Grid Animation Specification

## 1. Overview
In Space Dodger, the player spaceship visual state dynamically reflects the ship's operational health (`Player.Lives`). When the ship has extra armor/shields/lives (`Lives > 1`), it operates in **Boosted Mode** featuring an animated plasma propulsion engine trail. When degraded to critical emergency health (`Lives == 1`), the ship sheds its booster modules and displays the classic compact single-engine fighter craft.

---

## 2. Asset Specifications

| Characteristic | Normal / Critical Ship | Boosted Ship |
| :--- | :--- | :--- |
| **Texture Path** | `sprites/player.png` | `sprites/player_boosted.png` |
| **Sheet Dimensions** | 40 x 14 px (Horizontal strip) | 140 x 34 px (Grid layout) |
| **Grid Layout** | 2 columns x 1 row | 4 columns x 2 rows |
| **Frame Size** | 20 x 14 px | 35 x 17 px (20% scaled down) |
| **Frame Count** | 2 frames | 8 frames |
| **Frame Rate** | 10 FPS | 18 FPS |
| **Trigger Condition** | `Lives <= 1` | `Lives > 1` |

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
- When `Lives > 1`, shield offset is shifted forward by $+8.5\text{ px}$ on the X axis to cover the main cabin rather than the rear exhaust nozzle.

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
public bool IsBoosted => Lives > 1 && _boostedAnimation != null;
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
