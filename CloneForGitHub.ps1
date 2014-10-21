$Destination = $args[0]

Copy-Item .\* $Destination -Recurse -Force -Exclude .git,.git*

Remove-Item $Destination -include *internal*,*.bat,*.ps*,*.fsx,packages,*.nuspec,bin,obj,packaging,build,ShareFile.Api.Client.snk,TestResult.xml,generate_coverage.bat -Recurse -Force
