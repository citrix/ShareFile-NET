ApiToClient\NuGet\NuGet.exe restore ApiToClient\ApiToClient.sln

msbuild ApiToClient\ApiToClient.sln /t:ReBuild /p:"PLatform=x86" /p:Configuration=Debug

set languageDirectory=%CD%
for %%* in (.) do set CurrDirName=%%~n*

cd ApiToClient\bin\Debug
ApiToClient.exe -l %CurrDirName% -o "%languageDirectory%"

cd %languageDirectory%