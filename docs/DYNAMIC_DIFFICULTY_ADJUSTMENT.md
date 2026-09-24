# Space Dodger Adaptive Threat Director

## Purpose

Space Dodger keeps its authored JSON waves as the primary encounter design.
The Adaptive Threat Director adds a second, procedural pacing layer while an
authored level is still running. It is intentionally more reactive than the
Blocked DDA: strong play quickly earns a short reward window and then a sharp
but bounded Hunter Surge. The intended rhythm is reward, pressure, recovery,
and flow rather than a flat difficulty curve.

## Architecture

`Difficulty/` owns a small strategy seam:

```
GameplayScreen -> AdaptiveThreatDirector -> WaveSpawner -> EntityFactory
                                  |                 |
                           debug HUD          procedural enemies
```

`GameplayScreen` provides player telemetry (lives, combo, active threat count,
recent damage and kills). `WaveSpawner` reads only the director modifiers. This
keeps enemy movement, pooling, collision, and data-driven authored waves
independent from adaptive balancing.

The director is created once per run, not once per level. At normal level
boundaries it preserves clean-run time, kill heat, reward/surge phase, and its
current state. A player entering the next level as `GODLIKE` therefore receives
continued pressure instead of an artificial return to `IN FLOW`. Boss levels
still suppress procedural reinforcements while their authored choreography is
active.

## States and response

| State | Trigger | Result |
|---|---|---|
| `RECOVERING` | One life left, a hit in the last 3.5 seconds, or 18+ hostile bullets | Strong slowdown, lower health, high drops, 4-enemy cap |
| `STRUGGLING` | A lost life, a hit in the last 8 seconds, or sustained screen pressure | Moderated speed and spawn rate, increased drops |
| `IN FLOW` | Stable play | Authored baseline modifiers |
| `DOMINATING` | High combo, long clean survival, and sustained kill heat | Faster, denser reinforcements |
| `GODLIKE` | Exceptional combo/survival/kill heat | Aggressive health, speed, fire-rate, and reinforcement pressure |

The response is evaluated every 0.75 seconds and modifiers interpolate every
frame. This makes recovery feel immediate without turning visible values into
hard jumps.

Health supplies are not a universal reward: their selection bias is 48% in
`RECOVERING`, 24% in `STRUGGLING`, 2% in `IN FLOW`, and 0% when dominating or
godlike. The global supply cap/cooldown remains active, so assistance does not
become a loot flood.

## Reward / punishment cycle

When `DOMINATING` or `GODLIKE` is reached, the director alternates between:

1. `REWARD WINDOW` for 3.5 seconds: a temporary supply-rich breathing space earned by strong play.
2. `HUNTER SURGE` for 8.5 seconds: high spawn pressure, quick enemies, sparse drops.

Taking damage cancels the cycle and returns control to the recovery states.
Boss levels use `BOSS LOCK`: no procedural reinforcements are added, preserving
their authored choreography.

## Procedural encounter patterns

| Pattern | Entry | Motion | Purpose |
|---|---|---|---|
| Flank interceptor | Right edge | Leftward sine/chase | Familiar baseline pressure |
| Top dive | Above the playfield | Downward weave | Breaks horizontal-only avoidance habits |
| Diagonal flock | Upper left or right | To opposite lower corner | Short, readable formation that forces a lane decision |

Enemy selection progresses with the level number and uses existing species and
pooling. No new sprites, allocations, or platform-specific behavior are needed.

## Debug validation

Debug builds show a compact overlay beneath the normal HUD:

- state: `IN FLOW`, `DOMINATING`, `GODLIKE`, etc.
- cycle: `STEADY`, `REWARD WINDOW`, `HUNTER SURGE`
- intensity, active enemy/bullet counts, speed/health/drop modifiers

`GameConfig.ShowDifficultyDebug` is compiled as `true` only in Debug builds;
release builds contain no DDA monitor.
