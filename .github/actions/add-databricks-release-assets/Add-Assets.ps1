<#
    .SYNOPSIS
    Collect Databricks release artifacts

    .DESCRIPTION
    The script collects python wheel distribution files and Databricks assets in a
    common artifacts folder to be used for Databricks deployment.
#>
function Add-Assets {
    param(
        [Parameter(Mandatory)]
        [string]
        $WorkingDirectory
    )

    $destination = "${WorkingDirectory}/artifacts/calculation_engine"

    if ((Test-Path -Path $destination) -eq $false) {
        New-Item -Path $destination -ItemType 'directory'
    }

    Move-Item -Path "${WorkingDirectory}/calculation_engine/package/datamigration/migration_scripts" -Destination $destination
}
