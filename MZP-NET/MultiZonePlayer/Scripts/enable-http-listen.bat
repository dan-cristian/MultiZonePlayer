netsh http add urlacl http://*:80/ user=Everyone listen=yes
netsh http add urlacl http://+:80/ user=Everyone listen=yes

netsh http delete urlacl url=http://+:80/
netsh http delete urlacl url=http://*:80/
