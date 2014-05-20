$minSeqCoverage = 16.00
$minBranchCoverage = 10.00

[xml]$results = gc .\coverage\results.xml

$seqCoverage = $results.CoverageSession.Summary.sequenceCoverage -as [double]
$branchCoverage = $results.CoverageSession.Summary.branchCoverage -as [double]

if ($seqCoverage -lt $minSeqCoverage) { throw "Sequence coverage too low (Expected " + $minSeqCoverage + "% but was " + $seqCoverage + "%)"}

if ($branchCoverage -lt $minBranchCoverage) { throw "Branch coverage too low (Expected " + $minBranchCoverage + "% but was " + $branchCoverage + "%)"}
