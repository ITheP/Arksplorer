SETLOCAL

@REM Only generate dino listings if the save file is newer than the last dino listing.

set destDir=%1
set sourceFile=%2

rem Location of the Ark Save Viewer executable
set exeDir=C:\ArkTools\ArkViewer2021_4_20210502

if not exist %sourceFile% exit
if not exist %sourceFile%.timestamp goto MakeList

FOR /F %%i IN ('DIR /B /O:D %sourceFile%.timestamp %sourceFile%') DO SET NEWEST=%%i

rem Using a goto so we can watch the script's progress in the CMD window.
if "%NEWEST:timestamp=%"=="%NEWEST%" goto MakeList
exit

:MakeList

REM precautionary wait for save to finish, to avoid reading a file that's in use
timeout 10

rem create the output directories
if not exist %destDir% mkdir %destDir%

rem Copy the ark file to a temporary file, and extract the dino information from the temporary file.
rem This should avoid situations where the ark file is locked by ASV, thus preventing the Ark server from completing a save.
copy /Y %sourceFile% %sourceFile%.asv
%exeDir%\ARKSaveViewer.exe all %destDir%\Dinos.json %sourceFile%.asv

rem rename the output files to have shorter, easier to remember names.
move /Y %destDir%\ARKViewer_Export_Players.json %destDir%\Survivors.json
move /Y %destDir%\ARKViewer_Export_Tamed.json %destDir%\Dinos.json
move /Y %destDir%\ARKViewer_Export_Tribes.json %destDir%\Tribes.json
move /Y %destDir%\ARKViewer_Export_Wild.json %destDir%\Wild.json

rem create two files to act as a timestamp to show when these lists were created.
set dt=%DATE:~6,4%%DATE:~3,2%%DATE:~0,2%_%TIME:~0,2%%TIME:~3,2%
set dt=%dt: =0%
echo "now" > %sourceFile%.timestamp
echo {"date":"%dt%"} > %destDir%\timestamp.json

rem pause
exit

