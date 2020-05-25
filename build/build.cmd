@echo off
rem Parameter #1: A.B.C the new version number for nuget, assembly and the file. Ex: 1.0.5
rem Parameter #2: optional suffix (beta, alpha) Ex: alpha
rem For 1.0.5-alpha run this:  build 1.0.5 alpha   
rem For 2.0.1 run this:        build 2.0.1
set PACKAGE_VERSION=%1
if not "%1"=="" goto endif0
echo ERROR: Missing version number parameter
exit
endif0
if "%2"=="" goto endif1
set PACKAGE_VERSION=%1-%2
:endif1
dotnet tool install -g dotnet-property
pushd ..\src
dotnet-property mddox.csproj Version:%PACKAGE_VERSION%
dotnet-property mddox.csproj AssemblyVersion:%1.0
dotnet-property mddox.csproj FileVersion:%1.0
dotnet build -c Release
popd
move ..\src\nupkg\LoxSmoke.mddox.%PACKAGE_VERSION%.nupkg
