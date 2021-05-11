SETLOCAL

rem 
rem Download Ark Save Viewer from http://www.miragesoftware.co.uk/Ark/Apps/ArkViewer2021-4.zip
rem

:Start

rem Wait for a short time so the CPU doesn't get thrashed continuously.
timeout 60

rem location of the web site pages
set destDir=Z:\Apache24\htdocs

rem location of the Cluster's "Saved" directory
set sourceDir=C:\ArkServer\ShooterGame\ShooterGame\Saved

rem Get the date and time.
set dt=%DATE:~6,4%%DATE:~3,2%%DATE:~0,2%
set tm=%TIME:~0,2%%TIME:~3,2%
set dt=%dt: =0%
set tm=%tm: =0%

echo Dino listings were generated on %dt% at %tm% > %destDir%\LastGenTime.txt

rem Wiredcat Ark server is configured to use specific cpu cores allocated to specific maps - the /AFFINITY 0x03 limits the script to first two threads (the first core) on the CPU so to avoid using cores Ark is
rem as ArkViewer is very CPU heavy, we want it to have its own core.
rem If you dont need this, just remove the /AFFINITY 0x03
rem /WAIT waits till RunDinoListingOnSaveFile.bat has finished running before going to the next one - don't want them all running at once!
rem /MIN run in minimused window
rem /BELOWNORMAL run process at a low priority
rem See windows start.exe command in a windows dos based command prompt for further info - start.exe /?
start /AFFINITY 0x03 /WAIT /MIN /BELOWNORMAL RunDinoListingOnSaveFile.bat %destDir%\SavedIsland %sourceDir%\SavedIsland\TheIsland.ark
start /AFFINITY 0x03 /WAIT /MIN /BELOWNORMAL RunDinoListingOnSaveFile.bat %destDir%\SavedScorchedEarth %sourceDir%\SavedScorchedEarth\ScorchedEarth_P.ark
start /AFFINITY 0x03 /WAIT /MIN /BELOWNORMAL RunDinoListingOnSaveFile.bat %destDir%\SavedCenter %sourceDir%\SavedCenter\TheCenter.ark
start /AFFINITY 0x03 /WAIT /MIN /BELOWNORMAL RunDinoListingOnSaveFile.bat %destDir%\SavedAberration %sourceDir%\SavedAberration\Aberration_P.ark
start /AFFINITY 0x03 /WAIT /MIN /BELOWNORMAL RunDinoListingOnSaveFile.bat %destDir%\SavedExtinction %sourceDir%\SavedExtinction\Extinction.ark
start /AFFINITY 0x03 /WAIT /MIN /BELOWNORMAL RunDinoListingOnSaveFile.bat %destDir%\SavedValguero %sourceDir%\SavedValguero\Valguero_P.ark
start /AFFINITY 0x03 /WAIT /MIN /BELOWNORMAL RunDinoListingOnSaveFile.bat %destDir%\SavedGenesis %sourceDir%\SavedGenesis\Genesis.ark
start /AFFINITY 0x03 /WAIT /MIN /BELOWNORMAL RunDinoListingOnSaveFile.bat %destDir%\SavedCrystal %sourceDir%\SavedCrystal\CrystalIsles.ark
start /AFFINITY 0x03 /WAIT /MIN /BELOWNORMAL RunDinoListingOnSaveFile.bat %destDir%\SavedRagnarok %sourceDir%\SavedRagnarok\Ragnarok.ark

rem If the server isn't rebooting, then repeat the process.
if not exist \ArkServer\Rebooting.txt goto Start