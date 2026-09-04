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
    "fighter":  ["straight", "sine", "chase", "chase"],
    "wasp":     ["zigzag", "sine", "chase"],
    "mine":     ["straight", "sine"],
    "bomber":   ["straight", "sine"],
    "seeker":   ["chase", "sine", "zigzag", "chase"],
    "lancer":   ["straight", "chase"],
    "shielder": ["straight", "sine"],
    "spinner":  ["sine", "zigzag", "straight"],
    "raider":   ["chase", "sine", "straight", "chase"],
    "turret":   ["straight", "sine"],
    "hulk":     ["straight", "chase"],
}

# how many of a species make a sensible wave (light fodder vs heavies)
BULK = {
    "drone": (8, 16), "scout": (6, 14), "fighter": (5, 10), "wasp": (6, 12),
    "mine": (4, 8), "bomber": (2, 5), "seeker": (4, 8), "lancer": (4, 9),
    "shielder": (2, 5), "spinner": (3, 7), "raider": (4, 8),
    "turret": (2, 5), "hulk": (1, 3),
}

BOSSES = {
    10: "boss_warden",
    20: "boss_hydra",
    30: "boss_titan",
    40: "boss_core",
    50: "boss_nemesis",
    60: "boss_sentinel",
    70: "boss_serpent",
    80: "boss_leviathan",
    90: "boss_phantom",
    100: "boss_oblivion",
}

BOSS_MOVEMENTS = {
    "boss_warden": "boss",
    "boss_hydra": "boss",
    "boss_titan": "boss",
    "boss_core": "boss",
    "boss_nemesis": "boss",
    "boss_sentinel": "drift_boss",
    "boss_serpent": "sine_boss",
    "boss_leviathan": "lerp_boss",
    "boss_phantom": "orbit_boss",
    "boss_oblivion": "lerp_boss",
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
    "SHADOW GATE", "DARK NEBULA", "FROZEN ORBIT", "PLASMA STORM", "VOID WALKER",
    "WARP RIFT", "STELLAR DUST", "IRON FORTRESS", "PULSE WAVE", "SENTINEL",
    "DEEP SPACE", "ASTEROID BELT", "LUNAR BASE", "SOLAR WINDS", "COSMIC RAY",
    "DARK MATTER", "GRAVITY WELL", "NEUTRON STAR", "PULSAR", "SERPENT",
    "NOVA BURST", "GAMMA RAY", "SINGULARITY", "EVENT HORIZON II", "WARP DRIVE",
    "HYPERSPACE", "STAR GATE", "WORMHOLE", "QUASAR", "LEVIATHAN",
    "SUPERNOVA", "BLACK HOLE", "WHITE DWARF", "RED GIANT", "BLUE STRAGGLER",
    "BROWN DWARF", "ORION ARM", "ANDROMEDA", "MILKY WAY", "PHANTOM",
    "CYGNUS", "LYRA", "DRACO", "CASSIOPEIA", "PEGASUS",
    "CENTAURUS", "AQUILA", "URSA MAJOR", "CANIS MAJOR", "OBLIVION"
]


def available(level):
    return [s for s, lv in UNLOCK.items() if lv <= level]


def scaling(level):
    """Health / speed multipliers, kept player-friendly for fun arcade blasting."""
    hp = round(1.0 + (level - 1) * 0.025, 2)
    spd = round(min(1.0 + (level - 1) * 0.010, 1.7), 2)
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

    # Fast-paced wave count and snappy intervals
    wave_count = min(4 + level // 6, 8)
    gap = max(2.6, 4.4 - level * 0.02)
    interval_scale = max(0.40, 1.0 - level * 0.009)

    waves = []
    t = 0.8

    # Favour recent unlocks while keeping fodder for satisfying screen clearing
    recent = sorted(pool, key=lambda s: UNLOCK[s])[-6:]

    for _ in range(wave_count):
        species = random.choice(recent if random.random() < 0.60 else pool)
        lo, hi = BULK[species]
        count = random.randint(lo, hi)
        count = max(2, int(round(count * (0.85 + level * 0.008))))

        waves.append(wave(
            t, species,
            random.choice(MOVES[species]),
            count,
            random.uniform(0.18, 0.40) * interval_scale,
            random.choice(FORMATIONS),
            hp, spd))
        t += gap

    if is_boss:
        boss_time = round(t * 0.60, 1)
        boss_name = BOSSES[level]
        waves.append(wave(
            boss_time, boss_name, BOSS_MOVEMENTS.get(boss_name, "boss"), 1, 1.0, "Column",
            round(1.0 + (level // 10 - 1) * 0.08, 2), 1.0))

        candidates = [s for s in recent if s not in ("hulk", "turret")] or recent
        escort = random.choice(candidates)
        waves.append(wave(
            boss_time + 4.5, escort, random.choice(MOVES[escort]),
            max(4, 5 + level // 10), 0.5, "Diagonal", hp, spd))

    return {"number": level, "name": NAMES[level - 1], "waves": waves}


total_enemies = 0
print(f"{'lvl':>3}  {'name':<16} {'waves':>5} {'foes':>5}  species")
for level in range(1, 101):
    data = build(level)
    with open(os.path.join(OUT, f"level{level:02d}.json"), "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2)

    foes = sum(w["count"] for w in data["waves"])
    total_enemies += foes
    species = sorted({w["enemy"] for w in data["waves"]})
    mark = "  *BOSS*" if level in BOSSES else ""
    print(f"{level:3d}  {data['name']:<16} {len(data['waves']):5d} {foes:5d}  "
          f"{', '.join(species)}{mark}")

print(f"\n100 levels written, {total_enemies} enemies total.")
