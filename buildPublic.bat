@echo off
cls
"Tools\NuGet\nuget.exe" "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion"
"packages\FAKE\tools\Fake.exe" build.fsx "sign=false" "nugetkey=cb11effd-4525-41b9-9add-9da7c0441f5b"
pause