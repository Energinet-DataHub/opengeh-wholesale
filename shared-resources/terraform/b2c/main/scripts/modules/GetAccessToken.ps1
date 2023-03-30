# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

<#
    .SYNOPSIS
    Gets access token to Graph API using ClientId/ClientSecret.
#>
function Get-AccessToken {
    param (
        [Parameter(Mandatory)]
        [string]
        $B2CTenantId,
        [Parameter(Mandatory)]
        [string]
        $B2CClientId,
        [Parameter(Mandatory)]
        [string]
        $B2CClientSecret
    )

    Write-Information "Getting access token to B2C Graph API"

    $body = @{
        grant_type    = "client_credentials"
        scope         = "https://graph.microsoft.com/.default"
        client_id     = $B2CClientId
        client_secret = $B2CClientSecret
    }

    $response = Invoke-RestMethod `
        -Uri "https://login.microsoftonline.com/$B2CTenantId/oauth2/v2.0/token" -Method Post `
        -Body $body

    Write-Information "Access token to B2C GraphAPI obtained"
    return [string]$response.access_token
}
