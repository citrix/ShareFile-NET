ApiToClient\NuGet\NuGet.exe restore ApiToClient\ApiToClient.sln

msbuild ApiToClient\ApiToClient.sln /t:ReBuild /p:"Platform=x86" /p:Configuration=Debug

set languageDirectory=%CD%
for %%* in (.) do set CurrDirName=%%~n*

cd ApiToClient\bin\Debug
ApiToClient.exe -l %CurrDirName% -o "%languageDirectory%"\Core

cd %languageDirectory%