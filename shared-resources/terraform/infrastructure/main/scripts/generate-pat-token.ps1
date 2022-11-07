$databricksWorkspaceResourceId = $args[0]
$databricksEndpoint = $args[1]

# Must be run on a machine that is already logged in to Azure

# Get a token for the global Databricks application.
# The resource name is fixed and never changes.
$aadToken=$(az account get-access-token --resource=2ff814a6-3304-4ab8-85cb-cd0e6f879c1d --query accessToken --output tsv)

# Get a token for the Azure management API
$azToken=$(az account get-access-token --resource=https://management.core.windows.net/ --query accessToken --output tsv)

$uri        = "$databricksEndpoint/api/2.0/token/create"
$headers    = @{
                'Authorization' = "Bearer $aadToken"
                'X-Databricks-Azure-SP-Management-Token' = $azToken
                'X-Databricks-Azure-Workspace-Resource-Id' = $databricksWorkspaceResourceId
            }

$patToken = (Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $body).token_value

$patTokenJson = @"
{
"pat_token": "$patToken"
}
"@

return $patTokenJson