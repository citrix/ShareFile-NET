@echo off
cls
"Tools\NuGet\nuget.exe" "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion"
"packages\FAKE\tools\Fake.exe" build.fsx "sign=false" "nugetkey=nUg3tMyP@cKag3" "nugetserver=http://sf-source.citrite.net:8081/"
"packages\FAKE\tools\Fake.exe" build.fsx "sign=true" "nugetkey=nUg3tMyP@cKag3" "nugetserver=http://sf-source.citrite.net:8081/"
pause