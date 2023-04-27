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
    Create and configures a new OpenId identity provider.

    .DESCRIPTION
    Create and configures a new OpenId identity provider.
    If the identity provider with the specified identifier already exists, it will instead be updated with the new configuration.

    NOTE: OpenIdConfigurationUrl cannot be changed, only set during initial creation.
    Specifying a different url will throw an exception.
#>
function New-OpenIdProvider {
    param (
        [Parameter(Mandatory)]
        [string]
        $AccessToken,
        [Parameter(Mandatory)]
        [string]
        $Identifier,
        [Parameter(Mandatory)]
        [string]
        $OpenIdConfigurationUrl,
        [Parameter(Mandatory)]
        [string]
        $OpenIdConfigurationClientId,
        [Parameter(Mandatory)]
        [string]
        $OpenIdConfigurationClientSecret,
        [Parameter(Mandatory)]
        [string]
        $OpenIdConfigurationUserIdClaimName,
        [Parameter(Mandatory)]
        [string]
        $OpenIdConfigurationDisplayNameClaimName,
        [Parameter(Mandatory)]
        [string]
        $OpenIdConfigurationResponseMode,
        [Parameter(Mandatory)]
        [string]
        $OpenIdConfigurationResponseType,
        [Parameter(Mandatory)]
        [string]
        $OpenIdConfigurationScopes
    )

    $openIdConfiguration = @{
        clientId      = $OpenIdConfigurationClientId
        clientSecret  = $OpenIdConfigurationClientSecret
        claimsMapping = @{
            userId      = $OpenIdConfigurationUserIdClaimName
            displayName = $OpenIdConfigurationDisplayNameClaimName
        }
        responseMode  = $OpenIdConfigurationResponseMode
        responseType  = $OpenIdConfigurationResponseType
        scope         = $OpenIdConfigurationScopes
    }

    $existingProvider = Invoke-GetOpenIdProvider `
        -AccessToken $AccessToken `
        -Identifier $Identifier `
        -MetadataUrl $OpenIdConfigurationUrl

    $existingId = $existingProvider.id

    if ($existingProvider.exists -ne $true) {
        return Invoke-CreateOpenIdProvider `
            -AccessToken $AccessToken `
            -Identifier $Identifier `
            -MetadataUrl $OpenIdConfigurationUrl `
            -OpenIdConfiguration $openIdConfiguration
    }

    Invoke-UpdateOpenIdProvider `
        -AccessToken $AccessToken `
        -Id $existingId `
        -OpenIdConfiguration $openIdConfiguration

    return $existingId
}

<#
    .SYNOPSIS
    Gets the OpenID identity provider with the given identifier. If the identity provider is found, its id is returned.
#>
function Invoke-GetOpenIdProvider {
    param (
        [Parameter(Mandatory)]
        [string]
        $AccessToken,
        [Parameter(Mandatory)]
        [string]
        $Identifier,
        [Parameter(Mandatory)]
        [string]
        $MetadataUrl
    )

    Write-Information "Searching for identity provider with '$Identifier' identifier"

    $headers = @{
        Authorization = "Bearer $AccessToken"
    }

    $existingProviders = Invoke-RestMethod `
        -Uri "https://graph.microsoft.com/beta/identity/identityProviders" -Method Get `
        -Headers $headers

    foreach ($idProvider in $existingProviders.value) {

        if ($idProvider.displayName -eq $Identifier -and
            $idProvider."@odata.type" -eq "#microsoft.graph.openIdConnectIdentityProvider") {

            if ($idProvider.metadataUrl -ne $MetadataUrl) {
                throw "Cannot change metadata url on an existing OpenId identity provider."
            }

            Write-Information "Found identity provider with '${$idProvider.id}' id"
            return @{
                exists = $true
                id     = $idProvider.id
            }
        }
    }

    return @{ exists = $false }
}

<#
    .SYNOPSIS
    Creates a new OpenID provider, using the specified configuration.
#>
function Invoke-CreateOpenIdProvider {
    param (
        [Parameter(Mandatory)]
        [string]
        $AccessToken,
        [Parameter(Mandatory)]
        [string]
        $Identifier,
        [Parameter(Mandatory)]
        [string]
        $MetadataUrl,
        [Parameter(Mandatory)]
        [Hashtable]
        $OpenIdConfiguration
    )

    Write-Information "Creating OpenId identity provider with identifier '$Identifier'"

    $headers = @{
        Authorization = "Bearer $AccessToken"
    }

    $body = @{
        "@odata.type" = "#microsoft.graph.openIdConnectIdentityProvider"
        displayName   = $Identifier
        metadataUrl   = $MetadataUrl
    }

    $body += $OpenIdConfiguration

    $response = Invoke-RestMethod `
        -Uri "https://graph.microsoft.com/beta/identity/identityProviders" -Method Post `
        -Headers $headers `
        -ContentType "application/json" `
        -Body ($body | ConvertTo-Json)

    Write-Information "Created OpenId identity provider with id '${$response.id}'"
    return $response.id
}

<#
    .SYNOPSIS
    Updates the configuration of the OpenID identity provider with the specified id.
#>
function Invoke-UpdateOpenIdProvider {
    param (
        [Parameter(Mandatory)]
        [string]
        $AccessToken,
        [Parameter(Mandatory)]
        [string]
        $Id,
        [Parameter(Mandatory)]
        [Hashtable]
        $OpenIdConfiguration
    )

    Write-Information "Updating OpenId identity provider with id '$Id'"

    $headers = @{
        Authorization = "Bearer $AccessToken"
    }

    $body = @{
        "@odata.type" = "#microsoft.graph.openIdConnectIdentityProvider"
    }

    $body += $OpenIdConfiguration

    Invoke-RestMethod `
        -Uri "https://graph.microsoft.com/beta/identity/identityProviders/$Id" -Method Patch `
        -Headers $headers `
        -ContentType "application/json" `
        -Body ($body | ConvertTo-Json) | Out-Null
}
