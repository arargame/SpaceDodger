# Boss Encounter Pacing, Boss Weapon Resistance, and Dividing Splitter Spec

## 1. Supply & Power-Up Drop Rate Tuning (15%)

### Background & Balance Problem
In later levels (approaching Level 30+), enemy density, bullet hell patterns, and player pressure ramp up significantly. A flat 2% base supply drop rate starved players of critical supplies, shields, and weapon tiers, causing unfair attrition.

### Implementation
- Updated `GameConfig.PowerUpDropChance` from `0.02f` (2%) to `0.15f` (15%).
- Enemies destroyed in combat now have a generous 15% baseline chance to spawn power-up capsules (distinct W2–W5 weapon upgrades, shields, rapid fire, bombs, cartridges, and emergency health).
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

## 3. Boss Weapon Resistance (Normal Bullet Damage Cap)

### Background & Balance Problem
Special weapons—especially Tesla Chain Lightning, Sweep Lasers, and Wave Pulses—could deal devastating burst damage to bosses. Specifically, Chain Lightning would repeatedly bounce back and forth between a boss and adjacent targets, applying multiple high-damage hits and melting 60–140 HP bosses in seconds.

### Implementation (`Enemy.cs` & `ChainLightning.cs`)
1. **Damage Resistance Cap (`Enemy.TakeDamage`):**
   ```csharp
   if (IsBoss && amount > 1)
   {
       amount = 1;
   }
   ```
   All weapon impacts against a boss are resisted down to the damage of a standard player bullet (1 damage per hit). High-damage special weapons and plasma retain their wide coverage and screen-clearing utility against regular swarms, but cannot burst-shred bosses.
2. **Chain Lightning Boss Re-Targeting Prevention (`ChainLightning.cs`):**
   ```csharp
   if (enemy.IsBoss && recentlyStruck)
       continue;
   ```
   A single discharge of Tesla Chain Lightning will strike a boss at most **once** per shot, preventing rapid cyclical bouncing.

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
