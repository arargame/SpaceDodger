@echo off
title Google Play Games PC - Space Dodger Test Deployer
echo Starting Space Dodger deployment to the Google Play Games Developer Emulator...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0GooglePlayGamesTest.ps1"
echo.
echo Process completed.
pause
