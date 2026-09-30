# Space Dodger - 90-Second Google Play Store Showcase Video Specification

## 1. Executive Summary
This document outlines the architecture, timeline breakdown, typography, and technical encoding parameters used to generate the official 90-second Google Play Store showcase video for **Space Dodger**.

The video strictly adheres to **Google Play Store Video Flow** best practices:
- **Instant Hook (0-2s):** Immediate full-screen brand impression featuring high-resolution artwork and overarching game scale (**100 Thrilling Levels • 10 Colossal Bosses**).
- **Action-First Pacing:** Real in-game captures highlighting 60 FPS responsive controls, bullet dodging, and screen clearing without distracting tutorial overlays.
- **Weapon Showcase & Diversity:**
  - **Spread Firepower:** Multi-stream tier cannons (W1 to W5).
  - **Tesla Chain Lightning:** High-voltage electric arcs arcing and leaping across alien formations.
  - **Wave Pulse Cannon:** Expanding geometric shockwaves tearing through enemy ranks.
  - **Full-Screen Sweep Laser:** Massive full-height vertical energy beam sweeping left-to-right across the entire display.
- **Colossal Boss Threat:** Featuring intense boss battle scenes emphasizing dodging and weak-point targeting across the 10 epic boss encounters.
- **Power-Up Superiority:** Highlighting dynamic survival tools: energy absorption shields, orbital defense drones, and screen-clearing smart bombs.
- **Audio Branding:** Perfectly synchronized with the original chiptune soundtrack (*Pixel Saloon*).
- **Competitive Call-to-Action (CTA):** Closes on the global leaderboard challenge (*Climb the World Rankings #1*).

---

## 2. Technical Profile & Encoding Parameters

| Parameter | Specification | Standard / Benefit |
|---|---|---|
| **Container** | MP4 (`.mp4`) | Universal playback compatibility |
| **Duration** | Exactly `90.000` seconds | Complies with Play Console requirements (30s - 120s) |
| **Resolution** | `2340 x 1080` (19.5:9 Aspect Ratio) | Native mobile phone resolution; full-bleed on modern devices |
| **Frame Rate** | `60.00 fps` (CFR) | Smooth 60 FPS arcade gameplay rendering |
| **Video Codec** | H.264 / AVC (`libx264`, High Profile) | Universal hardware acceleration across Android devices |
| **Color Space** | `yuv420p` | Standard broadcast & web color format |
| **Video Bitrate** | ~1.79 Mbps | High visual clarity with zero artifacting for pixel-art |
| **Audio Codec** | AAC-LC (Stereo, 48 kHz, 192 kbps) | Studio-grade stereo chiptune reproduction |
| **Audio Fade-Out** | 88.0s - 90.0s (2.0s exponential fade) | Smooth acoustic resolution at video termination |
| **Web Optimization** | `-movflags +faststart` | Instant streaming playback without downloading full payload |
| **File Size** | `20.1 MB` (20,145,419 bytes) | Optimal storage footprint for Google Play asset uploads |

---

## 3. Timeline & Edit Decision List (EDL)

```
00:00   00:02      00:11     00:16    00:22        00:33    00:41     00:49        00:61       00:72        00:85     00:90
  |--0--|----1-----|---2A----|---2B---|-----3------|---4----|---5-----|-----6------|-----7-----|-----8------|----9----|
  Intro  Action     Spread    Tesla    Bosses 1     Wave     Laser     Bosses 2     Combos      Apex Climax  Outro
  (2.0s) (9.0s)     (5.0s)   (6.0s)    (11.0s)     (8.0s)   (8.0s)    (12.0s)      (11.0s)     (13.0s)      (5.0s)
```

### Segment Breakdown

| # | Segment ID | Source Timecode | Duration | Title Badge | Description & Weapon / Feature |
|---|---|---|---|---|---|
| **0** | `seg0_intro` | Artwork Asset | 2.0s | **SPACE DODGER**<br>*100 Thrilling Levels • 10 Colossal Bosses* | Full-screen visual hook displaying official key art with blurred lateral extensions. |
| **1** | `seg1_action` | `00:36 - 00:45` | 9.0s | **NON-STOP 60 FPS ACTION**<br>*Dodge Bullet Swarms & Blast Alien Fleets* | Pure reflex gameplay showcasing responsive touch controls and fluid ship movement. |
| **2A**| `seg2a_spread`| `01:45 - 01:50` | 5.0s | **UPGRADE WEAPON TIERS**<br>*Multi-Stream Spread Shots & Plasma Firepower* | Multi-stream cannon upgrades (W1 to W5) clearing clustered enemy swarms. |
| **2B**| `seg2b_tesla` | `13:38 - 13:44` | 6.0s | **TESLA CHAIN LIGHTNING**<br>*Electric Arcs Chain & Rebound Between Enemies* | Electric gun in action: high-voltage blue lightning arcs jumping between multiple targets. |
| **3** | `seg3_boss1`  | `04:22 - 04:33` | 11.0s| **10 COLOSSAL BOSS BATTLES**<br>*Target Weak Points & Dodge Lethal Bullet Hell* | Concentric bullet hell rings, weak-point focus, and boss HP gauge depletion. |
| **4** | `seg4_wave`   | `05:45 - 05:53` | 8.0s | **WAVE PULSE & SHIELDS**<br>*Expanding Shockwaves & Invulnerability Buffs* | Expanding geometric energy shockwave rings paired with defensive shield bubbles. |
| **5** | `seg5_laser`  | `08:01 - 08:09` | 8.0s | **FULL-SCREEN SWEEP LASER**<br>*Annihilate Everything in Left-to-Right Beam Path* | Full-screen vertical energy beam sweeping horizontally and vaporizing all obstacles. |
| **6** | `seg6_boss2`  | `10:10 - 10:22` | 12.0s| **MULTI-PHASE BARRAGES**<br>*Survive Relentless Spiral Bullet Traps* | Multi-stream spiral bullet patterns and evasive counter-attacks against massive bosses. |
| **7** | `seg7_combos` | `12:40 - 12:51` | 11.0s| **ORIGINAL RETRO SYNTH BEATS**<br>*Pump Up Multipliers & Screen-Clearing Bombs* | Explosive arcade synergy: soaring combo multipliers (`x3`, `x8`, `x12`), smart bomb detonations, and energetic chiptune music. |
| **8** | `seg8_boss3`  | `16:47 - 17:00` | 13.0s| **EXTREME BULLET HELL**<br>*Test Pure Reflexes Against Screen-Filling Bosses* | Climax showdown featuring screen-filling bullet storms, maximum intensity, and boss destruction. |
| **9** | `seg9_outro`  | `10:23 - 10:28` | 5.0s | **CLIMB THE WORLD RANKINGS**<br>*Compete for #1 Rank - Play Free on Google Play!* | Final call-to-action connecting to global leaderboards, resolving with a smooth fade-to-black. |

---

## 4. UI / UX Overlay Design System

- **Lower-Third Non-Intrusive Placement:** Badges are positioned strictly in the bottom zone (`y = 900..1045`), ensuring the top HUD (Boss HP bar, Score, Lives, Pause) remains 100% visible and uncompromised.
- **Xbox Live Style Transitions:** Every badge fades in smoothly at `+0.2s` and fades out gracefully `0.4s` before segment transitions, preventing visual clutter.
- **Color-Coded Weapon Badges:**
  - `TESLA CHAIN LIGHTNING`: Electric cyan theme (`#82F0FF`).
  - `WAVE PULSE`: Energy green theme (`#6EE68C`).
  - `FULL-SCREEN SWEEP LASER`: Plasma blue theme (`#58B4FF`).
  - `BOSS BATTLES`: Warning red theme (`#F04646`).

---

## 5. File Location
- **Render Output:**
  `C:\Users\ararg\OneDrive\Masaüstü\Antigravity\Space Dodger\videos\SpaceDodger_Store_Trailer_90s.mp4`
