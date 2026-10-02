@echo off
title DISABLE FIREBASE DEBUGVIEW - SPACE DODGER
echo ======================================================================
echo   DISABLE FIREBASE DEBUGVIEW MODE - SPACE DODGER
echo ======================================================================
echo.

set "ADB=%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe"
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

if %ERRORLEVEL% equ 0 (
    echo.
    echo  [SUCCESS] Firebase DebugView Realtime Mode DISABLED!
    echo  App returned to standard batched / battery-friendly telemetry mode.
) else (
    echo.
    echo  [ERROR] Failed to reset property. Make sure device is connected.
)

echo.
pause
