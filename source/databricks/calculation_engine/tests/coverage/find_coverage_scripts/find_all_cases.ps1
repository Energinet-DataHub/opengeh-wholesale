# Get the absolute path of the currently running script
$scriptPath = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

# Define the input Python file path relative to the script location
$pythonFilePath = Join-Path -Path $scriptPath -ChildPath "..\all_test_cases.py"

# Define the output CSV file path relative to the script location
$outputCsv = Join-Path -Path $scriptPath -ChildPath "..\find_coverage_script_output\all_cases.csv"

# Initialize an empty array to store the rows
$rows = @()

# Read the contents of the Python file
$fileContent = Get-Content -Path $pythonFilePath -Raw

# Function to recursively parse nested classes and test cases
function Parse-ClassContent
{
    param (
        [string]$content,
        [ref]$rowsRef  # Use reference to the array to modify it within the function
    )

    # Regex to match class declarations and test case definitions
    $classPattern = '^\s*class\s+(\w+):'
    $testCasePattern = '^\s+(\w+): str$'

    # Initialize stack to track class hierarchy
    $classStack = [System.Collections.Stack]::new()
    $currentIndent = 0

    foreach ($line in $content -split "`r?`n")
    {
        # Match class declarations
        if ($line -match $classPattern)
        {
            $className = $matches[1]
            $indentation = $line.Length - $line.TrimStart().Length

            # Update stack based on indentation level
            if ($indentation -gt $currentIndent)
            {
                # Entering a new nested class, but skip adding "Tests" to the stack
                if ($className -ne "Tests")
                {
                    $classStack.Push($className)
                }
            }
            elseif ($indentation -eq $currentIndent)
            {
                # Same level class, replace the top of the stack
                if ($classStack.Count -gt 0)
                {
                    $classStack.Pop() | Out-Null
                }
                if ($className -ne "Tests")
                {
                    $classStack.Push($className)
                }
            }
            else
            {
                # Exiting a nested level, adjust stack accordingly
                while ($classStack.Count -gt 0 -and ($indentation -lt $currentIndent))
                {
                    $classStack.Pop() | Out-Null
                    $currentIndent -= 4  # assuming standard Python indentation of 4 spaces
                }
                if ($className -ne "Tests")
                {
                    $classStack.Push($className)
                }
            }

            # Update current indentation level
            $currentIndent = $indentation
        }
        # Match test case definitions
        elseif ($line -match $testCasePattern)
        {
            $testCase = $matches[1]

            # Generate the current path without "Tests" base class
            $currentPath = ($classStack.GetEnumerator() | Sort-Object) -join '.'

            # Add each found test case to the referenced array
            $rowsRef.Value += [PSCustomObject]@{
                Path = $currentPath
                TestCase = $testCase
            }
        }
    }
}

# Parse the entire file content starting from the outermost class
Parse-ClassContent -content $fileContent -rowsRef ([ref]$rows)

# Check if any rows were added
if ($rows.Count -eq 0)
{
    Write-Host "No test cases found in the Python file."
}
else
{
    # Convert the objects to CSV format and then trim the end to remove extra newlines
    $csvContent = $rows | ConvertTo-Csv -NoTypeInformation | Out-String
    $csvContent = $csvContent.TrimEnd("`r`n")  # Ensure no trailing newlines

    # Write the CSV content to the file, ensuring file is created even if empty
    Set-Content -Path $outputCsv -Value $csvContent -NoNewline
    Write-Host "Test cases data has been written and saved to $outputCsv"
}
