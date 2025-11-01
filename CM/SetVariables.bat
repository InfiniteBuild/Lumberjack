@echo off

REM override - force setting the variables
if not %1.==. goto Force

REM only set the variables once
if not "%setvarScriptDir%"=="" goto Done

:Force
set scripterror=false

set setvarScriptDir=%~dp0
echo setvarScriptDir: %setvarScriptDir%

set rootDir=%setvarScriptDir%..
echo rootDir: %rootDir%

set binaryDir=%rootDir%\binaries
set buildDir=%binaryDir%\build
set buildDebugDir=%buildDir%\Debug
set buildReleaseDir=%buildDir%\Release

set publishDir=%binaryDir%\Publish
echo publishDir: %publishDir%

set pubRelDir=%publishdir%\Release
echo pubRelDir: %pubRelDir%

set pubDebDir=%publishdir%\Debug
echo pubDebDir: %pubDebDir%

set nugetDir=%binaryDir%\Nuget
echo nugetDir: %nugetDir%

set zipDir=%binaryDir%\zip
echo zipDir: %zipDir%

for /f "tokens=1,2,3* delims=<>" %%i in (%rootDir%\CM\version\assemblyversion.props) do if "%%j"=="FileVersion" set version=%%k
echo Version: %version%

:Done
set force=