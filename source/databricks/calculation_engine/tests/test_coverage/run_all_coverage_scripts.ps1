# Get the absolute path of the currently running script
$scriptPath = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

# Define the relative path to the 'find_coverage_scripts' directory
$coverageScriptsPath = Join-Path -Path $scriptPath -ChildPath "find_coverage_scripts"

# Validate that the directory exists
if (-Not (Test-Path -Path $coverageScriptsPath))
{
    Write-Error "The specified directory does not exist: $coverageScriptsPath"
    exit
}

# Get all PowerShell script files (.ps1) in the specified directory
$scriptFiles = Get-ChildItem -Path $coverageScriptsPath -Filter "*.ps1" -Recurse

# Check if any scripts were found
if ($scriptFiles.Count -eq 0)
{
    Write-Host "No PowerShell scripts found in the directory: $coverageScriptsPath"
    exit
}

# Loop through each script and execute it
foreach ($scriptFile in $scriptFiles)
{
    Write-Host "Running script: $( $scriptFile.FullName )"

    try
    {
        # Execute the script
        & $scriptFile.FullName
        Write-Host "Script executed successfully: $( $scriptFile.FullName )"
    }
    catch
    {
        Write-Error "An error occurred while executing script: $( $scriptFile.FullName )"
        Write-Error $_.Exception.Message
    }
}

Write-Host "All scripts have been executed."
