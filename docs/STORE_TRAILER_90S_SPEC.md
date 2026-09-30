# Space Dodger - 90-Second Google Play Store Showcase Video Specification

## 1. Executive Summary
This document outlines the architecture, timeline breakdown, typography, and technical encoding parameters used to generate the official 90-second Google Play Store showcase video for **Space Dodger**.

The video strictly adheres to **Google Play Store Video Flow** best practices:
- **Instant Hook (0-2s):** Immediate full-screen brand impression featuring high-resolution artwork and overarching game scale (**100 Thrilling Levels • 10 Colossal Bosses**).
- **Action-First Pacing:** Real in-game captures highlighting 60 FPS responsive controls, bullet dodging, and screen clearing without distracting tutorial overlays.
- **True Arsenal Scope:** Highlighting the game's actual progression system (**5 Weapon Tiers & 7 Unique Special Weapons**: Homing Missiles, Chain Lightning, Wave Cannons, Spiral Vortex, Scatter, Sweep Laser, Ricochet).
- **Colossal Boss Threat:** Featuring intense boss battle scenes emphasizing dodging and weak-point targeting across the 10 epic boss encounters rather than restrictive level-specific labels.
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
| **Video Bitrate** | ~1.82 Mbps | High visual clarity with zero artifacting for pixel-art |
| **Audio Codec** | AAC-LC (Stereo, 48 kHz, 192 kbps) | Studio-grade stereo chiptune reproduction |
| **Audio Fade-Out** | 88.0s - 90.0s (2.0s exponential fade) | Smooth acoustic resolution at video termination |
| **Web Optimization** | `-movflags +faststart` | Instant streaming playback without downloading full payload |
| **File Size** | `20.5 MB` (20,501,599 bytes) | Optimal storage footprint for Google Play asset uploads |

---

## 3. Timeline & Edit Decision List (EDL)

```
00:00        00:02       00:11          00:21       00:33          00:43       00:53          00:65       00:75          00:86     00:90
  |-- Intro --|-- Combat --|-- Weapons --|-- Bosses --|-- Powerups --|-- Threats --|-- Bosses 2-|-- Combos --|-- Climax --|-- Outro --|
  |  (2.0s)   |   (9.0s)   |   (10.0s)   |  (12.0s)   |   (10.0s)    |   (10.0s)   |  (12.0s)   |  (10.0s)   |  (11.0s)   |  (4.0s)   |
```

### Segment Breakdown

| # | Segment ID | Source Timecode | Duration | Title Badge | Description & Game Features |
|---|---|---|---|---|---|
| **0** | `seg0_intro` | Artwork Asset | 2.0s | **SPACE DODGER**<br>*100 Thrilling Levels • 10 Colossal Bosses* | Full-screen visual hook displaying official key art with blurred lateral extensions and centered branding. |
| **1** | `seg1_action` | `00:36 - 00:45` | 9.0s | **NON-STOP 60 FPS ACTION**<br>*Dodge Bullet Swarms & Blast Alien Fleets* | Pure reflex gameplay showcasing responsive touch controls and fluid ship movement. |
| **2** | `seg2_weapons` | `01:45 - 01:55` | 10.0s | **5 WEAPON TIERS & 7 SPECIALS**<br>*Homing Missiles, Chain Lightning & Wave Cannons* | Showcases heavy firepower: tier upgrades (W1 to W5) and specialized cartridge weapons. |
| **3** | `seg3_boss1` | `04:22 - 04:34` | 12.0s | **10 COLOSSAL BOSS BATTLES**<br>*Target Weak Points & Dodge Lethal Bullet Hell* | Epic boss encounter showcasing concentric bullet hell rings, weak-point focus, and boss HP gauge depletion. |
| **4** | `seg4_powerups` | `05:40 - 05:50` | 10.0s | **SHIELDS, DRONES & SMART BOMBS**<br>*Absorb Damage & Unleash Screen-Clearing Nukes* | Highlights tactical pickups: defensive energy shields, rotating orbital defense satellites, and screen-nuking bombs. |
| **5** | `seg5_enemies` | `08:20 - 08:30` | 10.0s | **DYNAMIC ENEMY SQUADRONS**<br>*Outmaneuver Dive Bombers & Guided Missiles* | Diverse fleet compositions: zigzag interceptors, tracking homing cruisers, and dive bombing swarms. |
| **6** | `seg6_boss2` | `10:10 - 10:22` | 12.0s | **MULTI-PHASE BARRAGES**<br>*Survive Relentless Spiral Bullet Traps* | High-intensity boss battle featuring heavy multi-stream spirals, point-blank evasion, and counter-attacks. |
| **7** | `seg7_combos` | `12:40 - 12:50` | 10.0s | **ORIGINAL RETRO SYNTH BEATS**<br>*Pump Up Multipliers & Dominate High Scores* | Explosive arcade synergy: soaring combo multipliers (`x3`, `x8`, `x12`), smart bomb detonations, and energetic chiptune music. |
| **8** | `seg8_boss3` | `16:48 - 16:59` | 11.0s | **EXTREME BULLET HELL**<br>*Test Pure Reflexes Against Screen-Filling Bosses* | Climax showdown featuring screen-filling bullet storms, maximum intensity, and spectacular boss destruction. |
| **9** | `seg9_outro` | `10:23 - 10:27` | 4.0s | **CLIMB THE WORLD RANKINGS**<br>*Compete for #1 Rank - Play Free on Google Play!* | Final call-to-action driving players to compete on the global leaderboard, resolving with a smooth fade-to-black. |

---

## 4. UI / UX Overlay Design System

- **Lower-Third Non-Intrusive Placement:** Badges are positioned strictly in the bottom zone (`y = 900..1045`), ensuring the top HUD (Boss HP bar, Score, Lives, Pause) remains 100% visible and uncompromised.
- **Xbox Live Style Transitions:** Every badge fades in smoothly at `+0.2s` and fades out gracefully `0.4s` before segment transitions, preventing visual clutter.
- **Full Game Scale Messaging:** Highlights the complete depth of Space Dodger: 100 levels, 10 bosses, 5 tiers, 7 specials, and global competition.

---

## 5. File Location
- **Render Output:**
  `C:\Users\ararg\OneDrive\Masaüstü\Antigravity\Space Dodger\videos\SpaceDodger_Store_Trailer_90s.mp4`
