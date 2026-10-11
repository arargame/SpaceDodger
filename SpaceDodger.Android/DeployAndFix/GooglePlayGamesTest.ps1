# Builds, installs, and launches Space Dodger on the Google Play Games Developer Emulator.
# The target is intentionally fixed to the GPG emulator so a connected phone is never used.

$ErrorActionPreference = "Stop"

$ProjectDirectory = (Resolve-Path "$PSScriptRoot\..").Path
$ProjectFile = Join-Path $ProjectDirectory "SpaceDodger.Android.csproj"
$PackageName = "com.arargames.spacedodger"
$TargetFramework = "net9.0-android36.0"
$EmulatorAddress = "localhost:6520"

function Get-AdbPath {
    $gpgAdb = "C:\Program Files\Google\Play Games Developer Emulator\current\emulator\adb.exe"
    if (Test-Path $gpgAdb) {
        Write-Host "Using the Google Play Games Developer Emulator ADB." -ForegroundColor Green
        return $gpgAdb
    }

    $pathAdb = Get-Command adb -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
    if ($pathAdb) { return $pathAdb }

    $sdkAdb = Join-Path $env:LOCALAPPDATA "Android\Sdk\platform-tools\adb.exe"
    if (Test-Path $sdkAdb) { return $sdkAdb }

    throw "ADB was not found. Install Android SDK platform-tools or the Google Play Games Developer Emulator."
}

function Get-GpgDeviceState([string]$AdbPath) {
    $devices = & $AdbPath devices
    if (($devices | Where-Object { $_ -match "^$([regex]::Escape($EmulatorAddress))\s+device$" }).Count -gt 0) {
        return "Device"
    }

    if (($devices | Where-Object { $_ -match "^$([regex]::Escape($EmulatorAddress))\s+offline$" }).Count -gt 0) {
        return "Offline"
    }

    return "Missing"
}

Write-Host "=== Google Play Games PC - Space Dodger Test Process ===" -ForegroundColor Cyan

Write-Host "[1/5] Locating ADB..." -ForegroundColor Yellow
$Adb = Get-AdbPath
Write-Host "ADB: $Adb" -ForegroundColor Green

Write-Host "[2/5] Connecting to the Google Play Games Developer Emulator..." -ForegroundColor Yellow
$deviceState = Get-GpgDeviceState $Adb
if ($deviceState -eq "Offline") {
    Write-Host "Repairing the offline GPG ADB transport..." -ForegroundColor Yellow
    & $Adb reconnect offline | Out-Null
    Start-Sleep -Seconds 3
    $deviceState = Get-GpgDeviceState $Adb
}

if ($deviceState -eq "Missing") {
    $bootstrapper = "C:\Program Files\Google\Play Games Developer Emulator\Bootstrapper.exe"
    if (Test-Path $bootstrapper) {
        Write-Host "Starting the Google Play Games Developer Emulator..." -ForegroundColor Yellow
        Start-Process $bootstrapper
    }

    $deadline = (Get-Date).AddSeconds(45)
    while ((Get-Date) -lt $deadline -and $deviceState -ne "Device") {
        Start-Sleep -Seconds 3
        $deviceState = Get-GpgDeviceState $Adb
        Write-Host "Waiting for emulator authorization..." -ForegroundColor DarkGray
    }
}

if ($deviceState -ne "Device") {
    if ($deviceState -eq "Offline") {
        throw "The GPG Developer Emulator is offline at $EmulatorAddress. Close and reopen the emulator, then run this script again."
    }

    throw "The GPG Developer Emulator is unavailable at $EmulatorAddress. Start it and accept any USB debugging authorization prompt."
}

Write-Host "Connected to $EmulatorAddress." -ForegroundColor Green

Write-Host "[3/5] Building the android-x64 Debug APK..." -ForegroundColor Yellow
& dotnet publish $ProjectFile `
    -c Debug `
    -f $TargetFramework `
    -r android-x64 `
    "-p:AndroidPackageFormat=apk" `
    "-p:AndroidCreatePackagePerAbi=false" `
    "-p:AndroidFastDeployment=false" `
    "-p:EmbedAssembliesIntoApk=true"
if ($LASTEXITCODE -ne 0) {
    throw "Android x64 build failed."
}

$apkSearchDirectories = @(
    (Join-Path $ProjectDirectory "bin\Debug\$TargetFramework\android-x64\publish"),
    (Join-Path $ProjectDirectory "bin\Debug\$TargetFramework\android-x64"),
    (Join-Path $ProjectDirectory "bin\Debug\$TargetFramework")
)
$apkCandidates = foreach ($directory in $apkSearchDirectories) {
    if (Test-Path $directory) {
        Get-ChildItem -Path $directory -Filter "*.apk" -Recurse |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1
    }
}
$apk = $apkCandidates | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if (-not $apk) {
    throw "No APK was found in the android-x64 build output."
}

Write-Host "[4/5] Installing $($apk.Name) on the GPG emulator..." -ForegroundColor Yellow
& $Adb -s $EmulatorAddress install -r -d $apk.FullName
if ($LASTEXITCODE -ne 0) {
    throw "APK installation failed."
}

Write-Host "[5/5] Launching Space Dodger and starting a fresh log capture..." -ForegroundColor Yellow
& $Adb -s $EmulatorAddress logcat -c
& $Adb -s $EmulatorAddress shell am force-stop $PackageName
& $Adb -s $EmulatorAddress shell monkey -p $PackageName 1 | Out-Null

$logDirectory = Join-Path $ProjectDirectory "Logs"
if (-not (Test-Path $logDirectory)) {
    New-Item -ItemType Directory -Path $logDirectory | Out-Null
}

$logFile = Join-Path $logDirectory ("gpg_keyboard_test_{0}.log" -f (Get-Date -Format "yyyyMMdd_HHmmss"))
Write-Host "Game launched. Test WASD/arrows, Enter/Space, Esc/P, and mouse." -ForegroundColor Green
Write-Host "Log capture: $logFile" -ForegroundColor Cyan
Write-Host "Press Ctrl+C when the test is complete." -ForegroundColor Cyan

& $Adb -s $EmulatorAddress logcat -v time `
    "DOTNET:*" "mono-stdout:*" "mono-stderr:*" "SpaceDodger:*" `
    "AndroidRuntime:E" "System.err:W" "ActivityManager:I" "*:S" |
    Tee-Object -FilePath $logFile
