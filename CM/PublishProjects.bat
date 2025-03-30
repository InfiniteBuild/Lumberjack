@echo off

set scriptDir=%~dp0
set rootDir=%scriptDir%..
set publishDir=%rootDir%\Publish
set pubRelDir=%publishdir%\Release
set pubDebDir=%publishdir%\Debug

if exist %PublishDir% rmdir /s /q %PublishDir%

pushd %rootDir%

dotnet publish Common\LumberjackLib\LumberjackLib.csproj -o %pubDebDir%\Lumberjack --no-self-contained -c Debug

dotnet publish Common\LumberjackLib\LumberjackLib.csproj -o %pubRelDir%\Lumberjack --no-self-contained -c Release

dotnet build Common\Lumberjack48\Lumberjack48.csproj -p:Configuration=Debug;Platform=AnyCPU -t:Rebuild
robocopy /e /s Common\Lumberjack48\bin\Debug %pubDebDir%\Lumberjack48

dotnet build Common\Lumberjack48\Lumberjack48.csproj -p:Configuration=Release;Platform=AnyCPU -t:Rebuild
robocopy /e /s Common\Lumberjack48\bin\Release %pubRelDir%\Lumberjack48

mkdir %nugetDir% >NUL
mkdir %nugetDir%\Data >NUL
copy /y %rootDir%\License %nugetDir%\Data\License.txt
copy /y %rootDir%\ReadMe.md %nugetDir%\Data\ReadMe.md
copy /y %rootDir%\Resources\Lumberjack.png %nugetDir%\Data\Lumberjack.png

echo.
echo Retrieve Nuget package dependency info

REM for readability, set the list in a variable (list is comma delimited)
set csprojList=Common\LumberjackLib\LumberjackLib.csproj
powershell %rootDir%\cm\scripts\generate_dependencies.ps1 -csprojFiles %csprojList% -outputDir %nugetDir%\Data

popd

:Done