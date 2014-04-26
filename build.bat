@echo off
cls
"Tools\NuGet\nuget.exe" "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion"
"packages\FAKE\tools\Fake.exe" build.fsx "sign=false"
"packages\FAKE\tools\Fake.exe" build.fsx "sign=true"
pause