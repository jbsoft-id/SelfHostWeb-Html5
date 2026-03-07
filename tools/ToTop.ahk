SetTitleMatchMode 2

if( A_Args[1] = "" )
{
  MsgBox, You must provide a window title as an argument.
  Exit, -1
}

if WinExist(A_Args[1])
{
  WinActivate ; use the window found above
}
