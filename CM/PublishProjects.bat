@echo off

if not %1.==. if "%1"=="/force" set force=true

set pubProjScriptDir=%~dp0
call %pubProjScriptDir%\setvariables.bat %force%

if exist %PublishDir% rmdir /s /q %PublishDir%

pushd %rootDir%

dotnet publish Common\LumberjackLib\LumberjackLib.csproj -o %pubDebDir%\Lumberjack --no-self-contained -c Debug

dotnet publish Common\LumberjackLib\LumberjackLib.csproj -o %pubRelDir%\Lumberjack --no-self-contained -c Release

dotnet build Common\Lumberjack48\Lumberjack48.csproj -p:Configuration=Debug;Platform=AnyCPU -t:Rebuild
robocopy /e /s Common\Lumberjack48\bin\Debug %pubDebDir%\Lumberjack48

dotnet build Common\Lumberjack48\Lumberjack48.csproj -p:Configuration=Release;Platform=AnyCPU -t:Rebuild
robocopy /e /s Common\Lumberjack48\bin\Release %pubRelDir%\Lumberjack48

popd

:Done