# Audio & Music System

## Overview

SpaceDodger uses a lightweight, low-overhead audio architecture that streams raw audio assets without requiring MGCB content pipeline compilation:

- **Sound Effects (WAV):** Short, uncompressed sound effects loaded via `TitleContainer.OpenStream` into pooled/cached `SoundEffect` instances.
- **Background Music (MP3):** Multi-track playlist streamed via MonoGame's `MediaPlayer` and `Song.FromUri`.

## Music Tracks

Located in `Content/musics/`:
1. `Arcade Pulse.mp3`
2. `Coin Dash Circuit.mp3`
3. `Coin Drop Sprint.mp3`
4. `Coin-Op Mirage.mp3`
5. `Pixel Saloon.mp3`

## Non-Repeating Shuffle Bag System

To provide dynamic arcade variety without player fatigue:

1. **Cycle Shuffle:** All tracks are indexed into a playlist and shuffled using the Fisher-Yates algorithm.
2. **Consecutive Repeat Prevention:** When a cycle ends and a new shuffled cycle is generated, the system checks if the first track of the new cycle matches the last track played in the previous cycle (`_playlist[0] == _lastTrackIndex`). If it matches, the first track is swapped with another index in the playlist. This mathematically guarantees that no song can ever be played back-to-back across cycle transitions.
3. **Automatic Progression:** The `AudioService.Update` loop observes `MediaPlayer.State`. When the current track concludes (`MediaState.Stopped`), it automatically pulls the next track from the shuffle playlist.

## Cross-Platform Content Management

- **Desktop (DesktopGL):**
  - Project configuration: `<Content Include="..\Content\**\*.*" Link="Content\%(RecursiveDir)%(Filename)%(Extension)" CopyToOutputDirectory="PreserveNewest" />`
  - All music files are copied to `bin/Debug/net8.0/Content/musics/`.
  - Loaded with absolute file URI when present on disk.
- **Mobile (Android):**
  - Project configuration: `<AndroidAsset Include="..\Content\**\*.*" Link="Assets\Content\%(RecursiveDir)%(Filename)%(Extension)" />`
  - Packaged directly into the APK as Android assets (`Assets/Content/musics/...`).
  - Loaded with relative URI (`Content/musics/...`) via `Song.FromUri` which delegates to Android `AssetFileDescriptor` / `MediaPlayer`.

## Options & Persistence Integration

- Respects `SaveData.MusicEnabled` and `SaveData.SoundEnabled`.
- Toggling music OFF immediately pauses playback. Toggling music ON immediately resumes or starts the next track.
- Master music volume is balanced to `0.60f` to leave acoustic headroom for combat sound effects.
