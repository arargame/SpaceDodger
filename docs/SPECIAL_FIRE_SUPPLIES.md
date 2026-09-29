# Finite Special-Fire Supplies

## Goal

Special fire must feel like an arcade reward without becoming a permanent
screen-clearing build. Each pickup is a readable tactical moment: use a finite
cartridge, deploy a short-lived guard, or save it for a dense enemy formation.
W1 remains the reliable baseline damage source; every stronger main weapon is
a finite upgrade that returns to W1 on expiry.

## Full armament balance

| Weapon / supply | Model | Duration or charges | Expiry result |
|---|---|---:|---|
| W1 Single Shot | Baseline | Unlimited | Never expires |
| W2 Double Shot | Timed main upgrade | random 15–24 seconds (+20-25%) | Returns to W1 |
| W3 Spread Shot | Timed main upgrade | random 12–22 seconds (+20-22%) | Returns to W1 |
| W4 Heavy Spread | Timed main upgrade | random 10–18 seconds (+20-25%) | Returns to W1 |
| W5 Storm / diffuse fire | Timed main upgrade | random 9–24 seconds (+20-28%) | Returns to W1 |
| Campaign damage bonus | Persistent campaign modifier | +1 damage at levels 10, 20, 30… | Applies to every player attack |
| Rapid Fire | Timed modifier | 12 seconds (+20%) | Normal fire cadence |
| Shield | Timed defence | 10 seconds (+25%) | Shield removed |
| Orbit Guard | Timed defence | random 6–18 seconds (+20%), 3–4 guards | Orbit shots removed |
| Scatter | Finite cartridge | random 12–24 shots (+20%) | Normal weapon only |
| Spiral | Finite cartridge | random 12–24 shots (+20%) | Normal weapon only |
| Ricochet | Finite cartridge | random 12–24 shots (+20%) | Normal weapon only |
| Homing | Finite cartridge | random 10–24 missiles (+25%) | Normal weapon only |
| Wave | Finite cartridge | random 10–24 pulses (+25%) | Normal weapon only |
| Sweep Laser | Finite cartridge | random 4–12 scans (+20-33%) | Normal weapon only |
| Tesla Lightning | Finite cartridge | random 15–25 charges (+25-50%) | Normal weapon only |

Main weapon pickups promote one tier and replace the previous tier timer rather
than stacking time. All active timers and finite cartridge counts carry across
normal level boundaries and resume saves. The campaign damage bonus is derived
from the current level (`floor(level / 10)`) and applies to main bullets,
special cartridges, orbit guards, Wave, and Sweep Laser.

## Cartridge rule

Scatter, Spiral, Homing, Ricochet, Wave, Sweep Laser, and Tesla Lightning are finite cartridges.
Only one cartridge is loaded at a time; a new cartridge deliberately replaces
the old one. A normal player shot can consume at most one cartridge round, and
the special cadence is throttled per type. This prevents simultaneous special
weapon stacking while preserving instant pickup feedback.

| Supply | Charges | Behaviour |
|---|---:|---|
| Scatter | random 12–24 | One radial eight-bolt burst per controlled shot |
| Spiral | random 12–24 | One rotating dual-plasma burst |
| Homing | random 10–24 | One missile that seeks the nearest live enemy |
| Ricochet | random 12–24 | One green bolt with four screen/enemy rebounds |
| Wave | random 10–24 | Growing forward-moving ring; each enemy can be hit once |
| Sweep Laser | random 4–12 | Full-height beam scans left-to-right; each enemy can be hit once |
| Tesla Lightning | random 15–25 | Jagged pixelated electric bolt leaps up to 6 times between live visible enemies; consumes 1 charge per shot |

The orbital pickup is intentionally different: it deploys 3–4 orbiting plasma
guards for a random 6–18 seconds. It is visible defence around the ship rather
than persistent automatic fire. A later orbit pickup refreshes/replaces the
current guard; it never adds another permanent ring. The HUD shows `O` plus
the active orbit count and a remaining-time bar.

## Supply pacing and level carry-over

There can be up to 8 active supply crates anywhere on screen (`MaximumActivePowerUps = 8`). Supply sources are:

- **Enemy drops:** Every destroyed non-boss enemy rolls a flat, unthrottled **2% chance** (`PowerUpDropChance = 0.02f`) to drop a supply/power-up crate, driven by a non-repeating draw-bag cycle (Weapon Upgrade 17.5% weight, remaining 13 supplies sharing 82.5% equally). Enemy death rolls are unthrottled by cooldown timers so multiple enemies dying in close succession each roll their independent 2% drop chance.
- **Supply drift:** A sparse right-to-left ambient supply drift every 11–17 seconds (throttled by the 3.5-second `_powerUpDropCooldown`), adjusted by the director's drop modifier.

Collected weapon tiers, special cartridge charges, timed buffs, and the remaining orbit guard are carried into the next
level and persisted in the resume save. Intro and level-clear screens disable
auto-fire so finite cartridges cannot be spent while no enemies are present.
Uncollected crates remain level-local visual entities and are cleaned up when
the next playfield is created.

Boss kills and retry starts are explicit recovery exceptions: each boss always
spawns exactly one Health crate, and a retry begins with one Health crate
plus one random offensive-supply crate entering from the right. These mandatory crates bypass the normal shared
cooldown/cap so they cannot be accidentally suppressed by an existing crate. Additionally, procedural Fat Drifter enemies roll a 50% chance upon destruction to drop a Health supply crate (or fall back to the standard 10% supply roll).

## Pooling and collision ownership

All special effects use preallocated `EntityPool<T>` instances owned by
`EntityFactory`:

- `RicochetBullets` and `OrbitShots` use normal collision resolution.
- `WavePulses` and `SweepLasers` own a pooled hit set so each enemy is damaged
  once per effect rather than once per frame while overlapping.
- Effects clear target references and hit sets on pool release.

No special weapon allocates in its frame update loop. Pickup artwork remains in
the generated `powerups.png` strip, using the existing ten-pixel framed pixel
capsule style.

## Player feedback

The normal HUD shows the active cartridge's initial and remaining charges. The
pickup toast shows the exact obtained count. Shield and rapid-fire effects also
roll a visible 5–15 second duration instead of using a short fixed timer. The `powerups.png` strip now has
four additional frames for Ricochet, Orbit, Wave, and Sweep Laser; its enum
order is kept exactly aligned with the frame order.

## Save compatibility

The resumable run state now saves `resumeSpecialFire` and
`resumeSpecialCharges`. Older timer-based Scatter/Spiral save fields are simply
ignored when loading; existing level, life, weapon, shield, and rapid-fire
progress remain valid.
