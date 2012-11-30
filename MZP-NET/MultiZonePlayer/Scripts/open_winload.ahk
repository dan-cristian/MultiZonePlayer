Loop
{
WinWait ahk_class TPasswordDlg
Sleep 3000
if WinExist("ahk_class TPasswordDlg")
{
	;Sleep 2000
	;WinActivateBottom, ahk_class TPasswordDlg
	
	ControlGet, OutputVar, Visible, , TEdit1, 1
	if ErrorLevel {
		;MsgBox There was a problem.
	}
	
	else {
		ControlSend TEdit1, master{Enter}, 1
		ControlSend TEdit2, 1234{Enter}, 1
		WinWait ahk_class TMainForm
		if WinExist("ahk_class TMainForm")
		{
			Sleep 2000
			WinActivate
			Send {Enter}
			Sleep 3000
			WinActivate
			Send {F6}
			;Sleep 3000
			WinActivate
			WinMaximize
			;Click 263, 136
			;Sleep 1000
			;WinActivate
			Click 292,173
			Sleep 1000
			WinRestore
			WinMinimize
		}


	}
	;Send {Enter}{Enter}
	;Send master{tab}1234{Enter}
	
}

}