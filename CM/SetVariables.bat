@echo off

set envfile=

REM create envfile for pipeline
if not %2.==. set envfile=%2

REM override - force setting the variables
if not %1.==. if %1.==force. goto Force

REM only set the variables once
if not "%setvarScriptDir%"=="" goto Done

:Force
if not "%envfile%"=="" if exist %envfile% erase /f /q %envfile%
set scripterror=false

set setvarScriptDir=%~dp0
echo setvarScriptDir: %setvarScriptDir%

set rootDir=%setvarScriptDir%..
echo rootDir: %rootDir%
if not "%envfile%"=="" echo rootDir=%rootDir% >> %envfile%

set binaryDir=%rootDir%\binaries
echo binaryDir: %binaryDir%
if not "%envfile%"=="" echo binaryDir=%binaryDir% >> %envfile%

set buildDir=%binaryDir%\build
echo buildDir: %buildDir%
if not "%envfile%"=="" echo buildDir=%buildDir% >> %envfile%

set buildDebugDir=%buildDir%\Debug
echo buildDebugDir: %buildDebugDir%
if not "%envfile%"=="" echo buildDebugDir=%buildDebugDir% >> %envfile%

set buildReleaseDir=%buildDir%\Release
echo buildReleaseDir: %buildReleaseDir%
if not "%envfile%"=="" echo buildReleaseDir=%buildReleaseDir% >> %envfile%

set publishDir=%binaryDir%\Publish
echo publishDir: %publishDir%
if not "%envfile%"=="" echo publishDir=%publishDir% >> %envfile%

set pubRelDir=%publishdir%\Release
echo pubRelDir: %pubRelDir%
if not "%envfile%"=="" echo pubRelDir=%pubRelDir% >> %envfile%

set pubDebDir=%publishdir%\Debug
echo pubDebDir: %pubDebDir%
if not "%envfile%"=="" echo pubDebDir=%pubDebDir% >> %envfile%

set nugetDir=%binaryDir%\Nuget
echo nugetDir: %nugetDir%
if not "%envfile%"=="" echo nugetDir=%nugetDir% >> %envfile%

set zipDir=%binaryDir%\zip
echo zipDir: %zipDir%
if not "%envfile%"=="" echo zipDir=%zipDir% >> %envfile%

for /f "tokens=1,2,3* delims=<>" %%i in (%rootDir%\CM\version\assemblyversion.props) do if "%%j"=="FileVersion" set version=%%k
echo Version: %version%

:Done
