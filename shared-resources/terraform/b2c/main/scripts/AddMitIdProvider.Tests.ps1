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

Describe "AddMitIdProvider" {
    Context "Given that MitID provider is created" {
        It "Should return JSON with MitID provider id" {
            InModuleScope 'ManageUserFlow' {
                $expectedId = "39545C4E-AFB0-4124-9B4B-AC5674061888"

                Mock Get-AccessToken { "mocked_value" }
                Mock New-OpenIdProvider { return $expectedId }

                $b2CTenantId = "mocked_value"
                $b2CClientId = "mocked_value"
                $b2CClientSecret = "mocked_value"
                $mitIdClientId = "mocked_value"
                $mitIdClientSecret = "mocked_value"

                # Act
                $response = ./AddMitIdProvider.ps1 $b2CTenantId $b2CClientId $b2CClientSecret $mitIdClientId $mitIdClientSecret

                $actual = $response | ConvertFrom-Json
                $actual.mitid | Should -Be $expectedId
            }
        }

        It "Should create them correctly" {
            InModuleScope 'ManageUserFlow' {
                Mock Get-AccessToken { "mocked_value" }
                Mock New-OpenIdProvider {}

                $b2CTenantId = "mocked_value"
                $b2CClientId = "mocked_value"
                $b2CClientSecret = "mocked_value"
                $mitIdClientId = "mocked_value_id"
                $mitIdClientSecret = "mocked_value_secret"

                # Act
                ./AddMitIdProvider.ps1 $b2CTenantId $b2CClientId $b2CClientSecret $mitIdClientId $mitIdClientSecret

                Should -Invoke -CommandName New-OpenIdProvider -Times 1 -ParameterFilter {
                    $openIdConfigurationClientId -eq $mitIdClientId -and $openIdConfigurationClientSecret -eq $mitIdClientSecret
                }
            }
        }
    }
}
