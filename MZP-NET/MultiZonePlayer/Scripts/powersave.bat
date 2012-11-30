echo --------------------------------
echo Initiate power off >> c:\power.log
date /T >> c:\power.log
time /T >> c:\power.log
"C:\Program Files\Gembird\Power Manager\pm.exe" -off -AMP -Socket1 >> c:\power.log
echo Power off completed >> c:\power.log