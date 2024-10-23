@echo off

set scriptDir=%~dp0
set rootDir=%scriptDir%..
set publishDir=%rootDir%\Publish
set pubRelDir=%publishdir%\Release
set pubDebDir=%publishdir%\Debug

if exist %PublishDir% rmdir /s /q %PublishDir%

pushd %scriptDir%..\..

dotnet publish Common\Lumberjack\LumberjackLib.csproj -o %pubDebDir%\Lumberjack --no-self-contained -c Debug

dotnet publish Common\Lumberjack\LumberjackLib.csproj -o %pubRelDir%\Lumberjack --no-self-contained -c Release

dotnet build Common\Lumberjack48\Lumberjack48.csproj -p:Configuration=Debug;Platform=AnyCPU -t:Rebuild
robocopy /e /s Lumberjack48\bin\Debug %pubDebDir%\Lumberjack48

dotnet build Common\Lumberjack48\Lumberjack48.csproj -p:Configuration=Release;Platform=AnyCPU -t:Rebuild
robocopy /e /s Lumberjack48\bin\Release %pubRelDir%\Lumberjack48

popd

:Done