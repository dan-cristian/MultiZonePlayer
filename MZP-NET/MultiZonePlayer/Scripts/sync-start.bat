set logfile=c:\mzp\script.log
set synclog=c:\mzp\sync.log
echo SYNC-START ---------------- >> %logfile%
date /t >> %logfile%
time /t >> %logfile%
diskpart /s c:\mzp\scripts\showbackupdrives.script >> %logfile%
ping -n 10 127.0.0.1
echo Sync Movies Start ........>> %logfile%
"c:\Program Files (x86)\Allway Sync\Bin\syncappw.exe" -s "Movies" -e -m -lf %synclog%
echo Sync Pictures Start ........>> %logfile%
"c:\Program Files (x86)\Allway Sync\Bin\syncappw.exe" -s "Pictures" -e -m -lf %synclog%

sync-end.bat