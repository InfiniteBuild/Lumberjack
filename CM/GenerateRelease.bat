@echo off

if not %1.==. if "%1"=="/force" set force=true

set genRelScriptDir=%~dp0
call %genRelScriptDir%\SetVariables.bat %force%

if exist %PublishDir% rmdir /s /q %PublishDir%

pushd %rootDir%
call %rootDir%\cm\PublishProjects.bat
popd

call %rootdir%\cm\GenerateNugetPackage.bat

%rootdir%\buildtools\7-zip\7z.exe a %zipdir%\Lumberjack_%version%.zip %pubRelDir%\Lumberjack\**
%rootdir%\buildtools\7-zip\7z.exe a %zipdir%\Lumberjack48_%version%.zip %pubRelDir%\Lumberjack48\**

:Done
