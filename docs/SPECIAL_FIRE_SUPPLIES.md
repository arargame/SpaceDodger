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
| W2 Double Shot | Timed main upgrade | random 12–20 seconds | Returns to W1 |
| W3 Spread Shot | Timed main upgrade | random 10–18 seconds | Returns to W1 |
| W4 Heavy Spread | Timed main upgrade | random 8–15 seconds | Returns to W1 |
| W5 Storm / diffuse fire | Timed main upgrade | random 7–20 seconds | Returns to W1 |
| Rapid Fire | Timed modifier | random 5–15 seconds | Normal fire cadence |
| Shield | Timed defence | random 5–15 seconds | Shield removed |
| Orbit Guard | Timed defence | random 5–15 seconds | Orbit shots removed |
| Scatter | Finite cartridge | random 10–20 shots | Normal weapon only |
| Spiral | Finite cartridge | random 10–20 shots | Normal weapon only |
| Ricochet | Finite cartridge | random 10–20 shots | Normal weapon only |
| Homing | Finite cartridge | random 8–20 missiles | Normal weapon only |
| Wave | Finite cartridge | random 8–20 pulses | Normal weapon only |
| Sweep Laser | Finite cartridge | random 3–10 scans | Normal weapon only |

Main weapon pickups promote one tier and replace the previous tier timer rather
than stacking time. All active timers and finite cartridge counts carry across
normal level boundaries and resume saves.

## Cartridge rule

Scatter, Spiral, Homing, Ricochet, Wave, and Sweep Laser are finite cartridges.
Only one cartridge is loaded at a time; a new cartridge deliberately replaces
the old one. A normal player shot can consume at most one cartridge round, and
the special cadence is throttled per type. This prevents simultaneous special
weapon stacking while preserving instant pickup feedback.

| Supply | Charges | Behaviour |
|---|---:|---|
| Scatter | random 10–20 | One radial eight-bolt burst per controlled shot |
| Spiral | random 10–20 | One rotating dual-plasma burst |
| Homing | random 8–20 | One missile that seeks the nearest live enemy |
| Ricochet | random 10–20 | One green bolt with four screen/enemy rebounds |
| Wave | random 8–20 | Growing forward-moving ring; each enemy can be hit once |
| Sweep Laser | random 3–10 | Full-height beam scans left-to-right; each enemy can be hit once |

The orbital pickup is intentionally different: it deploys 2–3 orbiting plasma
guards for a random 5–15 seconds. It is visible defence around the ship rather
than persistent automatic fire. A later orbit pickup refreshes/replaces the
current guard; it never adds another permanent ring. The HUD shows `O` plus
the active orbit count and a remaining-time bar.

## Supply pacing and level carry-over

There can be at most two active supply crates anywhere on screen and every
crate creation shares a 3.5-second cooldown. Supply sources are deliberately
limited to:

- rare enemy drops (7% base chance before DDA assistance);
- a sparse right-to-left supply drift every 11–17 seconds, adjusted by the
  director's drop modifier.

The two sources use the same cap and cooldown, so a cluster of enemy kills
cannot create a wall of five pickups. Collected weapon tiers, special cartridge
charges, timed buffs, and the remaining orbit guard are carried into the next
level and persisted in the resume save. Intro and level-clear screens disable
auto-fire so finite cartridges cannot be spent while no enemies are present.
Uncollected crates remain level-local visual entities and are cleaned up when
the next playfield is created.

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
