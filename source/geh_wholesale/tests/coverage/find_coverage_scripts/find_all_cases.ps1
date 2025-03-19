# Define the path to the Python file relative to the script
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$parentDirectory = Split-Path -Parent $scriptDirectory
$pythonFilePath = Join-Path $parentDirectory "all_test_cases.py"

# Define the output directory and ensure it exists
$outputDirectory = Join-Path $parentDirectory "find_coverage_script_output"
if (-Not (Test-Path $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
}

# Define the output CSV file path
$csvFilePath = Join-Path $outputDirectory "all_cases.csv"

# Ensure that the Python file path is valid
if (-Not (Test-Path $pythonFilePath)) {
    Write-Error "The specified Python file path does not exist: $pythonFilePath"
    exit
}

# Read the contents of the Python file
$pythonFileContent = Get-Content -Path $pythonFilePath -Raw

# Function to parse the Python class structure and output it with test cases to a CSV file
function Parse-PythonClass {
    param (
        [string]$content
    )

    $classStack = New-Object System.Collections.Generic.List[string]
    $classStack.Add("Cases") # Start with Root as the base path
    $lines = $content -split "`r`n" # Correctly split on new lines
    $currentIndent = 0
    $output = @()

    foreach ($line in $lines) {
        if ($line.Trim() -eq '') { continue } # Skip empty lines

        $newIndent = ($line.Length - $line.TrimStart().Length) / 4 # Assuming 4 spaces per indentation level

        if ($line -match '^\s*class\s+([\w-]+)') {
            if ($newIndent -le $currentIndent) {
                # Remove levels from stack according to the current indentation level
                $diff = $currentIndent - $newIndent + 1
                $classStack.RemoveRange($classStack.Count - $diff, $diff)
            }
            $className = $matches[1]
            $classStack.Add($className)
            $currentPath = $classStack -join '.'
            $currentIndent = $newIndent
        }
        elseif ($line -match '^\s*([\w-]+)\s*:\s*str') {
            $testCaseName = $matches[1]
            $output += [PSCustomObject]@{
                Path = $currentPath
                TestCase = $testCaseName
            }
        }
    }

    # Export the collected data to CSV
    $output | Export-Csv -Path $csvFilePath -NoTypeInformation
}

# Run the parsing function
Parse-PythonClass -content $pythonFileContent
