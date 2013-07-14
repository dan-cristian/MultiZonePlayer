set logfile=c:\mzp\script.log
echo SYNC-END ---------------- >> %logfile%
date /t >> %logfile%
time /t >> %logfile%
diskpart /s c:\mzp\scripts\hidebackupdrives.script >> %logfile%