# Define paths for the input Python file and the output CSV file
$pythonFilePath = "../all_test_cases_test.py"
$outputCsvPath = "../find_coverage_script_output/all_cases_test.csv"

# Read the Python file content
$pythonFileContent = Get-Content -Path $pythonFilePath

# Initialize parsing and path tracking variables
$startParsing = $false
$classPathStack = @()  # Initialize empty, will add 'Root' on first class detection
$csvContent = "`"Path`",`"TestCase`"`n"
$indentationStack = @()

# Process each line to extract classes and test cases
foreach ($line in $pythonFileContent) {
    if (-not $startParsing -and $line -match '@dataclass') {
        $startParsing = $true
        continue
    }
    if ($startParsing) {
        $currentIndentation = ($line -replace '\t', '    ').IndexOf($line.Trim()) / 4  # Assume 4 spaces per tab
        if ($line -match 'class\s+([\w-]+):') {
            # Adjust the class path based on current indentation
            while ($indentationStack.Count -gt 0 -and $currentIndentation -le $indentationStack[-1]) {
                $indentationStack = $indentationStack[0..($indentationStack.Count-2)]
                $classPathStack = $classPathStack[0..($classPathStack.Count-2)]
            }
            $className = $matches[1]  # Keep original class name with hyphen
            if ($classPathStack.Count -eq 0) {
                $classPathStack += 'Root'  # Add 'Root' only once at the start
            }
            $classPathStack += $className
            $indentationStack += $currentIndentation
        } elseif ($line -match '(\w+):\s+str') {
            $testCase = $matches[1]
            $path = $classPathStack -join '.'
            $csvContent += "`"$path`",`"$testCase`"`n"
        }
    }
}

# Write the resulting CSV content to the output file
$dirPath = Split-Path -Path $outputCsvPath -Parent
if (-Not (Test-Path -Path $dirPath)) {
    New-Item -Path $dirPath -ItemType Directory
}
Set-Content -Path $outputCsvPath -Value $csvContent
