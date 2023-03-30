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
    Creates and configures a user flow with the given id and type.
    If the user flow already exists, its user flow type must match the specified parameter or this function will throw.

    .DESCRIPTION
    Creates and configures a user flow with the given id and type. The id will be prefixed with B2C_1_ by Azure.
    If the user flow already exists, its user flow type must match the specified parameter or this function will throw.
    The user flow is created with 'da' and 'en' languages, where 'da' is set as default.
#>
function New-UserFlow {
    param (
        [Parameter(Mandatory)]
        [string]
        $AccessToken,
        [Parameter(Mandatory)]
        [string]
        $UserFlowId,
        [Parameter(Mandatory)]
        [string]
        $UserFlowType
    )

    Write-Information "Trying to create '$UserFlowId' user flow"

    $headers = @{
        Authorization = "Bearer $AccessToken"
    }

    $body = @{
        id                             = $UserFlowId
        userFlowType                   = $UserFlowType
        userFlowTypeVersion            = 3
        isLanguageCustomizationEnabled = "true"
        defaultLanguageTag             = "da"
    }

    try {
        Invoke-RestMethod `
            -Uri "https://graph.microsoft.com/beta/identity/b2cUserFlows" -Method Post `
            -Headers $headers `
            -ContentType "application/json" `
            -Body ($body | ConvertTo-Json) | Out-Null

        Write-Information "Created user flow '$UserFlowId' successfully"

    } catch [System.Net.WebException] {
        Write-Warning "User flow '$UserFlowId' was not created (maybe it already exists)"
        Invoke-AssertUserFlow -AccessToken $AccessToken -ExpectedUserFlowId $UserFlowId -ExpectedUserFlowType $UserFlowType
    }

    Invoke-AddUserFlowLanguage -AccessToken $AccessToken -UserFlowId $UserFlowId -Language "en"
}

<#
    .SYNOPSIS
    Adds and enables the specified language to the given user flow.
#>
function Invoke-AddUserFlowLanguage {
    param (
        [Parameter(Mandatory)]
        [string]
        $AccessToken,
        [Parameter(Mandatory)]
        [string]
        $UserFlowId,
        [Parameter(Mandatory)]
        [string]
        $Language
    )

    Write-Information "Adding language '$Language' to '$UserFlowId' user flow"

    $headers = @{
        Authorization = "Bearer $AccessToken"
    }

    $body = @{
        id        = $Language
        isEnabled = "true"
    }

    Invoke-RestMethod `
        -Uri "https://graph.microsoft.com/beta/identity/b2cUserFlows/B2C_1_$UserFlowId/languages/$Language" -Method Put `
        -Headers $headers `
        -ContentType "application/json" `
        -Body ($body | ConvertTo-Json) | Out-Null
}

<#
    .SYNOPSIS
    Checks that the specified user flow exists and has the correct type; throws otherwise.
#>
function Invoke-AssertUserFlow {
    param (
        [Parameter(Mandatory)]
        [string]
        $AccessToken,
        [Parameter(Mandatory)]
        [string]
        $ExpectedUserFlowId,
        [Parameter(Mandatory)]
        [string]
        $ExpectedUserFlowType
    )

    Write-Information "Asserting if '$ExpectedUserFlowId' user flow exists already"

    $headers = @{
        Authorization = "Bearer $AccessToken"
    }

    try {
        $existingUserFlow = Invoke-RestMethod `
            -Uri "https://graph.microsoft.com/beta/identity/b2cUserFlows/B2C_1_$ExpectedUserFlowId" -Method Get `
            -Headers $headers

        if ($existingUserFlow.userFlowType -eq $ExpectedUserFlowType) {
            Write-Information "Assertion of '$ExpectedUserFlowId' succeeded"
            return
        }

    } catch [System.Net.WebException] {
        Write-Error "Could not assert '$ExpectedUserFlowId' user flow: $_"
    }

    throw "Assertion of '$ExpectedUserFlowId' failed"
}
