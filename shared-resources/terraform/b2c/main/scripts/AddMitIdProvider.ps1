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

using module "./modules/ManageUserFlow.psd1"

<#
  .SYNOPSIS
  Creates and configures MitID OpenId identity provider.

  .DESCRIPTION
  Creates and configures MitID OpenId identity provider using Graph API in the specified tenant.
  If the provided ClientId and ClientSecret are changed, the configuration is updated with the new values.

  .EXAMPLE
  PS> ./AddMitIdProvider.ps1 <TenantId> <ClientId> <ClientSecret> <MitIdClientId> <MitIdClientSecret>
#>
param (
    [Parameter(Mandatory)]
    [string]
    $B2CTenantId,
    [Parameter(Mandatory)]
    [string]
    $B2CClientId,
    [Parameter(Mandatory)]
    [string]
    $B2CClientSecret,
    [Parameter(Mandatory)]
    [string]
    $MitIdClientId,
    [Parameter(Mandatory)]
    [string]
    $MitIdClientSecret
)

[string]$accessToken = Get-AccessToken -B2CTenantId $B2CTenantId -B2CClientId $B2CClientId -B2CClientSecret $B2CClientSecret

Write-Information "Creating OpenId provider for MitID"

# NOTE: Changing OpenIdConfigurationUrl is not supported through the API.
# In case of change, increment the identifier version, e.g. MitIDv4 > MitIDv5.
$providerId = New-OpenIdProvider `
    -AccessToken $AccessToken `
    -Identifier "MitIDv1" `
    -OpenIdConfigurationUrl "https://pp.netseidbroker.dk/op/.well-known/openid-configuration" `
    -OpenIdConfigurationClientId $MitIdClientId `
    -OpenIdConfigurationClientSecret $MitIdClientSecret `
    -OpenIdConfigurationUserIdClaimName "sub" `
    -OpenIdConfigurationDisplayNameClaimName "name" `
    -OpenIdConfigurationResponseMode "form_post" `
    -OpenIdConfigurationResponseType "code" `
    -OpenIdConfigurationScopes "openid mitid"

$providers = @"
{
  "mitId": "$providerId"
}
"@

return $providers
