:: Parts borrowed from:
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
set _buildConfig=Release
set _buildArgs="/p:Configuration=%_buildConfig%"

msbuild /m "%_buildproj%" %_buildArgs% %*


:: Run tests

set nunitRunner="%~dp0packages\NUnit.Runners.2.6.4\tools\nunit-console.exe"
set testFiles="%~dp0tests\RedisMonitorParser.Tests\bin\Release\RedisMonitorParser.Tests.dll"
%nunitRunner% %testFiles% /framework:4.5

exit /b

