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


InModuleScope 'ManageUserFlow' {
    Describe "New-OpenIdProvider" {
        BeforeAll {
            function Invoke-GetTestParameters() {
                return @{
                    AccessToken                             = "mocked_value"
                    Identifier                              = "TestID"
                    OpenIdConfigurationUrl                  = "https://localhost/.well-known/openid-configuration"
                    OpenIdConfigurationClientId             = "841a328b-8894-4bc9-9106-a0ebebe31fb6"
                    OpenIdConfigurationClientSecret         = "most_secret"
                    OpenIdConfigurationUserIdClaimName      = "sub"
                    OpenIdConfigurationDisplayNameClaimName = "name"
                    OpenIdConfigurationResponseMode         = "form_post"
                    OpenIdConfigurationResponseType         = "code"
                    OpenIdConfigurationScopes               = "openid"
                }
            }
        }

        Context "Given that the identity provider does not exist" {
            It "Should be created" {
                Mock Invoke-RestMethod { return @{ value = @() } } -ParameterFilter { $uri -eq "https://graph.microsoft.com/beta/identity/identityProviders" -and $method -eq "Get" }
                Mock Invoke-RestMethod { }

                $inputParameters = Invoke-GetTestParameters

                # Act
                New-OpenIdProvider @inputParameters

                Should -Invoke -CommandName Invoke-RestMethod -Times 1 -ParameterFilter {
                    $uri -eq "https://graph.microsoft.com/beta/identity/identityProviders" -and $method -eq "Post"
                }
            }

            It "Should return the new id" {
                $expectedId = "06C3DFC1-67AD-4EC8-A6F4-70049C959E11"

                $response = @{
                    id = $expectedId
                }

                Mock Invoke-RestMethod { return @{ value = @() } } -ParameterFilter { $uri -eq "https://graph.microsoft.com/beta/identity/identityProviders" -and $method -eq "Get" }
                Mock Invoke-RestMethod { return $response } -ParameterFilter { $uri -eq "https://graph.microsoft.com/beta/identity/identityProviders" -and $method -eq "Post" }
                Mock Invoke-RestMethod { }

                $inputParameters = Invoke-GetTestParameters

                # Act
                $actual = New-OpenIdProvider @inputParameters
                $actual | Should -Be $expectedId
            }
        }

        Context "Given that the identity provider already exist" {
            It "Should update the identity provider" {
                $expectedId = "06C3DFC1-67AD-4EC8-A6F4-70049C959E11"
                $inputParameters = Invoke-GetTestParameters

                $existingProviders = @{
                    value = @(@{
                            "@odata.type" = "#microsoft.graph.openIdConnectIdentityProvider"
                            id            = $expectedId
                            displayName   = $inputParameters.Identifier
                            metadataUrl   = $inputParameters.OpenIdConfigurationUrl
                        })
                }

                Mock Invoke-RestMethod { return $existingProviders } -ParameterFilter { $uri -eq "https://graph.microsoft.com/beta/identity/identityProviders" -and $method -eq "Get" }
                Mock Invoke-RestMethod { }

                # Act
                New-OpenIdProvider @inputParameters

                Should -Invoke -CommandName Invoke-RestMethod -Times 1 -ParameterFilter {
                    $uri -eq "https://graph.microsoft.com/beta/identity/identityProviders/$expectedId" -and $method -eq "Patch"
                }
            }

            It "Should return existing id" {
                $expectedId = "06C3DFC1-67AD-4EC8-A6F4-70049C959E11"
                $inputParameters = Invoke-GetTestParameters

                $existingProviders = @{
                    value = @(@{
                            "@odata.type" = "#microsoft.graph.openIdConnectIdentityProvider"
                            id            = $expectedId
                            displayName   = $inputParameters.Identifier
                            metadataUrl   = $inputParameters.OpenIdConfigurationUrl
                        })
                }

                Mock Invoke-RestMethod { return $existingProviders } -ParameterFilter { $uri -eq "https://graph.microsoft.com/beta/identity/identityProviders" -and $method -eq "Get" }
                Mock Invoke-RestMethod { }

                # Act
                $response = New-OpenIdProvider @inputParameters
                $response | Should -Be $expectedId
            }

            It "Should throw if metadata url is different" {
                $expectedId = "06C3DFC1-67AD-4EC8-A6F4-70049C959E11"
                $inputParameters = Invoke-GetTestParameters

                $existingProviders = @{
                    value = @(@{
                            "@odata.type" = "#microsoft.graph.openIdConnectIdentityProvider"
                            id            = $expectedId
                            displayName   = $inputParameters.Identifier
                            metadataUrl   = "https://wrong.dk/.well-known/configuration"
                        })
                }

                Mock Invoke-RestMethod { return $existingProviders } -ParameterFilter { $uri -eq "https://graph.microsoft.com/beta/identity/identityProviders" -and $method -eq "Get" }
                Mock Invoke-RestMethod { }

                # Act
                { New-OpenIdProvider @inputParameters } | Should -Throw
            }
        }
    }
}
