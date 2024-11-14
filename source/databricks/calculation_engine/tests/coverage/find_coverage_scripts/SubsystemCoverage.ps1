$scriptPath = $PSScriptRoot

# Determine the subsystem tests folder five levels up from the script path
$tempFolder = Split-Path (Split-Path (Split-Path (Split-Path (Split-Path $scriptPath -Parent) -Parent) -Parent) -Parent) -Parent
$subsystemTestFolder = "$tempFolder\dotnet\subsystem-tests\Features\"

# Output CSV file path set relative to the script's location
$outputCsv = "$scriptPath\..\find_coverage_script_output\subsystem_tests.csv"

# Prepare to collect output data
$outputData = @()

# Get all subfolders in the subsystem test folder
$subfolders = Get-ChildItem -Path $subsystemTestFolder -Directory

# Iterate through each subfolder to find .cs files and process each one
foreach ($subfolder in $subfolders) {
    $testFiles = Get-ChildItem -Path $subfolder.FullName -Filter *.cs
    foreach ($testFile in $testFiles) {
        $testClassFilePath = $testFile.FullName
        $content = Get-Content -Path $testClassFilePath -Raw

        # Regex pattern to find methods with their descriptions, handling optional ScenarioStep
        $pattern = '(?:\[ScenarioStep\((\d+)\)\]\s*)?\[SubsystemFact\]\s*public (async Task|void) (\w+)\s*\(\)\s*{((?s).*?)^\s*}\s*(?=\[|$)'
        $matchesReg = [regex]::Matches($content, $pattern, 'Multiline')

        foreach ($match in $matchesReg) {
            $methodName = $match.Groups[3].Value
            $methodBody = $match.Groups[4].Value.Trim()

            # Process to collect assertions even if spanning multiple lines
            $isCollecting = $false
            $currentAssertion = ""
            $assertions = @()
            foreach ($line in ($methodBody -split "\r?\n")) {
                if ($isCollecting) {
                    $currentAssertion += " " + $line.Trim()
                    if ($line.Trim().EndsWith(";")) {
                        $assertions += $currentAssertion
                        $currentAssertion = ""
                        $isCollecting = $false
                    }
                } elseif ($line -match "\.Should\(\)") {
                    $isCollecting = $true
                    $currentAssertion = $line.Trim()
                    if ($line.Trim().EndsWith(";")) {
                        $assertions += $currentAssertion
                        $currentAssertion = ""
                        $isCollecting = $false
                    }
                }
            }

            # Prepare data for CSV output
            foreach ($assertion in $assertions) {
                $outputData += [PSCustomObject]@{
                    Scenario   = $testFile.BaseName
                    TestCase   = $methodName
                    Assertions = $assertion
                }
            }
            if ($assertions.Count -eq 0) {
                $outputData += [PSCustomObject]@{
                    Scenario   = $testFile.BaseName
                    TestCase   = $methodName
                    Assertions = ""
                }
            }
        }
    }
}

# Ensure the output directory exists
$directory = Split-Path -Path $outputCsv
if (-not (Test-Path -Path $directory)) {
    New-Item -Path $directory -ItemType Directory
}

# Write collected data to CSV with UTF-8 encoding
$outputData | Export-Csv -Path $outputCsv -NoTypeInformation -Encoding UTF8

Write-Host "Data exported to CSV file at: $outputCsv"
