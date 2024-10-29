# Get the absolute path of the currently running script.
$scriptPath = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

# Define the relative path to the 'find_coverage_scripts' directory
$coverageScriptsPath = Join-Path -Path $scriptPath -ChildPath "find_coverage_scripts"

# Validate that the directory exists
if (-Not (Test-Path -Path $coverageScriptsPath))
{
    Write-Error "The specified directory does not exist: $coverageScriptsPath"
    exit
}

# Get all PowerShell script files (.ps1) in the specified directory, excluding 'update_covernator.ps1'
$scriptFiles = Get-ChildItem -Path $coverageScriptsPath -Filter "*.ps1" -Recurse |
               Where-Object { $_.Name -ne 'update_covernator.ps1' }

# Check if any scripts were found
if ($scriptFiles.Count -eq 0)
{
    Write-Host "No other PowerShell scripts found in the directory: $coverageScriptsPath"
}
else
{
    # Loop through each script and execute it, except 'update_covernator.ps1'
    foreach ($scriptFile in $scriptFiles)
    {
        Write-Host "Running script: $($scriptFile.FullName)"
        try
        {
            # Execute the script
            & $scriptFile.FullName
            Write-Host "Script executed successfully: $($scriptFile.FullName)"
        }
        catch
        {
            Write-Error "An error occurred while executing script: $($scriptFile.FullName)"
            Write-Error $_.Exception.Message
        }
    }
}

# Finally, run 'update_covernator.ps1' last if it exists
$updateScriptPath = Join-Path -Path $coverageScriptsPath -ChildPath "update_covernator.ps1"
if (Test-Path -Path $updateScriptPath)
{
    Write-Host "Running script: $updateScriptPath"
    try
    {
        # Execute the 'update_covernator.ps1' script
        & $updateScriptPath
        Write-Host "Script executed successfully: $updateScriptPath"
    }
    catch
    {
        Write-Error "An error occurred while executing script: $updateScriptPath"
        Write-Error $_.Exception.Message
    }
}
else
{
    Write-Host "'update_covernator.ps1' does not exist in the directory: $coverageScriptsPath"
}

Write-Host "All scripts have been executed."
