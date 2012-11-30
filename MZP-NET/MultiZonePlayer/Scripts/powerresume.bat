echo --------------------------------
echo Initiate power on >> c:\power.log
date /T >> c:\power.log
time /T >> c:\power.log
"C:\Program Files\Gembird\Power Manager\pm.exe" -on -AMP -Socket1 >> c:\power.log
echo Power ON completed >> c:\power.log