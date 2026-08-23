#!/usr/bin/env python3
"""Render the hand-authored pixel font into Content/sprites/font.png."""
import os, sys
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from font_data import G, rows, GLYPH_W, GLYPH_H, CELL_W, CELL_H

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "Content", "sprites")
os.makedirs(OUT, exist_ok=True)

FIRST, LAST, COLS = 32, 126, 16
chars = [chr(c) for c in range(FIRST, LAST + 1)]

missing = [c for c in chars if c not in G]
if missing:
    raise SystemExit(f"missing glyphs: {missing}")

rows_n = (len(chars) + COLS - 1) // COLS
img = Image.new("RGBA", (COLS * CELL_W, rows_n * CELL_H), (0, 0, 0, 0))
px = img.load()

for i, ch in enumerate(chars):
    ox, oy = (i % COLS) * CELL_W, (i // COLS) * CELL_H
    for y, row in enumerate(rows(ch)):
        for x, c in enumerate(row):
            if c == 'X':
                px[ox + x, oy + y] = (255, 255, 255, 255)

img.save(os.path.join(OUT, "font.png"))
print(f"font.png {img.size}  cell {CELL_W}x{CELL_H}  glyph {GLYPH_W}x{GLYPH_H}  "
      f"{len(chars)} glyphs, first={FIRST}, cols={COLS}")
