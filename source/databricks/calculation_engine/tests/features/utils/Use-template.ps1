# PowerShell script to overwrite 'test_output.py' with 'template.py' contents

# Define the path to the template file
$templateFilePath = Join-Path $PSScriptRoot "template.py"

# Ensure the template file exists
if (-Not (Test-Path -Path $templateFilePath)) {
    Write-Error "Template file does not exist: $templateFilePath"
    exit 1
}

# Get the content of the template file
$templateContent = Get-Content -Path $templateFilePath -Raw

# Define the parent directory of the current directory
$featuresDir = Split-Path -Path $PSScriptRoot -Parent

# Search for all 'test_output.py' files in the subdirectories of the parent directory
$filesToUpdate = Get-ChildItem -Path $featuresDir -Filter "test_output.py" -Recurse

# Iterate over each found 'test_output.py' file
foreach ($file in $filesToUpdate) {
    # Overwrite the file with the content of 'template.py'
    Set-Content -Path $file.FullName -Value $templateContent
    Write-Output "Updated: $($file.FullName)"
}

Write-Output "All files have been updated successfully."
