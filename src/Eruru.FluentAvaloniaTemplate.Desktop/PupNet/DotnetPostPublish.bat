@echo off
set "source=../bin/%BUILD_TARGET%/net11.0/publish/%DOTNET_RUNTIME%"
set "target=%BUILD_APP_BIN%"
if not exist "%target%" mkdir "%target%"
robocopy "%source%" "%target%" /E /COPY:DAT /R:2 /W:1
if %ERRORLEVEL% GEQ 8 exit /b %ERRORLEVEL%