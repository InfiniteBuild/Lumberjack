@echo off

setlocal

set packVer=%1
set sourceDir=%2
set nuspecFile=%3
set nugetDir=%sourceDir%\..\Nuget

echo packVer: %packVer%
echo sourceDir: %sourceDir%
echo nuspecFile: %nuspecFile%
echo nugetDir: %nugetDir%

REM Create nuspec
echo Create NuSpec file
if EXIST %nuspecFile% erase /f /q %nuspecFile%

echo ^<?xml version="1.0" encoding="utf-8"?^> >> %nuspecFile%
echo ^<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd"^> >> %nuspecFile%
echo ^<metadata^> >> %nuspecFile%
echo ^<id^>Lumberjack^</id^> >> %nuspecFile%
echo ^<version^>%packVer%^</version^> >> %nuspecFile%
echo ^<description^>A CSharp library to support basic application logging^</description^> >> %nuspecFile%
echo ^<authors^>Infinite Build^</authors^> >> %nuspecFile%
echo ^<repository type="git" url="https://github.com/InfiniteBuild/Lumberjack.git" /^> >> %nuspecFile%
echo ^<readme^>docs\ReadMe.md^</readme^> >> %nuspecFile%
echo ^<license type="file"^>License.txt^</license^>  >> %nuspecFile%
echo ^<icon^>images/Lumberjack.png^</icon^> >> %nuspecFile%
echo ^<dependencies^> >> %nuspecFile%
for %%i in ("%nugetDir%\data\dependencies*.xml") do (
    type %%i >> %nuspecFile%
	echo. >> %nuspecFile%
)
echo ^</dependencies^> >> %nuspecFile%
echo ^</metadata^> >> %nuspecFile%

echo ^<files^> >> %nuspecFile%

echo ^<file src="%sourceDir%\Lumberjack\**" target="lib\net8.0"/^> >> %nuspecFile%
echo ^<file src="%sourceDir%\Lumberjack48\**" target="lib\net48"/^> >> %nuspecFile%
echo ^<file src="%nugetDir%\Data\ReadMe.md" target="docs\" /^> >> %nuspecFile%
echo ^<file src="%nugetDir%\Data\License.txt" target="" /^> >> %nuspecFile%
echo ^<file src="%nugetDir%\Data\Lumberjack.png" target="images\" /^> >> %nuspecFile%

echo ^</files^> >> %nuspecFile%
echo ^</package^> >> %nuspecFile%

:Done
endlocal 
