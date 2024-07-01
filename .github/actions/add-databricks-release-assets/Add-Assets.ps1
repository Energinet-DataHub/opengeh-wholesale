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

    $destination_hive = "${WorkingDirectory}/artifacts/hive"
    if ((Test-Path -Path $destination_hive) -eq $false) {
        New-Item -Path $destination_hive -ItemType 'directory'
    }
    Move-Item -Path "${WorkingDirectory}/package/datamigration_hive/migration_scripts" -Destination $destination_hive

    $destination_unity_catalog = "${WorkingDirectory}/artifacts"
    if ((Test-Path -Path $destination_unity_catalog) -eq $false) {
        New-Item -Path $destination_unity_catalog -ItemType 'directory'
    }
    Move-Item -Path "${WorkingDirectory}/package/datamigration/migration_scripts" -Destination $destination_unity_catalog
}
