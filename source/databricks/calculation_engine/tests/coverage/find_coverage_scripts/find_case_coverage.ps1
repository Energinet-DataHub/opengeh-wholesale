# Get the absolute path of the currently running script
$scriptPath = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

# Define the output CSV file path relative to the script location
$outputCsv = Join-Path -Path $scriptPath -ChildPath "..\find_coverage_script_output\case_coverage.csv"

# Create an empty array to store the rows
$rows = @()

# Set the root path to the directory where the search for "given_*" folders should start
$rootPath = Join-Path -Path $scriptPath -ChildPath "..\..\"

# Debug: Output the root path
Write-Host "Root path: $rootPath"

# Recursively search for all "given_" folders at any depth starting from $rootPath
$givenFolders = Get-ChildItem -Path $rootPath -Recurse -Directory | Where-Object { $_.Name -like "given_*" }

# Debug: Output the number of "given_" folders found
Write-Host "Found $( $givenFolders.Count ) 'given_' folders."

# Loop through each "given_" folder
foreach ($givenFolder in $givenFolders)
{
    # Compute the relative path starting from 'tests' folder
    $testsIndex = $givenFolder.FullName.IndexOf("tests")
    $relativePath = $givenFolder.FullName.Substring($testsIndex + "tests".Length)

    # Ensure the relative path starts with a backslash for consistent formatting
    if (-not $relativePath.StartsWith("\"))
    {
        $relativePath = "\" + $relativePath
    }

    Write-Host "Processing 'given_' folder: $relativePath"

    # Look for the 'Coverage.py' file in each 'given_' folder
    $coverageFile = Get-ChildItem -Path $givenFolder.FullName -File | Where-Object { $_.Name -eq "Coverage.py" }

    if ($coverageFile)
    {
        $coverageContent = Get-Content -Path $coverageFile.FullName

        foreach ($line in $coverageContent)
        {
            if ( $line.Trim().StartsWith("Cases."))
            {
                # Extract the specific part after "Tests."
                $caseCoverage = $line.Trim().Substring("Cases.".Length)

                # Extract the last part of the test case string after the last '.'
                $testCaseName = $caseCoverage.Split('.')[-1]

                # Create a row for each valid statement with the base path and test case name
                $rows += [PSCustomObject]@{
                    Scenario = $relativePath
                    CaseCoverage = $testCaseName
                }
            }
        }
    }
    else
    {
        Write-Host "No 'Coverage.py' file found in $relativePath."
    }
}

# Check if any rows were added
if ($rows.Count -eq 0)
{
    Write-Host "No case coverage data found."
}
else
{
    # Convert the objects to CSV format and then trim the end to remove extra newlines
    $csvContent = $rows | ConvertTo-Csv -NoTypeInformation | Out-String
    $csvContent = $csvContent.TrimEnd("`r`n")  # Ensure no trailing newlines

    # Write the CSV content to the file, ensuring file is created even if empty
    Set-Content -Path $outputCsv -Value $csvContent -NoNewline
    Write-Host "Case coverage data has been written and saved to $outputCsv"
}
