#
# The script will always be called by the module's null_resource.
# If any of the parameters have invalid values (i.e. null or white space), the script is terminated using `Return`
# which will not cause the deployment pipeline to fail. If Exit or an exception was thrown, the pipeline would fail.
#

param (
    [Parameter(Mandatory = $true)]
    [string]$sqlServerName,
    [Parameter(Mandatory = $true)]
    [string]$resourceGroupName,
    [Parameter(Mandatory = $false)]
    [string]$adGroupDirectoryReader
)

if ([string]::IsNullOrWhiteSpace($sqlServerName))
{
    Write-Host "SQL server name null or whitespace"
    Return
}

if ([string]::IsNullOrWhiteSpace($resourceGroupName))
{
    Write-Host "Resource group name null or whitespace"
    Return
}

if ([string]::IsNullOrWhiteSpace($adGroupDirectoryReader))
{
    Write-Host "AD group name null or whitespace"
    Return
}

$sqlServerIdentityObjectId = az resource show --name "$sqlServerName" --resource-group "$resourceGroupName" --resource-type "Microsoft.Sql/servers" --query "identity.principalId" | ConvertFrom-Json

$isMember = az ad group member check --group "$adGroupDirectoryReader" --member-id "$sqlServerIdentityObjectId" --query "value" | ConvertFrom-Json

if ($isMember -eq $False)
{
    az ad group member add --group "$adGroupDirectoryReader" --member-id "$sqlServerIdentityObjectId"
    Write-Host "Identity of $sqlServerName is added to AD group $adGroupDirectoryReader"
}
else
{
    Write-Host "Identity of $sqlServerName is already member of AD group $adGroupDirectoryReader"
}