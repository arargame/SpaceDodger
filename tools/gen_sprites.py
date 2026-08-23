#!/usr/bin/env python3
"""Pixel-art sprite generator for SpaceImpact.

Approach: author the SILHOUETTE only, then shade it automatically.

Hand-placing shading on a mirrored sprite makes everything look like a glowing
blob (the mirror duplicates the highlight into the centre). Instead each sprite
is drawn as a solid shape and lit from the TOP: the upper edge of every column
gets the highlight, the lower edge falls off to dark, and the whole silhouette
is wrapped in a near-black outline. That keeps one consistent light direction
across the whole game and lets the artwork focus on silhouette, which is what
actually makes enemies readable at 320x180.

Grid characters:
    X  hull (auto-shaded)      A  accent (cockpit/eye)     B  bright accent
    T  thruster flame          .  transparent

Enemies face LEFT, the player faces RIGHT.
"""
import os, math, random
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "..", "Content", "sprites")
os.makedirs(OUT, exist_ok=True)

TRANSPARENT = (0, 0, 0, 0)

def rgba(r, g, b):
    return (r, g, b, 255)

# Each ramp: outline, dark, mid, light, highlight, accent, bright accent.
RAMPS = {
    'steel':   dict(k=rgba(16,18,28),  d=rgba(56,64,84),   m=rgba(104,116,142),
                    l=rgba(158,170,196), h=rgba(222,232,248), a=rgba(80,205,255),  b=rgba(190,245,255)),
    'green':   dict(k=rgba(10,28,14),  d=rgba(34,84,40),   m=rgba(72,150,66),
                    l=rgba(124,202,96),  h=rgba(190,240,144), a=rgba(255,88,72),   b=rgba(255,190,150)),
    'cyan':    dict(k=rgba(8,26,34),   d=rgba(22,84,102),  m=rgba(46,148,176),
                    l=rgba(98,206,230),  h=rgba(182,242,252), a=rgba(255,228,80),  b=rgba(255,250,190)),
    'purple':  dict(k=rgba(22,10,34),  d=rgba(64,30,98),   m=rgba(112,58,164),
                    l=rgba(166,106,216), h=rgba(220,176,250), a=rgba(255,72,72),   b=rgba(255,186,186)),
    'yellow':  dict(k=rgba(42,28,6),   d=rgba(116,82,16),  m=rgba(186,142,32),
                    l=rgba(234,198,72),  h=rgba(254,240,158), a=rgba(60,36,16),    b=rgba(255,124,56)),
    'brown':   dict(k=rgba(26,14,8),   d=rgba(76,44,24),   m=rgba(126,80,44),
                    l=rgba(174,124,74),  h=rgba(216,174,120), a=rgba(255,138,36),  b=rgba(255,216,152)),
    'blue':    dict(k=rgba(8,16,40),   d=rgba(24,50,108),  m=rgba(46,92,178),
                    l=rgba(94,148,234),  h=rgba(168,204,252), a=rgba(110,255,196), b=rgba(214,255,240)),
    'red':     dict(k=rgba(34,6,10),   d=rgba(100,20,28),  m=rgba(164,44,50),
                    l=rgba(218,86,82),   h=rgba(250,162,152), a=rgba(255,218,76),  b=rgba(255,248,192)),
    'magenta': dict(k=rgba(34,8,28),   d=rgba(102,22,86),  m=rgba(168,48,140),
                    l=rgba(220,102,194), h=rgba(250,176,238), a=rgba(110,255,255), b=rgba(218,255,255)),
    'slate':   dict(k=rgba(12,14,20),  d=rgba(38,44,58),   m=rgba(72,82,102),
                    l=rgba(114,128,154), h=rgba(166,182,208), a=rgba(255,84,56),   b=rgba(255,192,142)),
    'orange':  dict(k=rgba(40,16,6),   d=rgba(118,52,14),  m=rgba(190,96,26),
                    l=rgba(238,148,58),  h=rgba(254,204,134), a=rgba(60,216,255),  b=rgba(192,246,255)),
    'toxic':   dict(k=rgba(18,32,8),   d=rgba(54,90,16),   m=rgba(102,156,28),
                    l=rgba(160,214,60),  h=rgba(218,248,144), a=rgba(232,56,222),  b=rgba(252,172,246)),
    'void':    dict(k=rgba(8,6,18),    d=rgba(32,24,56),   m=rgba(62,48,100),
                    l=rgba(100,82,152),  h=rgba(154,136,208), a=rgba(255,52,116),  b=rgba(255,172,202)),
}


def parse(rows):
    """Split a grid into a hull mask plus accent/thruster overlays."""
    h, w = len(rows), len(rows[0])
    hull = [[False] * w for _ in range(h)]
    marks = {}
    for y, row in enumerate(rows):
        assert len(row) == w, f"row {y} width {len(row)} != {w}: {row!r}"
        for x, ch in enumerate(row):
            if ch == '.':
                continue
            hull[y][x] = True
            if ch in 'ABT':
                marks[(x, y)] = ch
    return hull, marks, w, h


def shade(rows, ramp, outline=True):
    """Render a silhouette grid with top-down lighting and an outline."""
    hull, marks, w, h = parse(rows)
    pal = RAMPS[ramp]

    # Outline grows one pixel outward from the hull, so allocate a border.
    pad = 1 if outline else 0
    img = Image.new("RGBA", (w + pad * 2, h + pad * 2), TRANSPARENT)
    px = img.load()

    if outline:
        for y in range(h):
            for x in range(w):
                if not hull[y][x]:
                    continue
                for dy in (-1, 0, 1):
                    for dx in (-1, 0, 1):
                        nx, ny = x + dx, y + dy
                        inside = 0 <= nx < w and 0 <= ny < h and hull[ny][nx]
                        if not inside:
                            px[nx + pad, ny + pad] = pal['k']

    # Column extents drive the lighting: brightest at the top of each column.
    for x in range(w):
        column = [y for y in range(h) if hull[y][x]]
        if not column:
            continue
        top, bottom = column[0], column[-1]
        span = max(1, bottom - top)
        for y in column:
            t = (y - top) / span
            if t <= 0.001:
                c = pal['h']
            elif t < 0.22:
                c = pal['h']
            elif t < 0.45:
                c = pal['l']
            elif t < 0.72:
                c = pal['m']
            elif t < 0.93:
                c = pal['d']
            else:
                c = pal['k']
            px[x + pad, y + pad] = c

    # Accents sit on top of the shading.
    for (x, y), ch in marks.items():
        px[x + pad, y + pad] = pal['a'] if ch == 'A' else (
            pal['b'] if ch == 'B' else pal['a'])

    return img


def mirror_down(top, has_center=False):
    """Vertically symmetric SILHOUETTE (shading is applied afterwards)."""
    body = top[:-1] if has_center else top
    return top + body[::-1]


def strip(frames):
    w = sum(f.width for f in frames)
    h = max(f.height for f in frames)
    img = Image.new("RGBA", (w, h), TRANSPARENT)
    x = 0
    for f in frames:
        img.paste(f, (x, 0))
        x += f.width
    return img


def save(name, frames):
    img = strip(frames)
    img.save(os.path.join(OUT, f"{name}.png"))
    print(f"  {name:<18} {len(frames)} frames of {frames[0].width}x{img.height}")
    return img


def shift_rows(rows, dy):
    blank = '.' * len(rows[0])
    if dy == 0:
        return list(rows)
    if dy > 0:
        return [blank] * dy + rows[:-dy]
    return rows[-dy:] + [blank] * (-dy)


# ============================================================== PLAYER =====
# Faces RIGHT. Nose at the right, engine flame trailing left.
PLAYER = mirror_down([
    "..................",
    ".............XX...",
    "..........XXXXXX..",
    ".....XXXXXXXXXXXX.",
    "..XXXXXXXXXXXXXXXX",
    "TTXXXXXXXXXAAXXXX.",
])

def player_frame(long_flame):
    rows = [r.replace('T', 'T' if long_flame else '.') for r in PLAYER]
    if not long_flame:
        rows = [r.replace('T', '.') for r in PLAYER]
    img = shade(rows, 'steel')
    # Draw the engine flame manually so it glows instead of being hull-shaded.
    px = img.load()
    flame = [rgba(255, 246, 200), rgba(255, 196, 60), rgba(255, 120, 30)]
    cy = img.height // 2
    length = 4 if long_flame else 2
    for i in range(length):
        col = 1 + (length - 1 - i)
        if col >= img.width:
            continue
        for dy, c in ((-1, flame[2]), (0, flame[min(i, 2)]), (1, flame[2])):
            y = cy + dy
            if 0 <= y < img.height and i < 2 or dy == 0:
                px[col, y] = c
    return img

print("player:")
save("player", [player_frame(False), player_frame(True)])


# ============================================================= ENEMIES =====
# Distinct silhouettes matter more than internal detail at this size.
SHAPES = {}

# round beetle
SHAPES['drone'] = ('green', mirror_down([
    "....XXXX....",
    "..XXXXXXXX..",
    ".XXXXXXXXXX.",
    "XXXXXXXXXXXX",
    "XXAAXXXXXXXX",
]))

# tiny dart
SHAPES['scout'] = ('cyan', mirror_down([
    "......XXXX..",
    "..XXXXXXXXX.",
    "XXXXXXXXXXXX",
    "XAAXXXXXXXXX",
]))

# arrowhead interceptor
SHAPES['fighter'] = ('purple', mirror_down([
    ".......XXXX...",
    ".....XXXXXXX..",
    "...XXXXXXXXXX.",
    ".XXXXXXXXXXXXX",
    "XXXXXXXXXXXXXX",
    "XAAXXXXXXXXXXX",
], has_center=True))

# swept-wing striker
SHAPES['wasp'] = ('yellow', mirror_down([
    "..X......XXX.",
    ".XX....XXXXXX",
    "XXXXXXXXXXXXX",
    ".XXXXXXXXXXXX",
    "..XXXXXXXXXXX",
    "..AAXXXXXXXXX",
], has_center=True))

# blocky gunship
SHAPES['bomber'] = ('brown', mirror_down([
    "...XXXXXXXXX....",
    ".XXXXXXXXXXXXX..",
    "XXXXXXXXXXXXXXX.",
    "XXXXXXXXXXXXXXXX",
    "XXXXXXXXXXXXXXXX",
    "XAAXXXXXXXXXXXXX",
    "XAAXXXXXXXXXXXXX",
]))

# front armour plate + body
SHAPES['shielder'] = ('blue', mirror_down([
    ".XX......XXXX..",
    ".XX...XXXXXXXX.",
    "XXX..XXXXXXXXXX",
    "XXX.XXXXXXXXXXX",
    "XXX.XXXXXXXXXXX",
    "XXX.AAXXXXXXXXX",
]))

# needle homing craft
SHAPES['seeker'] = ('red', mirror_down([
    ".....XXXXXXX",
    "..XXXXXXXXXX",
    "XXXXXXXXXXXX",
    "XXXXXXXXXXXX",
    "XAAXXXXXXXXX",
], has_center=True))

# squat armoured platform with barrel
SHAPES['turret'] = ('slate', mirror_down([
    "....XXXXXXXX..",
    "...XXXXXXXXXX.",
    "..XXXXXXXXXXXX",
    "XXXXXXXXXXXXXX",
    "XXXXXXXXXXXXXX",
    "XXXXXXXXXXXXXX",
    "XAAXXXXXXXXXXX",
]))

# four-armed rotating hazard
SPINNER_A = [
    "..XX....XX..",
    "..XX....XX..",
    "...XXXXXX...",
    "..XXXXXXXX..",
    ".XXXXXXXXXX.",
    "XXXXAAXXXXXX",
    "XXXXAAXXXXXX",
    ".XXXXXXXXXX.",
    "..XXXXXXXX..",
    "...XXXXXX...",
    "..XX....XX..",
    "..XX....XX..",
]
SPINNER_B = [
    "............",
    "XX........XX",
    "XXXXXXXXXXXX",
    "..XXXXXXXX..",
    ".XXXXXXXXXX.",
    ".XXXAAXXXXX.",
    ".XXXAAXXXXX.",
    ".XXXXXXXXXX.",
    "..XXXXXXXX..",
    "XXXXXXXXXXXX",
    "XX........XX",
    "............",
]
SHAPES['spinner'] = ('magenta', SPINNER_A)

# big armoured brute
SHAPES['hulk'] = ('void', mirror_down([
    ".....XXXXXXXXXXX......",
    "...XXXXXXXXXXXXXXX....",
    "..XXXXXXXXXXXXXXXXXX..",
    ".XXXXXXXXXXXXXXXXXXXX.",
    "XXXXXXXXXXXXXXXXXXXXXX",
    "XXXXXXXXXXXXXXXXXXXXXX",
    "XXAAXXXXXXXXXXXXXXXXXX",
    "XXAAXXXXXXXXXXXXXXXXXX",
]))

# spiky drifting hazard
SHAPES['mine'] = ('toxic', mirror_down([
    "..X..XX..X..",
    "X.X..XX..X.X",
    ".XXXXXXXXXX.",
    "..XXXXXXXX..",
    ".XXXXXXXXXX.",
    "XXXXAABBXXXX",
]))

# long strike craft
SHAPES['raider'] = ('orange', mirror_down([
    "........XXXXXXX..",
    "...XXXXXXXXXXXXX.",
    ".XXXXXXXXXXXXXXXX",
    "XXXXXXXXXXXXXXXXX",
    "XXXXXXXXXXXXXXXXX",
    "XAAXXXXXXXXXXXXXX",
], has_center=True))

# needle-nosed rammer
SHAPES['lancer'] = ('steel', mirror_down([
    "..........XXXXXX",
    "....XXXXXXXXXXXX",
    "XXXXXXXXXXXXXXXX",
    "XXXXXXXXXXXXXXXX",
    "XAAXXXXXXXXXXXXX",
], has_center=True))

print("enemies:")
for name, (colour, shape) in SHAPES.items():
    if name == 'spinner':
        frames = [shade(SPINNER_A, colour), shade(SPINNER_B, colour)]
    else:
        # Frame 2 bobs by a pixel: cheap, readable idle animation.
        frames = [shade(shape, colour), shade(shift_rows(shape, 1), colour)]
    save(f"enemy_{name}", frames)


# ============================================================== BOSSES =====
BOSSES = {}

# WARDEN - wide blunt cruiser with a central eye
BOSSES['boss_warden'] = ('purple', mirror_down([
    ".........XXXXXXXXXXXXXX.........",
    ".......XXXXXXXXXXXXXXXXXX.......",
    ".....XXXXXXXXXXXXXXXXXXXXXX.....",
    "...XXXXXXXXXXXXXXXXXXXXXXXXXX...",
    "..XXXXXXXXXXXXXXXXXXXXXXXXXXXX..",
    ".XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX.",
    "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
    "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
    "XXXXXXXXXAAAAAAXXXXXXXXXXXXXXXXX",
    "XXXXXXXXXAABBAAXXXXXXXXXXXXXXXXX",
    "XXXXXXXXXAABBAAXXXXXXXXXXXXXXXXX",
    "XXXXXXXXXAAAAAAXXXXXXXXXXXXXXXXX",
]))

# HYDRA - central body with two forward prongs
BOSSES['boss_hydra'] = ('toxic', mirror_down([
    "XXXXX.........XXXXXXXXXXXX......",
    "XXXXXX......XXXXXXXXXXXXXXXX....",
    "XXXXXXX...XXXXXXXXXXXXXXXXXXXX..",
    ".XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX.",
    "..XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
    "...XXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
    "....XXXXXAAAAAAXXXXXXXXXXXXXXXXX",
    "...XXXXXXAABBAAXXXXXXXXXXXXXXXXX",
    "..XXXXXXXAABBAAXXXXXXXXXXXXXXXXX",
    ".XXXXXXXXAAAAAAXXXXXXXXXXXXXXXXX",
], has_center=True))

# TITAN - massive slab battleship with twin cannon ports
BOSSES['boss_titan'] = ('slate', mirror_down([
    "...XXXXXXXXXXXXXXXXXXXXXXXXXX...",
    "..XXXXXXXXXXXXXXXXXXXXXXXXXXXX..",
    ".XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX.",
    "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
    "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
    "XXXAAAAXXXXXXXXXXXXXXXXXXXXXXXXX",
    "XXXAABBXXXXXXXXXXXXXXXXXXXXXXXXX",
    "XXXAAAAXXXXXXXXXXXXXXXXXXXXXXXXX",
    "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
    "XXXAAAAXXXXXXXXXXXXXXXXXXXXXXXXX",
    "XXXAABBXXXXXXXXXXXXXXXXXXXXXXXXX",
    "XXXAAAAXXXXXXXXXXXXXXXXXXXXXXXXX",
    "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
    "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
]))

# CORE - ring hull around an exposed reactor
BOSSES['boss_core'] = ('red', mirror_down([
    ".......XXXXXXXXXXXXXXXXX........",
    "....XXXXXXXXXXXXXXXXXXXXXXX.....",
    "..XXXXXXXXXXXXXXXXXXXXXXXXXXX...",
    ".XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX.",
    "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
    "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
    "XXXXXAAAAAAAAAAXXXXXXXXXXXXXXXXX",
    "XXXXAABBBBBBBBAAXXXXXXXXXXXXXXXX",
    "XXXAABBBBBBBBBBAAXXXXXXXXXXXXXXX",
    "XXXAABBBBBBBBBBAAXXXXXXXXXXXXXXX",
]))

# NEMESIS - final ship, forked prow and a wide command deck
BOSSES['boss_nemesis'] = ('void', mirror_down([
    "XXXXXX.....XXXXXXXXXXXXXXXX.....",
    "XXXXXXX..XXXXXXXXXXXXXXXXXXXX...",
    "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX.",
    ".XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
    "..XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
    "...XXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
    "....XXXXAAAAAAAAAAXXXXXXXXXXXXXX",
    "...XXXXXAABBBBBBAAXXXXXXXXXXXXXX",
    "..XXXXXXAABBBBBBAAXXXXXXXXXXXXXX",
    ".XXXXXXXAAAAAAAAAAXXXXXXXXXXXXXX",
], has_center=True))

print("bosses:")
for name, (colour, shape) in BOSSES.items():
    # Frame 2 swaps accent/bright so the core appears to pulse.
    alt = [r.replace('A', '#').replace('B', 'A').replace('#', 'B') for r in shape]
    save(name, [shade(shape, colour), shade(alt, colour)])


# ============================================================= BULLETS =====
print("bullets:")

def bolt(rows, colour):
    return shade(rows, colour, outline=False)

save("bullet_player", [bolt([
    "..XXXX",
    "XXXXXX",
    "..XXXX",
], 'steel')])

save("bullet_plasma", [bolt([
    ".XXXX.",
    "XXXXXX",
    "XXXXXX",
    ".XXXX.",
], 'cyan')])

save("bullet_enemy", [
    bolt([".XXXX.", "XXXXXX", "XXXXXX", ".XXXX."], 'red'),
    bolt(["..XX..", ".XXXX.", ".XXXX.", "..XX.."], 'red'),
])

save("bullet_heavy", [
    bolt([".XXXXX.", "XXXXXXX", "XXXXXXX", ".XXXXX."], 'orange'),
    bolt(["..XXX..", ".XXXXX.", ".XXXXX.", "..XXX.."], 'orange'),
])


# =========================================================== EXPLOSION =====
random.seed(7)
EXP = 20

def explosion_frame(i, n=6):
    img = Image.new("RGBA", (EXP, EXP), TRANSPARENT)
    px = img.load()
    c = (EXP - 1) / 2.0
    t = i / (n - 1.0)
    outer = 2.5 + t * (c - 0.5)
    inner = max(0.0, (t - 0.3) * 2.0 * c)
    cols = [rgba(255, 255, 235), rgba(255, 232, 130), rgba(255, 170, 40),
            rgba(232, 92, 28), rgba(168, 40, 30), rgba(90, 20, 24)]
    for y in range(EXP):
        for x in range(EXP):
            d = math.hypot(x - c, y - c) + random.uniform(-0.85, 0.85)
            if not (inner <= d <= outer):
                continue
            band = 0.0 if outer <= inner else (d - inner) / (outer - inner)
            idx = min(len(cols) - 1, int(band * 3) + int(t * 3))
            if random.random() < 0.9:
                px[x, y] = cols[idx]
    return img

print("effects:")
save("explosion", [explosion_frame(i) for i in range(6)])

def spark_frame(i):
    img = Image.new("RGBA", (8, 8), TRANSPARENT)
    px = img.load()
    c = 3.5
    r = 1.2 + i * 1.6
    cols = [rgba(255, 255, 240), rgba(255, 214, 110), rgba(240, 130, 50)]
    for y in range(8):
        for x in range(8):
            d = math.hypot(x - c, y - c)
            if r - 1.3 <= d <= r and random.random() < 0.85:
                px[x, y] = cols[min(i, 2)]
    return img

save("spark", [spark_frame(i) for i in range(3)])


# ============================================================ POWERUPS =====
def powerup(icon_rows, colour):
    """Framed capsule with a bright icon punched into it."""
    pal = RAMPS[colour]
    size = 10
    img = Image.new("RGBA", (size, size), TRANSPARENT)
    px = img.load()
    for y in range(size):
        for x in range(size):
            edge = x in (0, size - 1) or y in (0, size - 1)
            corner = (x in (0, size - 1)) and (y in (0, size - 1))
            if corner:
                continue
            if edge:
                px[x, y] = pal['k']
            elif x in (1, size - 2) or y in (1, size - 2):
                px[x, y] = pal['m'] if y < size // 2 else pal['d']
            else:
                px[x, y] = pal['d'] if y > size // 2 else pal['m']
    for y, row in enumerate(icon_rows):
        for x, ch in enumerate(row):
            if ch != '.':
                px[x + 2, y + 2] = pal['h'] if ch == 'X' else pal['b']
    return img

ICON_HEALTH = ["..XX..", "..XX..", "XXXXXX", "XXXXXX", "..XX..", "..XX.."]
ICON_WEAPON = ["...XX.", "..XX..", ".XXXX.", "XXXX..", "..XX..", ".XX..."]
ICON_SHIELD = [".XXXX.", "XX..XX", "XX..XX", "XX..XX", ".XXXX.", "..XX.."]
ICON_BOMB   = ["...XX.", "..XXX.", ".XXXX.", "XXXXXX", "XXXXXX", ".XXXX."]
ICON_RAPID  = ["X..X..", "XX.XX.", "XXXXXX", "XXXXXX", "XX.XX.", "X..X.."]
ICON_SCORE  = [".XXXX.", "XX..XX", "..XXX.", "..XX..", "......", "..XX.."]

print("powerups:")
save("powerups", [
    powerup(ICON_HEALTH, 'green'),
    powerup(ICON_WEAPON, 'yellow'),
    powerup(ICON_SHIELD, 'cyan'),
    powerup(ICON_BOMB,   'red'),
    powerup(ICON_RAPID,  'orange'),
    powerup(ICON_SCORE,  'magenta'),
])

def shield_bubble(i):
    size = 26
    img = Image.new("RGBA", (size, size), TRANSPARENT)
    px = img.load()
    c = (size - 1) / 2.0
    r = c - 0.5
    cols = [rgba(160, 240, 255), rgba(96, 204, 246), rgba(58, 150, 212)]
    for y in range(size):
        for x in range(size):
            d = math.hypot(x - c, y - c)
            if r - 1.1 <= d <= r:
                ang = math.degrees(math.atan2(y - c, x - c)) + i * 45
                if int(ang / 18) % 2 == 0:
                    px[x, y] = cols[i % 3]
    return img

save("shield", [shield_bubble(i) for i in range(4)])

print("\ndone.")
