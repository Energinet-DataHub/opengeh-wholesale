# Set the path to the Python file relative to the script location
$scriptPath = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$pythonFilePath = Join-Path -Path $scriptPath -ChildPath "..\..\..\package\calculation\calculation_output.py"

# Debug: Output the full path of the Python file being read
Write-Host "Reading Python file at: $pythonFilePath"

# Read the contents of the Python file
$fileContent = Get-Content -Path $pythonFilePath -Raw

# Debug: Check if the file content is being loaded
if ([string]::IsNullOrWhiteSpace($fileContent)) {
    Write-Host "No content found in the file."
} else {
    Write-Host "File content loaded successfully."
}

# Prepare to capture field names for each class
$classNames = @("EnergyResultsOutput", "WholesaleResultsOutput")
$fieldsList = @()

# Define a mapping from class name to the desired output value
$classMapping = @{
    "EnergyResultsOutput" = "energy_calculation"
    "WholesaleResultsOutput" = "wholesale_calculation"
}

foreach ($className in $classNames) {
    # Regex to find the class definition block and the fields directly below it
    $classPattern = "(?sm)@dataclass\s*class\s+$className\s*:\s*([\s\S]*?)^(?=@dataclass|\Z)"
    if ($fileContent -match $classPattern) {
        $classContent = $Matches[1]
        # Debug: Output the content found for the class block
        Write-Host "Found content for class ${className}: $classContent"

        # Regex to find fields defined directly in the class (assuming Python style of simple declaration)
        $fieldPattern = "^\s*(\w+):"
        $fieldMatches = [regex]::Matches($classContent, $fieldPattern, [System.Text.RegularExpressions.RegexOptions]::Multiline)
        foreach ($match in $fieldMatches) {
            # Use the mapping to determine the correct output value for the "Class" column
            $mappedClassName = $classMapping[$className]

            # Add each field to the list as a custom object
            $fieldsList += [PSCustomObject]@{
                Class = $mappedClassName
                Field = $match.Groups[1].Value
            }
        }
    } else {
        # Debug: Notify if no content found for the class
        Write-Host "No content found for class ${className}."
    }
}

# Export the collected field names to a CSV file without an extra newline at the end
$csvPath = Join-Path -Path $scriptPath -ChildPath "..\find_coverage_script_output\all_results.csv"

# Convert the objects to CSV format, skip the last newline, and make sure no trailing newline is added
$csvContent = $fieldsList | ConvertTo-Csv -NoTypeInformation | Out-String
$csvContent = $csvContent.TrimEnd("`r`n")  # Ensure no trailing newlines

# Write the CSV content to the file
Set-Content -Path $csvPath -Value $csvContent -NoNewline
Write-Host "Field names have been written to $csvPath without an extra newline."
