# Define the output CSV file path
$outputCsv = "coverage_overview_output.csv"

# Create an empty array to store the rows
$rows = @()

# Get the current directory where the script is running
$rootPath = Get-Location

# Recursively search for all "given_" folders at any depth
$givenFolders = Get-ChildItem -Path $rootPath -Recurse -Directory | Where-Object { $_.Name -like "given_*" }

# Check if any 'given_' folders were found
if ($givenFolders.Count -eq 0) {
    Write-Host "No 'given_' folders found."
}

# Loop through each "given_" folder
foreach ($givenFolder in $givenFolders) {
    # Create the relative path for the "Test" column
    $relativePath = $givenFolder.FullName.Substring($rootPath.Path.Length + 1)

    Write-Host "Processing 'given_' folder: $relativePath"

    # Look for the 'then' folder inside the 'given_' folder
    $thenFolder = Get-ChildItem -Path $givenFolder.FullName -Directory | Where-Object { $_.Name -eq "then" }

    if ($thenFolder) {
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
                $row = [PSCustomObject]@{
                    Test = $relativePath
                    Type = "Calculation Result"
                    Data = $fileNameWithoutExtension
                }

                # Add the row to the array
                $rows += $row

                Write-Host "Adding row: $relativePath | Calculation Result | $fileNameWithoutExtension"
            }
        }
    } else {
        Write-Host "No 'then' folder found in $relativePath."
    }

    # Look for 'readme.md' in the 'given_' folder
    $readmeFile = Get-ChildItem -Path $givenFolder.FullName -File | Where-Object { $_.Name -eq "readme.md" }

    if ($readmeFile) {
        # Read the content of the 'readme.md' file
        $readmeContent = Get-Content -Path $readmeFile.FullName

        # Flag to track if we're in the "## Coverage" section
        $inCoverageSection = $false

        # Loop through each line in the readme file
        foreach ($line in $readmeContent) {
            # Check if the line contains "## Coverage"
            if ($line -match "## Coverage") {
                $inCoverageSection = $true
                continue
            }

            # If we are in the "## Coverage" section, look for bullet points
            if ($inCoverageSection -and $line.TrimStart() -match "^- ") {
                # Extract the bullet point text
                $coverageItem = $line.TrimStart().Substring(2).Trim()

                # Create a row for each coverage item found
                $row = [PSCustomObject]@{
                    Test = $relativePath
                    Type = "Scenario"
                    Data = $coverageItem
                }

                # Add the row to the array
                $rows += $row

                Write-Host "Adding row: $relativePath | Scenario | $coverageItem"
            }

            # Exit the "## Coverage" section when hitting another heading
            if ($inCoverageSection -and $line.StartsWith("## ") -and -not $line.Contains("Coverage")) {
                $inCoverageSection = $false
            }
        }
    } else {
        Write-Host "No 'readme.md' file found in $relativePath."
    }
}

# Check if any rows were added
if ($rows.Count -eq 0) {
    Write-Host "No rows were added to the CSV file."
} else {
    # Export the array to a CSV file with headers
    $rows | Export-Csv -Path $outputCsv -NoTypeInformation -Force

    # Inform the user where the CSV file was saved
    Write-Host "CSV file generated and saved to $outputCsv"
}
