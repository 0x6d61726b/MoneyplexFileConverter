#Requires AutoHotkey v2.0
#SingleInstance Force

AppWinTitle :=  "Banking4 - "

F5::
{	
	; run script only in Banking4 app
	VerifyWindowTitle(AppWinTitle)
	
	; endless loop
	Loop
	{
		; open split booking window
		Click("Left")
		Click("Right")
		Send("{Down}{Down}{Down}{Enter}")
		VerifyWindowTitle("Splitbuchung")

		; get current booking information
		booking := Map()
		Send("{Tab}{Tab}")
		booking["Name"] := GetTextFieldContent()
		Send("{Tab}")
		booking["Info"] := GetTextFieldContent()
		Send("{Tab}")
		booking["Amount"] := GetTextFieldContent()

		; get split booking for retrieved booking
		splitBooking := GetSplitBooking(booking)
		if (splitBooking)
		{
			; create split booking
			Send("{Tab}")
			for index, element in splitBooking
			{
				Send("{Enter}")
				VerifyWindowTitle("Split")
				Send("+{Tab}")
				Send(element["CreditDebit"])
				Send("{Tab}^a")
				Send(element["Amount"])
				Send("{Tab}^a")
				Send(element["Category"])
				Send("+{Tab}+{Tab}+{Tab}+{Tab}{Enter}")
				Sleep(500) ; needed to avoid NullReferenceException of Banking4
				VerifyWindowTitle("Splitbuchung")
			}
			Send("{Tab}{Enter}")
			VerifyWindowTitle(appWinTitle)
		}
		else
		{
			Send("{Escape}")
			VerifyWindowTitle(appWinTitle)
		}
	}
}

; Returns the array of split bookings for the provided booking
GetSplitBooking(booking)
{
	; Insert the script data provided by MoneyplexFileConverter below
	; (e.g., "MoneyplexXmlExport.split.ahk.txt")
	;============================================================================================================================



	;============================================================================================================================
}

; Returns the text content of the current object.
GetTextFieldContent()
{
	A_Clipboard := ""
	Send("^a^c")
	ClipWait(0.1, 0)
	
	return A_Clipboard
}

; Verifies that the expected window is present and exits the script after timeout.
VerifyWindowTitle(Title)
{
	if not WinWait(Title, , 2)
	{
		MsgBox("WindowTitle '" Title "' not found")
		Exit
	}
}