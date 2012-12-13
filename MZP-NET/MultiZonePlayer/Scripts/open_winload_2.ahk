DetectHiddenWindows On
Loop
{
Sleep 5000
IfWinExist, ahk_class TPasswordDlg
{	
	ControlGet, OutputVar, Visible, , TEdit1, 1
	if ErrorLevel {
		;MsgBox There was a problem.
	}
	else {
		ControlSend TEdit1, master{Enter}, 1
		ControlSend TEdit2, 1234{Enter}, 1
		WinWait ahk_class TMainForm
		Sleep 2000
		WinActivate
		Send {Enter}
		Sleep 5000
		;WinMinimize
	}
	
}

ifWinExist, ahk_class TMainForm
{
	ControlGetText, OutputVar, TRzBitBtn1, 
	If (OutputVar="`&Connect")
	{
		ControlClick, TRzBitBtn1,
		Sleep 5000

	}
	else
	{
	
		ControlGet, OutputVar, Enabled,, TRzBitBtn1, 
		if (OutputVar=1)
		{
			;MsgBox Connected
			ControlGet, OutputVar, Visible,, TZoneDigiplex32
			if (OutputVar=1)
			{
				;MsgBox ALL OK
			}
			else
			{
				;MsgBox Monitoring not visible
				ControlClick, X264 Y140
			}
		}
	}
}
else
{
	;MsgBox Main Form NOT Detected
}
}