# Define the output CSV file path
$scriptPath = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$outputCsv = Join-Path -Path $scriptPath -ChildPath "..\find_coverage_script_output\results_coverage.csv"

# Prepare an array to store the results
$rows = @()

# Get the root directory where the script is running
$rootPath = Get-Location

# Recursively search for all "given_*" folders at any depth
$givenFolders = Get-ChildItem -Path $rootPath -Recurse -Directory | Where-Object { $_.Name -like "given_*" }

# Loop through each "given_" folder
foreach ($givenFolder in $givenFolders) {
    # Extract only the name of the "given_" folder for the "Scenario" column
    $scenarioName = $givenFolder.Name

    Write-Host "Processing 'given_' folder: $scenarioName"

    # Look for the 'then' folder inside the 'given_' folder
    $thenFolder = Get-ChildItem -Path $givenFolder.FullName -Directory | Where-Object { $_.Name -eq "then" }

    if ($thenFolder) {
        # Get all CSV files directly under the 'then' folder
        $resultFiles = Get-ChildItem -Path $thenFolder.FullName -File | Where-Object { $_.Extension -eq ".csv" }

        # Loop through each file directly in the 'then' folder
        foreach ($file in $resultFiles) {
            $fileNameWithoutExtension = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)

            # Create a row for each file found
            $rows += [PSCustomObject]@{
                Scenario = $scenarioName
                Results = $fileNameWithoutExtension
            }

            Write-Host "Adding row: $scenarioName | Result | $fileNameWithoutExtension"
        }

        # Look for all folders ending with "_results" inside the 'then' folder
        $resultsFolders = Get-ChildItem -Path $thenFolder.FullName -Directory | Where-Object { $_.Name -like "*_results" }

        # Loop through each '_results' folder
        foreach ($resultsFolder in $resultsFolders) {
            # Get all filenames in the '_results' folder
            $resultFiles = Get-ChildItem -Path $resultsFolder.FullName -File

            # Loop through each file in the '_results' folder
            foreach ($file in $resultFiles) {
                $fileNameWithoutExtension = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)

                # Create a row for each file found
                $rows += [PSCustomObject]@{
                    Scenario = $scenarioName
                    Results = $fileNameWithoutExtension
                }

                Write-Host "Adding row: $scenarioName | Result | $fileNameWithoutExtension"
            }
        }
    } else {
        Write-Host "No 'then' folder found in $scenarioName."
    }
}

# Convert the objects to CSV format, remove extra newlines, and write to the file
if ($rows.Count -eq 0) {
    Write-Host "No rows were added to the CSV file."
} else {
    $csvContent = $rows | ConvertTo-Csv -NoTypeInformation | Out-String
    $csvContent = $csvContent.TrimEnd("`r`n")  # Ensure no trailing newlines
    Set-Content -Path $outputCsv -Value $csvContent -NoNewline
    Write-Host "CSV file generated and saved to $outputCsv"
}
