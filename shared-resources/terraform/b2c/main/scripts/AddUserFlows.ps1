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
  Creates user flows in the specified tenant.

  .DESCRIPTION
  Creates and configures invitation user flow and sign in user flow using Graph API in the specified tenant.
  The user flows are not recreated if they already exist.

  .EXAMPLE
  PS> ./Add-UserFlows.ps1 <TenantId> <ClientId> <ClientSecret>
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
    $B2CClientSecret
)

[string]$accessToken = Get-AccessToken -B2CTenantId $B2CTenantId -B2CClientId $B2CClientId -B2CClientSecret $B2CClientSecret

Write-Information "Creating user flow for inviting users"
New-UserFlow -AccessToken $AccessToken -UserFlowId "InvitationFlow" -UserFlowType "passwordReset"

Write-Information "Creating user flow for signing users in using TOTP"
New-UserFlow -AccessToken $AccessToken -UserFlowId "SignInFlow" -UserFlowType "signIn"

$user_flows = @"
{
  "inviteUserFlowId": "B2C_1_InvitationFlow",
  "signInUserFlowId": "B2C_1_SignInFlow"
}
"@

return $user_flows
