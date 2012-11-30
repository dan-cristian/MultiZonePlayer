Dim oXMLHTTP 
Dim oStream 
Dim objFSO, objTextFile, objFile
Dim strPath, strText, currentDirectory
Dim strUrl
On Error Resume Next
 
Set oXMLHTTP = CreateObject("MSXML2.XMLHTTP.3.0") 
Set args = WScript.Arguments

strUrl = "http://localhost:888/cmd?command=cameraevent&"&args.Item(0) & "&" & args.Item(1)& "&" & args.Item(2)
strText =  Now() & " - " & strUrl
oXMLHTTP.Open "GET", strUrl, False 
oXMLHTTP.Send 

If Err.Number <> 0 Then
  'WScript.Echo "Error in http send " & Err.Description
  strText strText & " NOT SENT OK ERR=" & Err.Description
  Err.Clear
End If

'msgbox oXMLHTTP.Status
If oXMLHTTP.Status = 200 Then 
	'Set oStream = CreateObject("ADODB.Stream") 
        'oStream.Open 
        'oStream.Type = 1 
        'oStream.Write oXMLHTTP.responseBody 
        'oStream.SaveToFile "c:\somefolder\file.pdf" 
        'oStream.Close 
   	strText = strText & " SENT OK"
Else
	strText = strText & " STATUS NOT OK " & oXMLHTTP.Status & oXMLHTTP.StatusText
End If 


currentDirectory = left(WScript.ScriptFullName,(Len(WScript.ScriptFullName))-(len(WScript.ScriptName)))
strPath = currentDirectory & "alert.log"
' Create the File System Object
Set objFSO = CreateObject("Scripting.FileSystemObject")

If Not objFSO.FileExists(strPath) Then
   Set objFile = objFSO.CreateTextFile(strPath)
   set objFile = nothing
   'WScript.Echo "Just created " & strPath
End If 

' OpenTextFile Method needs a Const value
' ForAppending = 8 ForReading = 1, ForWriting = 2
Const ForAppending = 8
Set objTextFile = objFSO.OpenTextFile (strPath, ForAppending, True)

' Writes strText every time you run this VBScript
objTextFile.WriteLine(strText)
objTextFile.Close