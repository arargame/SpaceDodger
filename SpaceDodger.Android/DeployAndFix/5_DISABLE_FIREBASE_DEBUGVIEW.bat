@echo off
title DISABLE FIREBASE DEBUGVIEW - SPACE DODGER
echo ======================================================================
echo   DISABLE FIREBASE DEBUGVIEW MODE - SPACE DODGER
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

echo  Disabling Firebase DebugView mode on target device...
"%ADB%" shell setprop debug.firebase.analytics.app .none.

if %ERRORLEVEL% neq 0 goto :failed

echo.
echo  [SUCCESS] Firebase DebugView Realtime Mode DISABLED!
echo  Telemetry events will now be batched and uploaded periodically.
goto :done

:failed
echo.
echo  [ERROR] Failed to reset debug property. Make sure device is connected via USB.

:done
echo.
pause
