// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Core.TestCommon.Authorization;

public class AuthorizationConfiguration
{
    public string Environment { get; }

    public string FrontendOpenIdUrl { get; }

    public string FrontendAppId { get; }

    public IConfigurationRoot RootConfiguration { get; }

    public IConfigurationRoot SecretsConfiguration { get; }

    public AuthorizationConfiguration(
        string environment,
        string localSettingsJsonFilename,
        string azureSecretsKeyVaultUrlKey)
    {
        Environment = environment;
        RootConfiguration = BuildKeyVaultConfigurationRoot(localSettingsJsonFilename);
        SecretsConfiguration = BuildSecretsKeyVaultConfiguration(RootConfiguration.GetValue<string>(azureSecretsKeyVaultUrlKey));
        FrontendAppId = SecretsConfiguration.GetValue<string>(BuildB2CFrontendAppId(Environment));
        FrontendOpenIdUrl = SecretsConfiguration.GetValue<string>(BuildB2CFrontendOpenIdUrl(Environment));
    }

    private static string BuildB2CFrontendAppId(string environment)
    {
        return $"B2C-{environment}-frontend-app-id";
    }

    private static string BuildB2CFrontendOpenIdUrl(string environment)
    {
        return $"B2C-{environment}-frontend-open-id-url";
    }

    /// <summary>
    /// Load settings from key vault.
    /// </summary>
    private static IConfigurationRoot BuildSecretsKeyVaultConfiguration(string keyVaultUrl)
    {
        return new ConfigurationBuilder()
            .AddAuthenticatedAzureKeyVault(keyVaultUrl)
            .Build();
    }

    private static IConfigurationRoot BuildKeyVaultConfigurationRoot(string localSettingsJsonFilename)
    {
        return new ConfigurationBuilder()
            .AddJsonFile(localSettingsJsonFilename, optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
