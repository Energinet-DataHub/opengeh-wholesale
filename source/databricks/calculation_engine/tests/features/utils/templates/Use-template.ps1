# PowerShell script to overwrite 'test_output.py' with 'template.py' contents

function CheckFileExists
{
    param (
        [Parameter(Mandatory = $true)]
        [string]$filePath
    )

    if (-Not (Test-Path -Path $filePath))
    {
        Write-Error "Template file does not exist: $filePath"
        exit 1
    }
}

# Needs to concatenate before applying Join-Path in order to work
# probably on all powershell versions.
$pathToCalculationTestTemplate = "/calculation-test-template.py"
$pathToPublicModelTestTemplate = "/data-product-test-template.py"

# Define the path to the calculation test template file
$calculationTestTemplateFilePath = Join-Path $PSScriptRoot $pathToCalculationTestTemplate

# Define the path to the data product test template file
$publicModelTestTemplateFilePath = Join-Path $PSScriptRoot $pathToPublicModelTestTemplate

# Ensure the template files exists
CheckFileExists -filePath $calculationTestTemplateFilePath
CheckFileExists -filePath $publicModelTestTemplateFilePath


# Get the content of the template files
$calculationTestTemplateContent = Get-Content -Path $calculationTestTemplateFilePath -Raw
$publicModelTestTemplateContent = Get-Content -Path $publicModelTestTemplateFilePath -Raw

# Define the parent directory of the current directory
$featuresDir = Split-Path -Path $PSScriptRoot -Parent

# Search for all 'test_output.py' files in the subdirectories of the parent directory
$filesToUpdate = Get-ChildItem -Path $featuresDir -Filter "test_output.py" -Recurse


# Iterate over each found 'test_output.py' file
foreach ($file in $filesToUpdate)
{
    # Overwrite the file with the content of the template file
    if ( $file.FullName.Contains("_calculation"))
    {
        Set-Content -Path $file.FullName -Value $calculationTestTemplateContent -NoNewLine
    }
    elseif ($file.FullName.Contains("data_products"))
    {
        Set-Content -Path $file.FullName -Value $publicModelTestTemplateContent -NoNewLine
    }
    else
    {
        Write-Error "Invalid file path: $filePath"
        exit 1
    }
    Write-Output "Updated: $( $file.FullName )"
}

Write-Output "All files have been updated successfully."
