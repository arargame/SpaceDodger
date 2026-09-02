@echo off
echo.
echo ========================================
echo  SpaceDodger - Build + Imzalama
echo ========================================
echo.

:: Keystore dosya yolu ve Alias tanimi
set KEYSTORE=C:\Users\ararg\AppData\Local\Xamarin\Mono for Android\Keystore\SpaceDodger-Key\SpaceDodger-Key.keystore
set ALIAS=spacedodger-key
for %%I in ("%~dp0..") do set "PROJECT=%%~fI"
for %%I in ("%~dp0..\..\SpaceDodger.Shared") do set "SHARED=%%~fI"
set "OUTPUT=%PROJECT%\bin\Release\signed"
set "SIGNED_FILE=%OUTPUT%\com.koray.spacedodger-SIGNED.aab"

echo [INFO] Keystore: %KEYSTORE%
echo [INFO] Alias: %ALIAS%
echo.

set /p PASS=Keystore sifrenizi girin: 

echo.
echo [1/4] Onceki build kalintilari temizleniyor...
if exist "%PROJECT%\obj\Release" (
    rmdir /s /q "%PROJECT%\obj\Release"
    echo     SpaceDodger.Android obj\Release silindi.
)
if exist "%SHARED%\obj\Release" (
    rmdir /s /q "%SHARED%\obj\Release"
    echo     SpaceDodger.Shared obj\Release silindi.
)
echo     Temizlik tamamlandi.

echo.
echo [2/4] Release AAB paketi derleniyor...
for /f "usebackq" %%i in (`powershell -Command "$vc=(Get-Date).ToString('yy').Substring(1)+(Get-Date).ToString('MMddHHmm'); Write-Output $vc"`) do set "VERSION_CODE=%%i"
for /f "usebackq" %%i in (`powershell -Command "Write-Output (Get-Date).ToString('yyyy.MM.dd')"`) do set "VERSION_NAME=1.0.%%i"
echo     VersionCode: %VERSION_CODE%
echo     VersionName: %VERSION_NAME%

dotnet build "%PROJECT%\SpaceDodger.Android.csproj" -c Release -f net8.0-android /p:AndroidPackageFormat=aab /p:ApplicationVersion=%VERSION_CODE% /p:ApplicationDisplayVersion=%VERSION_NAME%
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo HATA: Build basarisiz!
    pause
    exit /b 1
)

echo.
echo [3/4] Imzaliyor...
if not exist "%OUTPUT%" mkdir "%OUTPUT%"

jarsigner -sigalg SHA256withRSA -digestalg SHA-256 ^
    -keystore "%KEYSTORE%" ^
    -storepass %PASS% ^
    -keypass %PASS% ^
    -signedjar "%SIGNED_FILE%" ^
    "%PROJECT%\bin\Release\net8.0-android\com.koray.spacedodger.aab" ^
    %ALIAS%

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo HATA: Imzalama basarisiz!
    echo Lutfen sifrenizin, Keystore dosya yolunuzun veya Alias (%ALIAS%) adinizin dogru oldugundan emin olun.
    pause
    exit /b 1
)

echo.
echo [4/4] Tamamlandi!
echo.
echo Play Store'a yuklenecek dosya:
echo %SIGNED_FILE%
echo.
if exist "%SIGNED_FILE%" (
    explorer /select,"%SIGNED_FILE%"
) else (
    explorer "%OUTPUT%"
)
pause
