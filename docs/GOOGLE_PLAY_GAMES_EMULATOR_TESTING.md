# Google Play Games PC Emulator Testing

Run `SpaceDodger.Android/DeployAndFix/GooglePlayGamesTest.bat` to deploy the
current Android build to the Google Play Games **Developer Emulator**. The
script uses its dedicated ADB endpoint (`localhost:6520`), builds an
`android-x64` Debug APK, installs it, launches Space Dodger, and saves a fresh
logcat capture under `SpaceDodger.Android/Logs/`.

This is a local developer-emulator workflow. It does not install a package into
the consumer Google Play Games client or alter a Play Console release.

## Prerequisites

- Google Play Games Developer Emulator installed.
- The emulator started and USB debugging authorized when prompted.
- .NET Android workload and the API 36 SDK installed.

The script first uses the ADB executable shipped by the GPG Developer Emulator,
then falls back to `adb` on `PATH` or Android SDK platform-tools.

## Connection troubleshooting

The emulator must appear as `localhost:6520 device` in its bundled ADB's
`devices` output. The deployer does not repeatedly issue `adb connect` once
that entry exists, because repeated connection requests can put this emulator
into an `offline` state. If a prior run left it offline, the deployer repairs
the target transport with `adb reconnect offline` before it starts the build.

## Keyboard validation pass

The Android build shares the hybrid input path used by touch, mouse, and GPG
PC. Test each group on every meaningful screen, not only during gameplay:

| Area | Inputs to verify |
| --- | --- |
| Menus | Up/Down or W/S, Enter/Space, Escape |
| Level selection | Arrow keys or WASD, Enter/Space, Escape |
| Gameplay | WASD/arrow movement, Space/J fire, P/Escape pause |
| Ads, second-chance, and house-ad screens | Mouse click, Enter/Space, Escape where applicable |
| Pause and game-over | Keyboard selection, confirm, retry, and back navigation |

When a control fails, stop the log capture with `Ctrl+C` and keep the generated
`gpg_keyboard_test_*.log` file. It is the required diagnostic artifact for the
next input fix.

## Android hardware keyboard bridge

On Google Play Games PC, `Keyboard.GetState()` can be empty even while the
emulator is receiving physical keyboard events. `MainActivity` therefore maps
Android `KeyEvent` values into `HardwareKeyboardBridge`, and the shared
`KeyboardInputProvider` merges those keys with the native MonoGame keyboard
state. This preserves desktop controls while enabling the Android GPG PC host:

- WASD and arrow keys: movement and menu navigation.
- Enter and Space: menu confirmation; Space/J: manual fire.
- P and Escape: pause/back, depending on the active screen.
- Android Back: mapped to Escape and consumed so Android does not close the
  activity.
