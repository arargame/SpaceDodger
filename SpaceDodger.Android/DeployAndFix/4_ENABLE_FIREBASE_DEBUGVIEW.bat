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
"%ADB%" shell setprop log.tag.FA VERBOSE
"%ADB%" shell setprop log.tag.FA-SVC VERBOSE

if %ERRORLEVEL% neq 0 goto :failed

echo.
echo  [SUCCESS] Firebase DebugView Realtime Mode ENABLED!
echo.
echo  All game telemetry events:
echo    - screen_view
echo    - level_started
echo    - level_completed
echo    - supply_collected
echo    - level_failed
echo    - boss_fight_result
echo    - level_5_reached
echo  will now stream to Firebase Console within 1-2 seconds.
echo.
echo  Where to monitor live in Firebase:
echo    Firebase Console -^> Space Dodger -^> Analytics -^> DebugView
echo.
echo  When finished testing, run 5_DISABLE_FIREBASE_DEBUGVIEW.bat
goto :done

:failed
echo.
echo  [ERROR] Failed to set debug property. Make sure device is connected via USB.

:done
echo.
pause
