@echo off
rem Copies this mod into MADNESS: Project Nexus's Mods folder for the current Windows user.
rem Run it from the unzipped folder (double-click). The game must be on the releasebeta branch.
setlocal
set "MODS=%USERPROFILE%\AppData\LocalLow\Gibbing Tree\Madness Project Nexus\Mods"
set "DEST=%MODS%\SwapUnaware"
if not exist "%~dp0SwapUnaware\info.txt" (
  echo Cannot find the SwapUnaware folder next to this file. Unzip the whole zip first.
  pause
  exit /b 1
)
if not exist "%MODS%" mkdir "%MODS%"
if exist "%DEST%" rmdir /s /q "%DEST%"
xcopy "%~dp0SwapUnaware" "%DEST%\" /e /i /q /y >nul
echo Installed to "%DEST%"
echo.
echo Next: Steam, right-click MADNESS: Project Nexus, Properties, Betas, pick releasebeta.
echo Then launch, click the gear icon at the bottom left of the main menu,
echo select "Swap And Unaware", tick ENABLED, Apply, and restart the game.
pause
