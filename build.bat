set languageDirectory=%CD%
for %%* in (.) do set CurrDirName=%%~n*

cd ApiToClient\bin\Debug
ApiToClient.exe -l %CurrDirName% -o "%languageDirectory%"

cd %languageDirectory%