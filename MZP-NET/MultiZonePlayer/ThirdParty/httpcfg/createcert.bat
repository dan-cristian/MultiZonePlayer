makecert -r -pe -n "CN=localhost-MZP" -b 01/01/2000 -e 01/01/2036 -eku 1.3.6.1.5.5.7.3.1 -ss my -sr localMachine -sky exchange -sp "Microsoft RSA SChannel Cryptographic Provider" -sy 12

echo check mmc/certif icates (local machine)/personal/details/thumbprint

pause 

httpcfg.exe set ssl -i 0.0.0.0:12345 -c "MY" -h 0c58887acfb2f19400cfe18509e2152ccd0c2dbd