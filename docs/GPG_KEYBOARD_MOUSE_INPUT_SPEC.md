# Space Dodger - Google Play Games on PC (GPG) & Hybrid Input Specification

## 1. Overview & Architecture

Space Dodger is engineered to run seamlessly across standard Android smartphones, tablets, foldables, and **Google Play Games for PC (GPG emulator)** with zero friction.

Previously, `AndroidPlatform` only provided `TouchInputProvider`, meaning users running the Android build in Google Play Games on PC were forced to simulate joystick movements by dragging the mouse across the screen.

To deliver a native desktop experience on Google Play Games for PC without compromising the mobile touch experience, we introduced a unified **Composite Input Provider** following SOLID and Design Pattern best practices.

### Architectural Separation (Composite Pattern - SOLID)
- **`IInputProvider`**: Core contract returning a platform-agnostic `InputState` each frame.
- **`TouchInputProvider`**: Handles touch drag joystick, multi-touch virtual coordinates, touch tap candidates, and hardware back button.
- **`KeyboardInputProvider`**: Handles physical keyboard states (`Keyboard.GetState()`), mouse clicks, and mouse wheel delta (`Mouse.GetState()`).
- **`CompositeInputProvider`**: Merges `TouchInputProvider` and `KeyboardInputProvider` concurrently. Movement from the keyboard (WASD/arrows) takes priority when pressed; otherwise, touch joystick / mouse drag is used. Actions (Fire, Confirm, Back, Pause, Directions) are aggregated seamlessly.

---

## 2. Input Mapping Matrix

| Action / Context | Keyboard Input | Mouse / Touch Input |
| :--- | :--- | :--- |
| **Menu Navigation (Item Move)** | `Up`, `Down`, `Left`, `Right` / `W`, `S`, `A`, `D` | Hover (Desktop) / Direct Touch |
| **Menu Item Selection** | `Enter`, `Space` | Mouse Left Click / Touch Tap |
| **Menu Back / Exit** | `Escape`, `Back` key | On-screen "BACK" buttons / Android back gesture |
| **Gameplay Movement** | `W`, `A`, `S`, `D` or Arrow keys (8-way normalized) | Left-side touch drag / mouse virtual joystick |
| **Gameplay Firing** | `Space`, `J` (held or pressed) | Mouse Left Button (held/pressed) / Touch hold / Auto-Attack |
| **Gameplay Pause** | `Escape`, `P`, `Pause` | On-screen Pause button (top header) |
| **Pause Screen Resume** | `Escape`, `P`, or `Enter` on "RESUME" | Mouse click / Touch tap on "RESUME" |
| **Level Select Navigation** | `Arrows` / `WASD` (Grid navigation across 50 levels per page) | Mouse Click / Touch Tap on level badge / Wheel scroll |
| **High Scores Navigation** | `Left` / `Right` arrows to switch between "WORLD RANKING" & "BACK" | Mouse click / Touch tap on buttons |
| **Second Chance (Revive Ad)** | `Up` / `Down` + `Enter` ("WATCH AD" vs "GIVE UP"), `Escape` to Give Up | Mouse click / Touch tap on buttons |
| **House Ad Intermission** | `Space`, `Enter`, or `Escape` to continue (after 5s countdown) | Mouse click / Touch tap on "PLAY" |

---

## 3. Screen-Specific Enhancements

1. **`MenuList`**:
   - Accepts both `Up/Down` and `Left/Right` keys to navigate vertically.
   - Activates selected item with `Enter` or `Space`.
   - Directly activates clicked/tapped item with `Tap`.
2. **`MenuScreen`**:
   - Updated control hint to `"TAP OR ARROWS + ENTER"`.
   - `Escape` safely exits the game.
3. **`HighScoreScreen`**:
   - Added `_mobileButtonIndex` (0: World Ranking, 1: Back).
   - Keyboard users can toggle between buttons using `Left/Right/Up/Down` and hit `Enter` with a distinct visual focus frame.
4. **`SecondChanceScreen`**:
   - Added `BackPressed` check; pressing `Escape` triggers `GiveUp()` to return to game over.
5. **`GameOverScreen`**:
   - Added `BackPressed` check; pressing `Escape` returns to the main menu.
6. **`HouseAdScreen`**:
   - Removed `!IsMobile` restriction on keyboard continue; when 5-second countdown finishes, pressing `Space`, `Enter`, or `Escape` advances the level.
7. **`Hud`**:
   - Dynamic gameplay hint updated to `"DRAG / WASD: MOVE"` and `"SPACE / CLICK: FIRE"`.

---

## 4. Zero-Regression Mobile Guarantee

- On standard mobile smartphones without physical keyboards, `Keyboard.GetState()` reports no keys down.
- `CompositeInputProvider` falls back directly to `TouchInputProvider`.
- Touch joystick movement, touch auto-fire, and touch tap detection operate with 100% fidelity identical to earlier builds.
