@echo on

set RIMWORLD_MODS_DIR=../../../Mods
set MOD_NAME=CallATrader

REM extract current branch name
git rev-parse --abbrev-ref HEAD > temp.txt
set /p BRANCH=<temp.txt
DEL temp.txt

REM copy Assemblies depending on branch
if "%BRANCH%" == "1.3" xcopy /e /i /y "../Assemblies" "%RIMWORLD_MODS_DIR%/%MOD_NAME%/1.3/Assemblies"
if "%BRANCH%" == "1.4" xcopy /e /i /y "../Assemblies" "%RIMWORLD_MODS_DIR%/%MOD_NAME%/1.4/Assemblies"
if "%BRANCH%" == "1.5" xcopy /e /i /y "../Assemblies" "%RIMWORLD_MODS_DIR%/%MOD_NAME%/1.5/Assemblies"
if "%BRANCH%" == "1.6" xcopy /e /i /y "../Assemblies" "%RIMWORLD_MODS_DIR%/%MOD_NAME%/1.6/Assemblies"

REM clear mod and copy latest files into the target mods dir
rmdir /s /q "%RIMWORLD_MODS_DIR%/%MOD_NAME%/About"
rmdir /s /q "%RIMWORLD_MODS_DIR%/%MOD_NAME%/Defs"
rmdir /s /q "%RIMWORLD_MODS_DIR%/%MOD_NAME%/Languages"
rmdir /s /q "%RIMWORLD_MODS_DIR%/%MOD_NAME%/Textures"

REM copy the static resources
xcopy /e /i /y "../About" "%RIMWORLD_MODS_DIR%/%MOD_NAME%/About"
xcopy /e /i /y "../Defs" "%RIMWORLD_MODS_DIR%/%MOD_NAME%/Defs"
xcopy /e /i /y "../Languages" "%RIMWORLD_MODS_DIR%/%MOD_NAME%/Languages"
xcopy /e /i /y "../Textures" "%RIMWORLD_MODS_DIR%/%MOD_NAME%/Textures"

REM start rimworld
set STEAM_REGKEY=HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam
set STEAM_REGVAL=InstallPath

REM Check for presence of key first.
reg query %STEAM_REGKEY% /v %STEAM_REGVAL% 2>nul || (echo Steam install dir not found! & exit /b 1)

REM query the value.
REM pipe it through findstr in order to find the matching line that has the value. 
REM only grab token 3 and the remainder of the line. %%b is what we are interested in here.
set STEAM_INSTALL_DIR=
for /f "tokens=2,*" %%a in ('reg query %STEAM_REGKEY% /v %STEAM_REGVAL% ^| findstr %STEAM_REGVAL%') do (
    set STEAM_INSTALL_DIR=%%b
)

REM Possibly no value set
if not defined STEAM_INSTALL_DIR (echo Steam.exe not found! & exit /b 1)

REM start rimworld
"%STEAM_INSTALL_DIR%\Steam.exe" -applaunch 294100
