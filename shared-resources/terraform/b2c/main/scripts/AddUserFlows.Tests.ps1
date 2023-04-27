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

Describe "AddUserFlows" {
    Context "Given that user flows are created" {
        It "Should return JSON with user flow ids" {
            InModuleScope 'ManageUserFlow' {
                Mock Get-AccessToken { "mocked_value" }
                Mock New-UserFlow {}
                Mock New-UserFlowAttribute {}

                $b2CTenantId     = "mocked_value"
                $b2CClientId     = "mocked_value"
                $b2CClientSecret = "mocked_value"
                $mitIdProviderId = "mocked_value"

                # Act
                $response = ./AddUserFlows.ps1 $b2CTenantId $b2CClientId $b2CClientSecret $mitIdProviderId

                $actual = $response | ConvertFrom-Json
                $actual.inviteUserFlowId | Should -Be "B2C_1_InvitationFlow"
                $actual.signInUserFlowId | Should -Be "B2C_1_SignInFlow"
                $actual.mitIdInviteUserFlowId | Should -Be "B2C_1_MitID_InvitationFlow"
                $actual.mitIdSignInUserFlowId | Should -Be "B2C_1_MitID_SignInFlow"
            }
        }
    }

    Context "Given default user flows" {
        It "Should create them correctly" {
            InModuleScope 'ManageUserFlow' {
                Mock Get-AccessToken { "mocked_value" }
                Mock New-UserFlow {}
                Mock New-UserFlowAttribute {}

                $b2CTenantId     = "mocked_value"
                $b2CClientId     = "mocked_value"
                $b2CClientSecret = "mocked_value"
                $mitIdProviderId = "mocked_value"

                # Act
                ./AddUserFlows.ps1 $b2CTenantId $b2CClientId $b2CClientSecret $mitIdProviderId

                Should -Invoke -CommandName New-UserFlow -Times 1 -ParameterFilter {
                    $userFlowId -eq "InvitationFlow" -and $userFlowType -eq "passwordReset"
                }

                Should -Invoke -CommandName New-UserFlow -Times 1 -ParameterFilter {
                    $userFlowId -eq "SignInFlow" -and $userFlowType -eq "signIn"
                }

                Should -Invoke -CommandName New-UserFlow -Times 1 -ParameterFilter {
                    $userFlowId -eq "MitID_InvitationFlow" -and $userFlowType -eq "signUp"
                }

                Should -Invoke -CommandName New-UserFlow -Times 1 -ParameterFilter {
                    $userFlowId -eq "MitID_SignInFlow" -and $userFlowType -eq "signIn"
                }
            }
        }
    }
}
