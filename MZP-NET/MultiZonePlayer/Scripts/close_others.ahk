Loop
{
Sleep 1000

if WinExist("Performance Options")
{
	WinActivate
	Send {Enter}

}

if WinExist("Optional update delivery is not working")
{
	WinActivate
	Send {Enter}

}

}

