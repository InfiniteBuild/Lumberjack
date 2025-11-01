@echo off

if not %1.==. if "%1"=="/force" set force=force


set pubProjScriptDir=%~dp0
call %pubProjScriptDir%\setvariables.bat %force%

if exist %buildDir% rmdir /s /q %buildDir%

pushd %rootDir%

dotnet build Common\LumberjackLib\LumberjackLib.csproj -o %builddebugdir%\Lumberjack -p:Configuration=Debug;Platform=AnyCPU
if errorlevel 1 goto BuildError

dotnet build Common\LumberjackLib\LumberjackLib.csproj -o %buildreleasedir%\Lumberjack -p:Configuration=Release;Platform=AnyCPU
if errorlevel 1 goto BuildError

dotnet build Common\Lumberjack48\Lumberjack48.csproj -p:Configuration=Debug;Platform=AnyCPU -t:Rebuild
if errorlevel 1 goto BuildError
robocopy /e /s Common\Lumberjack48\bin\Debug %builddebugdir%\Lumberjack48

dotnet build Common\Lumberjack48\Lumberjack48.csproj -p:Configuration=Release;Platform=AnyCPU -t:Rebuild
if errorlevel 1 goto BuildError
robocopy /e /s Common\Lumberjack48\bin\Release %buildreleasedir%\Lumberjack48

goto BuildComplete

:BuildError
echo ERROR during build
set scripterror=true
goto Done

:BuildComplete
call %pubProjScriptDir%\GenerateNugetInfo.bat

:Done

popd

if "%scripterror%"=="true" exit /b 1
exit /b 0