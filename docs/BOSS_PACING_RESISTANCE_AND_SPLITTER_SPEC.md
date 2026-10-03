# Boss Encounter Pacing, Boss Weapon Resistance, and Dividing Splitter Spec

## 1. Supply & Power-Up Drop Rate Tuning (5%)

### Background & Balance Problem
In later levels (approaching Level 30+), enemy density, bullet hell patterns, and player pressure ramp up significantly. A flat 2% base supply drop rate starved players of critical supplies, while 15% proved too generous. A tuned 5% drop chance creates optimal arcade tension, ensuring drops feel genuinely rewarding and valuable without cluttering the playfield.

### Implementation
- Updated `GameConfig.PowerUpDropChance` to `0.05f` (5%).
- Enemies destroyed in combat now have a 5% baseline chance to spawn power-up capsules (distinct W2–W5 weapon upgrades, shields, rapid fire, bombs, cartridges, and emergency health).
- DDA (Dynamic Difficulty Adjustment) continues to modulate the health bias within dropped supplies when the player is low on lives.

---

## 2. Boss Encounter Pacing: Sequential 10-Second Swarms

### Background & Balance Problem
Previously in boss battles (levels 10, 20, 30, 40, etc.), authored minion waves fired concurrently with boss entry according to fixed timestamps. This created chaotic situations where multiple dense minion formations (e.g. 9 wasps and 6 bombers) dumped simultaneously onto the screen during bullet-hell boss phases, making dodging nearly impossible.

### Implementation (`WaveSpawner.cs`)
- **Boss Active Detection:** `IsBossActive()` monitors the live entity pool for active bosses.
- **Concurrent Wave Suppression:** While a boss is active, parallel wave timestamps are suspended.
- **10-Second Cadence (`_bossAddTimer`):**
  - When the boss enters the screen, `_bossAddTimer` initializes to 10.0 seconds.
  - Exactly once every 10.0 seconds, ONE sequential minion wave arrives.
  - If authored waves remain for that level, the next authored minion wave is spawned cleanly.
  - If authored waves have finished but the boss is still alive, a disciplined 3-ship escort formation (fighters or wasps) arrives every 10 seconds.
- **Outcome:** The player can focus cleanly on boss bullet hell and telegraphs, while managing predictable 10-second support waves without screen-filling swarm clutter.

---

## 3. Boss Damage Balance & Health Progression Tuning

### Background & Balance Problem
Initially, all boss damage was capped to 1 per hit (`if (IsBoss && amount > 1) amount = 1;`) to prevent burst melting from special weapons. However, in late campaign progression (Level 40+), this had unintended game-breaking side effects:
- The persistent campaign `Player.DamageBonus` (earned every 10 levels, e.g. +4 damage at Level 40) was completely nullified against bosses.
- High-tier weapon upgrades (W2 through W5) dealt the exact same 1 damage as a starter pea-shooter.
- Level 40 Boss (Core) required 236 individual bullet hits, and Level 100 required 1,548 hits, creating exhausting and unfair encounters.

### Resolution & Implementation
1. **Removal of Artificial 1-Damage Cap (`Enemy.TakeDamage`):**
   - The hardcap was removed from `Enemy.cs`. Player weapon damage and earned `DamageBonus` now fully apply to bosses, making weapon upgrades and level progression rewarding during boss battles.
2. **Chain Lightning Protection (`ChainLightning.cs`):**
   - Tesla Chain Lightning retains its strict anti-melt guard:
     ```csharp
     if (enemy.IsBoss && recentlyStruck)
         continue;
     ```
     A single discharge strikes a boss at most **once**, preventing rapid back-and-forth ping-pong damage while allowing intended damage.
3. **Rebalanced Arcade Boss Health Progression (`EnemyDefinition.cs` & `gen_levels.py`):**
   - Boss waves no longer apply secondary multipliers (`healthMultiplier = 1.0`), ensuring that the values defined in `EnemyCatalog` are the exact in-game spawn HP.
   - Smooth, accessible arcade progression curve:
     - Level 10 (Warden): **50 HP** (was 60)
     - Level 20 (Hydra): **75 HP** (was 103)
     - Level 30 (Titan): **100 HP** (was 162)
     - Level 40 (Core): **130 HP** (was 236)
     - Level 50 (Nemesis): **170 HP** (was 343)
     - Level 60 (Sentinel): **220 HP** (was 476)
     - Level 70 (Serpent): **275 HP** (was 651)
     - Level 80 (Leviathan): **330 HP** (was 874)
     - Level 90 (Phantom): **390 HP** (was 1,148)
     - Level 100 (Oblivion): **450 HP** (was 1,548)

---

## 4. The Dividing Splitter Enemy (`splitter`)

### Overview & Visual Identity
- **Visuals:** Large, cellular/bio-mechanical carrier with a military khaki-green hull and a vivid glowing yellow fission core (`RAMPS['khaki']`).
- **Dimensions:** 26x20 px (generated via `tools/gen_sprites.py` into `Content/sprites/enemy_splitter.png`).
- **Pacing:** Chunky, deliberate drift speed (`Speed = 32f`). Enters from the right, travels straight across the playfield towards the left.

### Mitosis & Division Mechanics
- **Initial Health:** 8 HP (4x standard enemy baseline of 2 HP).
- **Split Interval:** Every 2.0 seconds while on screen, the splitter divides into 2 daughter cells.
- **Generation Cascade (Up to 3 Generations):**
  - **Generation 0 (Parent):** 8 HP, Scale `1.0f` (26x20 px).
  - **Generation 1 (2 cells):** 4 HP each, Scale `0.78f` (~20x16 px).
  - **Generation 2 (4 cells):** 2 HP each, Scale `0.60f` (~15x12 px).
  - **Generation 3 (8 cells):** 1 HP each, Scale `0.48f` (~12x10 px).
- **Strict Lane Separation (Anti-Clipping):**
  When splitting, the parent deactivates with an energetic spark effect (`_factory.SpawnSpark`), and two child cells spawn in distinct vertical lanes:
  ```csharp
  float separation = (parent.SplitGeneration == 0) ? 36f : (parent.SplitGeneration == 1 ? 24f : 16f);
  float halfSep = separation / 2f;
  float centerY = MathHelper.Clamp(parent.Position.Y, topLimit + halfSep, bottomLimit - halfSep);
  float upperY = centerY - halfSep;
  float lowerY = centerY + halfSep;
  ```
  This guarantees that child cells never overlap, never clip into each other, and stay strictly within safe playfield bounds.
- **Spawning Channels:**
  - **Procedural Drifter (`UpdateSplitterDrifter`):** Arrives every 22–34 seconds in standard campaign levels.
  - **Authored Levels:** Added to `tools/gen_levels.py` with unlock at Level 15 onwards across all 100 levels.
