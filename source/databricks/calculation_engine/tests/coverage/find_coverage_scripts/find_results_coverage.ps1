# Get the absolute path of the currently running script
$scriptPath = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

# Correctly setting the output CSV path relative to the script location
$outputCsv = Join-Path -Path $scriptPath -ChildPath "..\find_coverage_script_output\results_coverage.csv"

# Set the root path to the directory where the search for "given_*" folders should start
$rootPath = Join-Path -Path $scriptPath -ChildPath "..\..\"

# Debug: Output the root path
Write-Host "Root path: $rootPath"

# Create an empty array to store the rows
$rows = @()

# Recursively search for all "given_" folders at any depth starting from $rootPath
$givenFolders = Get-ChildItem -Path $rootPath -Recurse -Directory | Where-Object { $_.Name -like "given_*" }

# Debug: Output the number of "given_" folders found
Write-Host "Found $( $givenFolders.Count ) 'given_' folders."

# Check if any 'given_' folders were found
if ($givenFolders.Count -eq 0)
{
    Write-Host "No 'given_' folders found."
}
else
{
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

        # Look for the 'then' folder inside the 'given_' folder
        $thenFolder = Get-ChildItem -Path $givenFolder.FullName -Directory | Where-Object { $_.Name -eq "then" }

        if ($thenFolder)
        {
            Write-Host "Found 'then' folder in $relativePath"

            # Get all CSV files directly under the 'then' folder
            $resultFiles = Get-ChildItem -Path $thenFolder.FullName -File | Where-Object { $_.Extension -eq ".csv" }

            # Check if any CSV files are found
            if ($resultFiles.Count -eq 0)
            {
                Write-Host "No CSV files found in 'then' folder of $relativePath"
            }

            # Loop through each file directly in the 'then' folder
            foreach ($file in $resultFiles)
            {
                $fileNameWithoutExtension = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)

                # Extract the test case name from the full path
                $testCaseName = $fileNameWithoutExtension.Split('.')[-1]

                # Create a row for each file found
                $row = [PSCustomObject]@{
                    Scenario = $relativePath
                    Results = $testCaseName
                }

                # Add the row to the array
                $rows += $row

                Write-Host "Adding row: $relativePath | Results | $testCaseName"
            }

            # Look for all folders ending with "_results" inside the 'then' folder
            $resultsFolders = Get-ChildItem -Path $thenFolder.FullName -Directory | Where-Object { $_.Name -like "*_results" }

            # Check if any '_results' folders are found
            if ($resultsFolders.Count -eq 0)
            {
                Write-Host "No '_results' folders found in 'then' folder of $relativePath"
            }

            # Loop through each '_results' folder
            foreach ($resultsFolder in $resultsFolders)
            {
                Write-Host "Processing '_results' folder: $( $resultsFolder.Name )"

                # Get all filenames in the '_results' folder
                $resultFiles = Get-ChildItem -Path $resultsFolder.FullName -File

                # Check if any files are found in '_results' folder
                if ($resultFiles.Count -eq 0)
                {
                    Write-Host "No files found in '_results' folder: $( $resultsFolder.Name )"
                }

                # Loop through each file in the '_results' folder
                foreach ($file in $resultFiles)
                {
                    $fileNameWithoutExtension = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)

                    # Extract the test case name from the full path
                    $testCaseName = $fileNameWithoutExtension.Split('.')[-1]

                    # Create a row for each file found
                    $row = [PSCustomObject]@{
                        Scenario = $relativePath
                        Results = $testCaseName
                    }

                    # Add the row to the array
                    $rows += $row

                    Write-Host "Adding row: $relativePath | Results | $testCaseName"
                }
            }
        }
        else
        {
            Write-Host "No 'then' folder found in $relativePath."
        }
    }
}

# Check if any rows were added
if ($rows.Count -eq 0)
{
    Write-Host "No rows were added to the CSV file."
}
else
{
    # Convert the objects to CSV format and then trim the end to remove extra newlines
    $csvContent = $rows | ConvertTo-Csv -NoTypeInformation | Out-String
    $csvContent = $csvContent.TrimEnd("`r`n")  # Ensure no trailing newlines

    # Write the CSV content to the file, ensuring file is created even if empty
    Set-Content -Path $outputCsv -Value $csvContent -NoNewline

    # Inform the user where the CSV file was saved
    Write-Host "CSV file generated and saved to $outputCsv"
}
