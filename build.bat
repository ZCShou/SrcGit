@echo off

rmdir /s /q publish

cd src
dotnet publish SrcGit.csproj --nologo -c Release -r win-x64 -f net6.0-windows -p:PublishSingleFile=true --no-self-contained -o ..\publish
dotnet publish SrcGit.csproj --nologo -c Release -r win-x64 -f net6.0-windows -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishReadyToRun=true --self-contained -o ..\publish\tmp
move ..\publish\tmp\SrcGit.exe ..\publish\SrcGit.bundle.exe
rmdir /s /q ..\publish\tmp
del /f /s /q ..\publish\SrcGit.pdb

cd ..