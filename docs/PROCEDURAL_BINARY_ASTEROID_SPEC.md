# Procedural Binary Orbiting Asteroid Hazards Specification

## 1. Overview
This document specifies the architecture, mathematics, asset generation, and gameplay mechanics of the procedural binary orbiting asteroid hazard system introduced in SpaceDodger.

Asteroids spawn procedurally across non-boss levels in synchronized pairs. Rather than following static linear trajectories, the asteroid pair locks into a mutual orbital dance (binary helix orbit) around a shared drifting center of mass. Each asteroid requires exactly 2 hits to destroy. If the player dodges them, they cleanly exit through the left edge of the playfield and despawn without awarding score or triggering effects.

---

## 2. Core Mechanics & Design Pillars

| Property | Value | Notes |
| :--- | :--- | :--- |
| **Entity Key** | `"asteroid"` | Defined in `EnemyCatalog` |
| **Pair Count** | 2 | Spawned concurrently with opposing phase offsets |
| **Health** | 2 HP | Takes exactly 2 hits from standard player bullets to destroy |
| **Movement Strategy** | `BinaryOrbitMovement` | Implements `IMovementStrategy` (Flyweight / Strategy pattern) |
| **Drift Speed** | 38 px/s | Horizontal leftward translation |
| **Orbit Radius** | 18 px | Separation distance between orbiting bodies (~36px diameter) |
| **Angular Speed** | 2.8 rad/s | ~0.45 rev/sec (~2.24s per full rotation) |
| **Score Value** | 30 | Awarded only when shot down; no score if dodged |
| **Weapon** | `EnemyWeapon.None` | Pure hazard (no firing bullets) |
| **Contact Damage** | 1 | Damages player on hull collision |
| **Spawn Cadence** | Every 18s - 28s | Initial spawn at 9s - 17s during non-boss waves |

---

## 3. Mathematical Movement Model

Each entity calculates its position in `BinaryOrbitMovement.Move(enemy, world, dt)` as a function of the shared virtual center of mass and angular phase offset:

$$X_{center}(t) = \text{SpawnX} - \text{EffectiveSpeed} \times t$$
$$Y_{center} = \text{SpawnY}$$

$$\theta(t) = t \times \omega + \phi$$

$$X(t) = X_{center}(t) + R \cos(\theta(t))$$
$$Y(t) = \text{clamp}(Y_{center} + R \sin(\theta(t)), Y_{min}, Y_{max})$$

Where:
- $t$: `enemy.Age` (accumulated active lifetime in seconds)
- $\omega$: Angular speed ($2.8\text{ rad/s}$)
- $R$: Orbit radius ($18\text{ px}$)
- $\phi$: Phase offset ($0$ for Asteroid A, $\pi$ for Asteroid B)

### Visual Trajectory
Because $\phi_B = \phi_A + \pi$:
$$\cos(\theta + \pi) = -\cos(\theta)$$
$$\sin(\theta + \pi) = -\sin(\theta)$$
The two asteroids remain permanently antipodal across the center of mass $(X_{center}, Y_{center})$. To the player, they appear as a rotating celestial pair traversing the screen in a double-helix configuration.

### Decoupling & Fault-Tolerance
Because the orbital trajectory is computed purely from flyweight parameters and per-entity state (`SpawnX`, `SpawnY`, `Age`), destroying one asteroid causes **zero disruption** to the surviving partner. The surviving asteroid smoothly continues its orbital arc until destroyed or despawned.

---

## 4. Off-Screen Despawn Lifecycle
When an asteroid passes beyond the left edge of the screen ($X < \text{world.Left} - \text{FrameWidth}$):
- `Enemy.Update(dt)` checks boundary conditions.
- `Deactivate()` returns the entity to the `EntityPool<Enemy>`.
- No `Destroyed` event is fired; no score is awarded; no drops occur.
- Natural dodging is fully rewarded through survival.

When shot by player projectiles:
- First hit: Asteroid health drops from 2 to 1, entity flashes red (`_hitFlash = 0.08f`), sound/impact triggers.
- Second hit: Asteroid health drops from 1 to 0, invokes `Destroyed?.Invoke(this)`, spawns explosion particle effect, awards 30 points to score, rolls standard powerup drop chance, and deactivates entity.

---

## 5. Architectural Integration (SOLID / OOP / DRY)

### Open/Closed Principle (OCP)
- `BinaryOrbitMovement` implements `IMovementStrategy` without modifying existing movement patterns.
- Registered via `MovementRegistry` under `"binary_orbit_a"` and `"binary_orbit_b"`. Can be leveraged in authored level JSON as well as procedural spawners.

### Single Responsibility Principle (SRP)
- `BinaryOrbitMovement`: Responsible solely for orbital coordinate calculation.
- `WaveSpawner`: Responsible for procedural timing and invoking entity factory.
- `gen_sprites.py`: Responsible for generating authentic retro pixel-art silhouettes and palette ramps.

### Asset Pipeline
- Sprite `enemy_asteroid.png` (28x14 px, 2 frames of 14x14) generated with stony craggy silhouette using the `brown` palette ramp (`RAMPS['brown']`).
- Frame 1 and Frame 2 incorporate crater depth accents (`A` marks) and tumbling contours to provide natural rotation animation.
