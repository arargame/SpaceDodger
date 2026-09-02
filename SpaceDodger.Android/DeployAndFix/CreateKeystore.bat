@echo off
echo.
echo ========================================
echo  SpaceDodger - Keystore Olusturucu
echo ========================================
echo.

set KEYSTORE_DIR=C:\Users\ararg\AppData\Local\Xamarin\Mono for Android\Keystore\SpaceDodger-Key
set KEYSTORE_PATH=%KEYSTORE_DIR%\SpaceDodger-Key.keystore
set ALIAS=spacedodger-key

if not exist "%KEYSTORE_DIR%" (
    mkdir "%KEYSTORE_DIR%"
)

if exist "%KEYSTORE_PATH%" (
    echo [UYARI] Keystore dosyasi zaten mevcut:
    echo %KEYSTORE_PATH%
    echo.
    echo Uzerine yazmak istemiyorsaniz bu pencereyi kapatin.
    echo.
)

set /p PASS=Belirleyeceginiz Keystore sifresini girin: 

echo.
echo Keystore olusturuluyor...

keytool -genkeypair -v ^
    -keystore "%KEYSTORE_PATH%" ^
    -alias %ALIAS% ^
    -keyalg RSA ^
    -keysize 2048 ^
    -validity 10000 ^
    -storepass %PASS% ^
    -keypass %PASS% ^
    -dname "CN=ArarGames, OU=Games, O=ArarGames, C=TR"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [HATA] Keystore olusturulamadi!
    pause
    exit /b 1
)

echo.
echo ========================================
echo [BASARILI] Keystore olusturuldu!
echo Dosya: %KEYSTORE_PATH%
echo Alias: %ALIAS%
echo ========================================
echo.
pause
