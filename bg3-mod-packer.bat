@ECHO OFF
REM Mod packing utility for BG3, created by ShinyHobo

REM Work from local directory
@setlocal enableextensions
@cd /d "%~dp0"
ECHO Welcome to the BG3 mod packing utility, created by ShinyHobo. 
ECHO Please ensure your workspace root directory is the same name as your mod, and that this file is in the same directory as divine.exe (LSLib).

REM Get full path
SET "MODDIR=%~1"

REM Get pak name
for %%F in ("%~1") do SET "PAKNAME=%%~nxF"

REM Find UUID, NAME, FOLDER, VERSION VALUES in meta.lsx
SET META="%MODDIR%\Mods\%PAKNAME%\meta.lsx"

for /F tokens^=6^delims^=^"^/  %%a in (
 'findstr /c:"<attribute id=\"Folder\"" ^<%META%'
) do SET FOLDERVALUE=%%~a

IF "%FOLDERVALUE%" NEQ "%PAKNAME%" ( GOTO :die)

for /F tokens^=6^delims^=^"^/  %%a in (
 'findstr /c:"<attribute id=\"UUID\"" ^<%META%'
) do SET UUIDVALUE=%%~a
for /F tokens^=6^delims^=^"^/  %%a in (
 'findstr /c:"<attribute id=\"Name\"" ^<%META%'
) do SET NAMEVALUE=%%~a

set "FIRSTMATCH=TRUE"
for /F tokens^=6^delims^=^"^/  %%a in (
 'findstr /c:"<attribute id=\"Version\"" ^<%META%'
) do (
 IF DEFINED FIRSTMATCH (
  SET VERSION=%%~a
  SET "FIRSTMATCH="
 )
)
REM FOLDERVALUE and PAKNAME should be the same
REM TODO ERROR HANDLING FOR UUID, NAME, FOLDER VALUES

REM create mod pack and create temp directory
divine.exe -g "bg3" --action "create-package" --source "%MODDIR%" --destination "%MODDIR%\..\temp\%PAKNAME%.pak" -l "all"

REM Create info file
echo {> "%MODDIR%\..\temp\info.json"
echo    "modName": "%NAMEVALUE%",>> "%MODDIR%\..\temp\info.json"
echo    "GUID": "%UUIDVALUE%",>> "%MODDIR%\..\temp\info.json"
echo    "folderName": "%PAKNAME%",>> "%MODDIR%\..\temp\info.json"
echo    "version": "%VERSION%",>> "%MODDIR%\..\temp\info.json"
echo    "MD5": "">> "%MODDIR%\..\temp\info.json"
echo }>> "%MODDIR%\..\temp\info.json"

REM zip here
powershell "Compress-Archive -Force '%MODDIR%\..\temp\*' '%MODDIR%\..\%PAKNAME%.zip'" 

REM remove temp folder
rmdir /Q /S "%MODDIR%\..\temp"

ECHO All done!
PAUSE
KILL

:die
ECHO Input folder is not the same name as the folder name used in meta.lsx.
PAUSE
KILL