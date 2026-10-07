# Space Dodger - Player Invulnerability & Boss Battle Supply System

**Document Version:** 1.0  
**Date:** October 7, 2026  
**Status:** Implemented & Verified  

---

## 1. Overview & Objectives

This specification covers two critical arcade pacing and survivability adjustments requested for Space Dodger:
1. **Extended Post-Damage Mercy Window (Flicker / Invulnerability):** Doubled the player's damage immunity and visual flicker duration from 2.0 seconds to 4.0 seconds to prevent immediate chain damage and death spirals.
2. **Boss Battle Dynamic Supply Spawns:** Introduced a periodic supply drift during active boss encounters (every 20 seconds with a 20% probability) entering from the right side of the screen.

---

## 2. Technical Architecture & Changes

### 2.1 Player Invulnerability & Visual Flicker (`GameConfig.cs`, `Player.cs`)

* **Configuration:**
  * `GameConfig.PlayerInvulnTime` increased from `2.0f` to `4.0f` seconds (+100%).
* **Mercy Frame Behavior:**
  * When the player takes damage (`TakeHit`), `_invulnTimer` is initialized to `GameConfig.PlayerInvulnTime` (4.0s).
  * During shield breaks, the grace buffer (`PlayerInvulnTime * 0.5f`) automatically scales from 1.0s to 2.0s.
  * In `Player.Draw()`, the 10 Hz blink cadence `(int)(_invulnTimer * 10f) % 2 == 0` remains active for the full 4.0s duration.
  * During the entire 4.0s interval, `IsInvulnerable` (`_invulnTimer > 0f`) prevents any bullet or collision damage.

### 2.2 Boss Battle Periodic Supply Drops (`WaveSpawner.cs`, `EntityFactory.cs`)

* **Configuration (`GameConfig.cs`):**
  * `BossSupplyCheckInterval = 20.0f;` (cadence for periodic rolls).
  * `BossSupplyChance = 0.20f;` (20% trigger chance per cycle).
* **Spawner Logic (`WaveSpawner.cs`):**
  * Added `_bossSupplyTimer` initialized to `BossSupplyCheckInterval`.
  * `UpdateBossSupplyDrift(dt)` is evaluated each frame in `WaveSpawner.Update()`.
  * Checks `IsBossActive()`:
    * If no boss is active, timer does not tick.
    * If a boss is active, `_bossSupplyTimer` counts down.
    * When `_bossSupplyTimer <= 0`, it resets to 20.0s and performs a random roll against `BossSupplyChance` (20%).
    * On success, invokes `_factory.TrySpawnBossSupplyDrift(new Vector2(_world.Bounds.Right + 10f, RandomLane()), _director.HealthSupplyBias)`.
* **Factory Method (`EntityFactory.cs`):**
  * `TrySpawnBossSupplyDrift` checks `PowerUps.CountActive < GameConfig.MaximumActivePowerUps`.
  * Bypasses standard enemy drop cooldowns (`_powerUpDropCooldown`) so boss aid is never throttled by trash mob drop timers.
  * Drifts across the virtual playfield from right to left using standard pickup magnetics (`PowerUp`).

---

## 3. Verification & Build Results

* **SpaceDodger.Desktop:** Compiled cleanly with 0 Errors (`Release net8.0`).
* **SpaceDodger.Android:** Compiled cleanly with 0 Errors (`Release net9.0-android36.0`).
* **AutoVersion Validation:** Automatically generated monotonic version code `610070523`.
