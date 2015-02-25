:: borrowed from:
::  - https://github.com/aspnet/Caching/blob/dev/build.cmd
::  - https://github.com/dotnet/corefx/blob/master/build.cmd

@echo off
setlocal


:: It's easiest to require VS2013 or later really.
if not defined VisualStudioVersion (
    if defined VS140COMNTOOLS (
        call "%VS140COMNTOOLS%\VsDevCmd.bat"
        goto :Run
    )

    if defined VS120COMNTOOLS (
        call "%VS120COMNTOOLS%\VsDevCmd.bat"
        goto :Run
    )

	echo Error: build.cmd requires Visual Studio 2013 or 2015.
	exit /b 1
)


:Run

SET DIR=%~dp0
SET CACHED_NUGET=%LocalAppData%\NuGet\NuGet.exe


echo Downloading latest version of NuGet.exe...
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
IF NOT EXIST %LocalAppData%\NuGet\nuget.exe @powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%CACHED_NUGET%'"


set _buildproj=%~dp0RedisMonitorParser.sln
set _buildArgs="/p:Configuration=Release"

msbuild /m "%_buildproj%" %_buildArgs% %*


exit /b

