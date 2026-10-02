@echo off
title ENABLE FIREBASE DEBUGVIEW - SPACE DODGER
echo ======================================================================
echo   ENABLE FIREBASE REALTIME STREAM (DEBUGVIEW) - SPACE DODGER
echo ======================================================================
echo.

set "ADB=%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe"
if not exist "%ADB%" set "ADB=C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe"
if not exist "%ADB%" (
    where adb >nul 2>&1
    if %ERRORLEVEL% equ 0 (
        set "ADB=adb"
    ) else (
        echo  [ERROR] adb.exe could not be found!
        pause
        exit /b 1
    )
)

echo  Enabling Firebase DebugView mode on target device...
"%ADB%" shell setprop debug.firebase.analytics.app com.arargames.spacedodger

if %ERRORLEVEL% equ 0 (
    echo.
    echo  [SUCCESS] Firebase DebugView Realtime Mode ENABLED!
    echo.
    echo  All game telemetry events (screen_view, level_started, level_completed,
    echo  supply_collected, level_failed, boss_fight_result, level_5_reached, etc.)
    echo  will now stream to Firebase Console within 1-2 seconds.
    echo.
    echo  Where to monitor live in Firebase:
    echo    Firebase Console ^> Space Dodger ^> Analytics ^> DebugView
    echo.
    echo  When finished testing, run "5_DISABLE_FIREBASE_DEBUGVIEW.bat".
) else (
    echo.
    echo  [ERROR] Failed to set debug property. Make sure device is connected via USB.
)

echo.
pause
