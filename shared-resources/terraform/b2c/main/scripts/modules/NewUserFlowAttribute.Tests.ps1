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

Describe "New-UserFlowAttribute" {
    Context "Given that the attribute does not exist" {
        It "Should be created" {
            InModuleScope 'ManageUserFlow' {
                Mock Invoke-RestMethod { }

                $accessToken = "mocked_value"
                $userFlowId = "SignInFlow"
                $attributeId = "email"
                $attributeType = "emailBox"

                # Act
                New-UserFlowAttribute -AccessToken $accessToken -UserFlowId $userFlowId -AttributeId $attributeId -AttributeType $attributeType

                Should -Invoke -CommandName Invoke-RestMethod -Times 1 -ParameterFilter {
                    $uri -eq "https://graph.microsoft.com/beta/identity/b2cUserFlows/B2C_1_$userFlowId/userAttributeAssignments" -and $method -eq "Post"
                }
            }
        }
    }

    Context "Given that attribute already exist" {
        It "Should not throw if valid" {
            InModuleScope 'ManageUserFlow' {
                $accessToken = "mocked_value"
                $userFlowId = "SignInFlow"
                $attributeId = "email"
                $attributeType = "emailBox"

                Mock Invoke-RestMethod { throw [System.Net.WebException] "Exists" } -ParameterFilter { $uri -eq "https://graph.microsoft.com/beta/identity/b2cUserFlows/B2C_1_$userFlowId/userAttributeAssignments" -and $method -eq "Post" }
                Mock Invoke-RestMethod { }

                $mockedResponse = @{
                    value = @(
                        @{
                            id = $attributeId
                        }
                    )
                }

                Mock Invoke-RestMethod { $mockedResponse } -ParameterFilter { $uri -eq "https://graph.microsoft.com/beta/identity/b2cUserFlows/B2C_1_$userFlowId/userAttributeAssignments" -and $method -eq "Get" }

                # Act
                New-UserFlowAttribute -AccessToken $accessToken -UserFlowId $userFlowId -AttributeId $attributeId -AttributeType $attributeType
            }
        }

        It "Should throw if not valid" {
            InModuleScope 'ManageUserFlow' {
                $accessToken = "mocked_value"
                $userFlowId = "SignInFlow"
                $attributeId = "email"
                $attributeType = "emailBox"

                Mock Invoke-RestMethod { throw [System.Net.WebException] "Exists" } -ParameterFilter { $uri -eq "https://graph.microsoft.com/beta/identity/b2cUserFlows/B2C_1_$userFlowId/userAttributeAssignments" -and $method -eq "Post" }
                Mock Invoke-RestMethod { }

                $mockedResponse = @{
                    value = @(
                        @{
                            id = "incorrect_value"
                        }
                    )
                }

                Mock Invoke-RestMethod { $mockedResponse } -ParameterFilter { $uri -eq "https://graph.microsoft.com/beta/identity/b2cUserFlows/B2C_1_$userFlowId/userAttributeAssignments" -and $method -eq "Get" }

                # Act
                { New-UserFlowAttribute -AccessToken $accessToken -UserFlowId $userFlowId -AttributeId $attributeId -AttributeType $attributeType } | Should -Throw
            }
        }
    }
}
