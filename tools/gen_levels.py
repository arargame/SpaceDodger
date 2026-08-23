#!/usr/bin/env python3
"""Generate the 50 campaign levels.

Design rules:
  * Species unlock gradually, so every few levels something new shows up.
  * Difficulty ramps with level number via health/speed multipliers, wave count
    and spawn density.
  * Every 10th level is a boss fight with escort waves.
"""
import json, os, random

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "..", "Content", "levels")
os.makedirs(OUT, exist_ok=True)

random.seed(20260721)

FORMATIONS = ["Line", "Diagonal", "Scatter", "Column"]

# species -> level it becomes available
UNLOCK = {
    "drone": 1, "scout": 2, "fighter": 3, "wasp": 5, "mine": 6,
    "bomber": 7, "seeker": 9, "lancer": 12, "shielder": 14,
    "spinner": 17, "raider": 21, "turret": 25, "hulk": 31,
}

# species -> movement patterns that suit it
MOVES = {
    "drone":    ["straight", "sine", "zigzag"],
    "scout":    ["straight", "zigzag", "sine"],
    "fighter":  ["straight", "sine", "chase"],
    "wasp":     ["zigzag", "sine", "chase"],
    "mine":     ["straight", "sine"],
    "bomber":   ["straight", "sine"],
    "seeker":   ["chase", "sine", "zigzag"],
    "lancer":   ["straight", "chase"],
    "shielder": ["straight", "sine"],
    "spinner":  ["sine", "zigzag", "straight"],
    "raider":   ["chase", "sine", "straight"],
    "turret":   ["straight", "sine"],
    "hulk":     ["straight", "chase"],
}

# how many of a species make a sensible wave (light fodder vs heavies)
BULK = {
    "drone": (6, 12), "scout": (5, 11), "fighter": (4, 8), "wasp": (4, 9),
    "mine": (3, 7), "bomber": (2, 5), "seeker": (3, 7), "lancer": (3, 7),
    "shielder": (2, 5), "spinner": (3, 6), "raider": (3, 6),
    "turret": (2, 4), "hulk": (1, 3),
}

BOSSES = {
    10: "boss_warden",
    20: "boss_hydra",
    30: "boss_titan",
    40: "boss_core",
    50: "boss_nemesis",
}

NAMES = [
    "FIRST CONTACT", "ASTEROID DRIFT", "OUTER PATROL", "MINEFIELD", "SIGNAL LOST",
    "DEBRIS RUN", "COLD ORBIT", "IRON RAIN", "BLACK TIDE", "WARDEN",
    "DEEP SCAN", "HUNTER PACK", "SHATTERED MOON", "SIEGE LINE", "STATIC STORM",
    "GHOST LANE", "CRIMSON PASS", "THE NARROWS", "SWARM PROTOCOL", "HYDRA",
    "DERELICT FIELD", "SOLAR FLARE", "RAIDER COUNTRY", "BROKEN CHORUS", "GUN LINE",
    "EVENT HORIZON", "SILENT RUNNING", "SCRAP HEAVEN", "LAST BEACON", "TITAN",
    "VOID CROSSING", "HEAVY WEATHER", "THE GRINDER", "NULL SECTOR", "ASH BELT",
    "FRACTURE", "DEAD RECKONING", "BLIND JUMP", "RED SHIFT", "CORE BREACH",
    "TERMINAL DRIFT", "STARFALL", "THE LONG DARK", "IRON CROWN", "LAST LIGHT",
    "OVERLOAD", "ZERO HOUR", "FINAL APPROACH", "THE GATE", "NEMESIS",
]


def available(level):
    return [s for s, lv in UNLOCK.items() if lv <= level]


def scaling(level):
    """Health / speed multipliers, rounded to keep the JSON tidy."""
    hp = round(1.0 + (level - 1) * 0.045, 2)
    spd = round(min(1.0 + (level - 1) * 0.016, 1.85), 2)
    return hp, spd


def wave(t, enemy, movement, count, interval, formation, hp, spd):
    d = {
        "startTime": round(t, 1), "enemy": enemy, "movement": movement,
        "formation": formation, "count": count, "interval": round(interval, 2),
    }
    if hp != 1.0:
        d["healthMultiplier"] = hp
    if spd != 1.0:
        d["speedMultiplier"] = spd
    return d


def build(level):
    pool = available(level)
    hp, spd = scaling(level)
    is_boss = level in BOSSES

    wave_count = min(3 + level // 6, 7)
    gap = max(5.5, 8.5 - level * 0.05)
    interval_scale = max(0.45, 1.0 - level * 0.011)

    waves = []
    t = 1.5

    # Favour the most recently unlocked species so new levels feel new.
    recent = sorted(pool, key=lambda s: UNLOCK[s])[-6:]

    for _ in range(wave_count):
        species = random.choice(recent if random.random() < 0.65 else pool)
        lo, hi = BULK[species]
        count = random.randint(lo, hi)
        count = max(1, int(round(count * (0.75 + level * 0.012))))

        waves.append(wave(
            t, species,
            random.choice(MOVES[species]),
            count,
            random.uniform(0.35, 0.85) * interval_scale,
            random.choice(FORMATIONS),
            hp, spd))
        t += gap

    if is_boss:
        boss_time = round(t * 0.55, 1)
        waves.append(wave(
            boss_time, BOSSES[level], "boss", 1, 1.0, "Column",
            round(1.0 + (level // 10 - 1) * 0.12, 2), 1.0))

        candidates = [s for s in recent if s != "hulk"] or recent
        escort = random.choice(candidates)
        waves.append(wave(
            boss_time + 6.0, escort, random.choice(MOVES[escort]),
            max(3, 4 + level // 12), 1.3, "Diagonal", hp, spd))

    return {"number": level, "name": NAMES[level - 1], "waves": waves}


total_enemies = 0
print(f"{'lvl':>3}  {'name':<16} {'waves':>5} {'foes':>5}  species")
for level in range(1, 51):
    data = build(level)
    with open(os.path.join(OUT, f"level{level:02d}.json"), "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2)

    foes = sum(w["count"] for w in data["waves"])
    total_enemies += foes
    species = sorted({w["enemy"] for w in data["waves"]})
    mark = "  *BOSS*" if level in BOSSES else ""
    print(f"{level:3d}  {data['name']:<16} {len(data['waves']):5d} {foes:5d}  "
          f"{', '.join(species)}{mark}")

print(f"\n50 levels written, {total_enemies} enemies total.")
