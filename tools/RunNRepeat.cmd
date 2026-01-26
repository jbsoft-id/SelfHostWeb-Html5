@echo off
:TOP
cls
dotnet run --project ..\SelfHostWeb
set "LAST_ERRORLEVEL=%ERRORLEVEL%"
ToTop.exe %0
IF %LAST_ERRORLEVEL% NEQ 0 goto DONE
goto TOP
:DONE
