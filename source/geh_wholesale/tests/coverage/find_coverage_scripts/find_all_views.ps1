# Get the absolute path of the currently running script
$scriptPath = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

# Correctly setting the relative path to the target directory
$targetDirectory = Join-Path -Path $scriptPath -ChildPath "..\..\..\contracts\data_products"

# Prepare an array to hold the output data
$fileList = @()

# Validate the path before proceeding
if (Test-Path -Path $targetDirectory)
{
    # Get all files in the target directory and its subdirectories, excluding "__pycache__" directories
    $files = Get-ChildItem -Path $targetDirectory -Recurse -File | Where-Object {
        $_.DirectoryName -notlike "*__pycache__*"
    }

    # Collect each filename without its extension and its parent directory name
    foreach ($file in $files)
    {
        $filenameWithoutExtension = [System.IO.Path]::GetFileNameWithoutExtension($file.FullName)
        $parentDirectoryName = Split-Path -Path $file.FullName -Parent | Split-Path -Leaf

        # Add the filename and its parent directory name to the array as a custom object
        $fileList += [PSCustomObject]@{
            Path = $parentDirectoryName
            FileName = $filenameWithoutExtension
        }
    }

    # Export the list to a CSV file without an extra newline at the end
    $csvPath = Join-Path -Path $scriptPath -ChildPath "..\find_coverage_script_output\all_views.csv"

    # Convert the objects to CSV format, skip the last newline, and make sure no trailing newline is added
    $csvContent = $fileList | ConvertTo-Csv -NoTypeInformation | Out-String
    $csvContent = $csvContent.TrimEnd("`r`n")  # Ensure no trailing newlines
    Set-Content -Path $csvPath -Value $csvContent -NoNewline

    Write-Host "File names have been written to $csvPath without an extra newline."
}
else
{
    Write-Error "The specified path does not exist: $targetDirectory"
}