rmdir /S /Q coverage
mkdir coverage

NuGet.exe install NUnit.Runners -ExcludeVersion -o packages -Version 2.6.3
NuGet.exe install OpenCover -ExcludeVersion -o packages -Version 4.5.2506
NuGet.exe install ReportGenerator -ExcludeVersion -o packages -Version 1.9.1

@ECHO OFF
IF "%1" == "" (
	SET config=Debug
)
IF "%1" NEQ "" (
	SET config=%1
)

@ECHO ON

packages\OpenCover\OpenCover.Console.exe -target:"packages\NUnit.Runners\tools\nunit-console-x86.exe" -targetargs:"/noshadow Tests\ShareFile.Api.Client.Core.Tests\bin\%config%\ShareFile.Api.Client.Core.Tests.dll" -register:user -filter:+[ShareFile*]* -filter:-[*Test*]* -filter:-[ShareFile*]ShareFile.Api.Models.* -filter:-[ShareFile*]ShareFile.Api.Client.Entities.* -output:"coverage\results.xml"

packages\ReportGenerator\ReportGenerator.exe -verbosity:Error -reports:"coverage\results.xml" -targetdir:coverage
