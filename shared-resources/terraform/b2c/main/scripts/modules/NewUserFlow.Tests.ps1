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

using module "./ManageUserFlow.psd1"

Describe "New-UserFlow" {
    Context "Given that the user flow does not exist" {
        It "Should be created" {
            InModuleScope 'ManageUserFlow' {
                Mock Invoke-RestMethod { }

                $accessToken = "mocked_value"
                $userFlowId = "SignInFlow"
                $userFlowType = "signIn"

                # Act
                New-UserFlow -AccessToken $accessToken -UserFlowId $userFlowId -UserFlowType $userFlowType

                Should -Invoke -CommandName Invoke-RestMethod -Times 1 -ParameterFilter {
                    $uri -eq "https://graph.microsoft.com/beta/identity/b2cUserFlows" -and $method -eq "Post"
                }
            }
        }

        It "Should be assigned additional languages" {
            InModuleScope 'ManageUserFlow' {
                Mock Invoke-RestMethod { }

                $accessToken = "mocked_value"
                $userFlowId = "SignInFlow"
                $userFlowType = "signIn"

                $primaryLanguage = "da"
                $additionalLanguages = "en"

                # Act
                New-UserFlow -AccessToken $accessToken -UserFlowId $userFlowId -UserFlowType $userFlowType

                Should -Invoke -CommandName Invoke-RestMethod -Times 1 -ParameterFilter {
                    $uri -eq "https://graph.microsoft.com/beta/identity/b2cUserFlows" `
                        -and $method -eq "Post" `
                        -and ($body | ConvertFrom-Json).defaultLanguageTag -eq $primaryLanguage
                }

                foreach ($lang in $additionalLanguages) {
                    Should -Invoke -CommandName Invoke-RestMethod -Times 1 -ParameterFilter {
                        $uri -eq "https://graph.microsoft.com/beta/identity/b2cUserFlows/B2C_1_SignInFlow/languages/$lang" -and $method -eq "Put"
                    }
                }
            }
        }
    }

    Context "Given that user flow already exist" {
        It "Should not throw if valid" {
            InModuleScope 'ManageUserFlow' {
                Mock Invoke-RestMethod { throw [System.Net.WebException] "Exists" } -ParameterFilter { $uri -eq "https://graph.microsoft.com/beta/identity/b2cUserFlows" }
                Mock Invoke-RestMethod { }

                $accessToken = "mocked_value"
                $userFlowId = "SignInFlow"
                $userFlowType = "signIn"

                $mockedResponse = @{
                    userFlowType = $userFlowType
                }

                Mock Invoke-RestMethod { $mockedResponse } -ParameterFilter { $uri -eq "https://graph.microsoft.com/beta/identity/b2cUserFlows/B2C_1_$userFlowId" }

                # Act
                New-UserFlow -AccessToken $accessToken -UserFlowId $userFlowId -UserFlowType $userFlowType
            }
        }

        It "Should throw if not valid" {
            InModuleScope 'ManageUserFlow' {
                Mock Invoke-RestMethod { throw [System.Net.WebException] "Exists" } -ParameterFilter { $uri -eq "https://graph.microsoft.com/beta/identity/b2cUserFlows" }
                Mock Invoke-RestMethod { }

                $accessToken = "mocked_value"
                $userFlowId = "SignInFlow"
                $userFlowType = "signIn"

                $mockedResponse = @{
                    userFlowType = "incorrect_type"
                }

                Mock Invoke-RestMethod { $mockedResponse } -ParameterFilter { $uri -eq "https://graph.microsoft.com/beta/identity/b2cUserFlows/B2C_1_$userFlowId" }

                # Act
                { New-UserFlow -AccessToken $accessToken -UserFlowId $userFlowId -UserFlowType $userFlowType } | Should -Throw
            }
        }
    }
}
