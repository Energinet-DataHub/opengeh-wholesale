# Get the absolute path of the currently running script
$scriptPath = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

# Define the output CSV file path relative to the script location
$outputCsv = Join-Path -Path $scriptPath -ChildPath "CaseCoverage.csv"

# Create an empty array to store the rows
$rows = @()

# Get the current directory where the script is running to search for "given_*" folders
$rootPath = Get-Location

# Recursively search for all "given_" folders at any depth
$givenFolders = Get-ChildItem -Path $rootPath -Recurse -Directory | Where-Object { $_.Name -like "given_*" }

# Loop through each "given_" folder
foreach ($givenFolder in $givenFolders) {
    $relativePath = $givenFolder.FullName.Substring($rootPath.Path.Length + 1)

    # Look for the 'Coverage.py' file in each 'given_' folder
    $coverageFile = Get-ChildItem -Path $givenFolder.FullName -File | Where-Object { $_.Name -eq "Coverage.py" }

    if ($coverageFile) {
        $coverageContent = Get-Content -Path $coverageFile.FullName

        foreach ($line in $coverageContent) {
            if ($line.Trim().StartsWith("Tests.")) {
                # Extracting the specific part after "Tests."
                $caseCoverage = $line.Trim().Substring("Tests.".Length)

                # Create a row for each valid statement
                $rows += [PSCustomObject]@{
                    Scenario = $relativePath
                    CaseCoverage = $caseCoverage
                }
            }
        }
    } else {
        Write-Host "No 'Coverage.py' file found in $relativePath."
    }
}

# Convert the objects to CSV format and then trim the end to remove extra newlines
$csvContent = $rows | ConvertTo-Csv -NoTypeInformation | Out-String
$csvContent = $csvContent.TrimEnd("`r`n")  # Ensure no trailing newlines

# Write the CSV content to the file, ensuring file is created even if empty
Set-Content -Path $outputCsv -Value $csvContent -NoNewline
Write-Host "Case coverage data has been written and saved to $outputCsv"
