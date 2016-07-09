DetectHiddenWindows On
Loop
{
Sleep 5000
IfWinExist, ahk_class TPasswordDlg
{	
	;MsgBox Win1
	ControlGet, OutputVar, Visible, , TEdit1, 1
	if ErrorLevel {
		;MsgBox There was a problem getting login username field
	}
	else {
		;MsgBox Loging In text=%OutputVar%
		ControlSetText TEdit1, master, 1
		ControlSend TEdit2, 1234{Enter}, 1
		WinWait ahk_class TMainForm
		Sleep 5000
		WinActivate
		ControlSend TDBGrid1, {Enter}
		;Send {Enter}
		Sleep 5000
		;WinMinimize
		;WinMove ,,100,100,300,300
	}
	
}
else {
	;MsgBox No login box found
}

ifWinExist, ahk_class TMainForm
{
	ControlGetText, OutputVar, TRzBitBtn1, 
	If (OutputVar="`&Connect")
	{
		Send {F6}
		;ControlClick, TRzBitBtn1,
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
				;Minimise
				Sleep 1000
				WinActivate
				WinMinimize
				;WinWait ahk_class TMainForm
				;Send {Alt}+{Space}
				;Send n
			}
		}
	}

	ControlGet, OutputVar, Enabled,, TZoneDigiplex32, 
	if (OutputVar=0)
	{
		;MsgBox Monitoring not active, closing winload
		WinKill, WinLoad
	}
}
else
{
	;MsgBox Main Form NOT Detected
}
}