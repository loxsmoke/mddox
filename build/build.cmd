@echo off
rem Parameter: A.B.C the new version number for nuget, assembly and the file
dotnet tool install -g dotnet-property
pushd ..\src
dotnet-property mddox.csproj Version:%1
dotnet-property mddox.csproj AssemblyVersion:%1.0
dotnet-property mddox.csproj FileVersion:%1.0
dotnet build -c Release
popd
move ..\src\nupkg\LoxSmoke.mddox.%1.nupkg
