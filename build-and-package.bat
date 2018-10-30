@ECHO OFF

msbuild /t:Clean

FOR /F "tokens=*" %%G IN ('DIR /B /AD /S bin') DO RMDIR /S /Q "%%G"
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

msbuild /t:Restore
msbuild /t:Build /p:Configuration=Release

SETLOCAL
SET VERSION=1.0.2
SET NUGET=nuget.exe

FOR /R %%G IN (*.nuspec) DO (
  nuget.exe pack %%G -Version %VERSION% -properties Configuration=Release
)