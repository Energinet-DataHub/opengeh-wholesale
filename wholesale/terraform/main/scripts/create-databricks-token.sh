#!/bin/bash

#Read arguments
workspaceUrl=${workspaceUrl}
lifetimeSeconds=${lifetimeSeconds}
tenantId=${tenantId}
clientId=${clientId}
clientSecret=${clientSecret}
keyvaultName=${keyvaultName}
keyvaultSecretName=${keyvaultSecretName}

# Request an access token for the service principal
echo 'Issuing token...'
aadToken=$(curl -X POST -H 'Content-Type: application/x-www-form-urlencoded' \
https://login.microsoftonline.com/$tenantId/oauth2/v2.0/token \
-d "client_id=$clientId" \
-d 'grant_type=client_credentials' \
-d 'scope=2ff814a6-3304-4ab8-85cb-cd0e6f879c1d%2F.default' \
-d "client_secret=$clientSecret" | jq -r '.access_token')

# Authorize the API request using Databricks SPN access token
headers="Authorization: Bearer $aadToken"

# Create the JSON body for issuing a Databricks token
body=$(jq -n \
    --arg lifetime_seconds "$lifetimeSeconds" \
    --arg comment "Created by Terraform" \
    '{
        lifetime_seconds: $lifetime_seconds,
        comment: $comment
    }')

echo 'Creating token for SPN...'
# Request token from Databricks API
token=$(curl -s -X POST "https://$workspaceUrl/api/2.0/token/create" \
    -H "$headers" \
    -H "Content-Type: application/json" \
    -d "$body")

echo "Token for SPN created"
tk=$(jq -r '.token_value' <<< "$token")

# Persist secret in internal keyvault from where it can be retrieved and sent to external consumers
echo "Persisting token $keyvaultSecretName in $keyvaultName..."
az keyvault secret set --vault-name $keyvaultName --name $keyvaultSecretName  --value $tk >/dev/null
echo 'Token persisted'

# Return an output to save in state file otherwise 'terraform apply' will output changes on every run
echo $(jq -n '{"token_generated": "true"}')


