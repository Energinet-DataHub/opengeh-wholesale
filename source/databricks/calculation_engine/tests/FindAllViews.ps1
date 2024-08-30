# Get the absolute path of the currently running script
$scriptPath = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

# Correctly setting the relative path to the target directory
$targetDirectory = Join-Path -Path $scriptPath -ChildPath "..\contracts\data_products"

# Prepare an array to hold the output data
$fileList = @()

# Validate the path before proceeding
if (Test-Path -Path $targetDirectory) {
    # Get all files in the target directory and its subdirectories
    $files = Get-ChildItem -Path $targetDirectory -Recurse -File

    # Collect each filename without its extension
    foreach ($file in $files) {
        $filenameWithoutExtension = [System.IO.Path]::GetFileNameWithoutExtension($file.FullName)
        # Add the filename to the array as a custom object
        $fileList += [PSCustomObject]@{
            FileName = $filenameWithoutExtension
        }
    }
    
    # Export the list to a CSV file
    $csvPath = Join-Path -Path $scriptPath -ChildPath "AllViews.csv"
    $fileList | Export-Csv -Path $csvPath -NoTypeInformation
    Write-Host "File names have been written to $csvPath"
} else {
    Write-Error "The specified path does not exist: $targetDirectory"
}
