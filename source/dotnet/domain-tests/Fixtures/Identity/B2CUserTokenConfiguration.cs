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

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures.Identity
{
    /// <summary>
    /// Responsible for extracting secrets for authorization needed for performing tests using B2C 'user' access tokens.
    /// This means access tokens similar to what is used if logging into the UI.
    ///
    /// Developers, and the service principal under which the tests are executed, must have access
    /// to the Key Vault so secrets can be extracted.
    /// </summary>
    public class B2CUserTokenConfiguration
    {
        /// <summary>
        /// Ensure secrets are retrieved and ready for use.
        /// </summary>
        public B2CUserTokenConfiguration(string b2cKeyVaultUrl, string environment, string user, string tokenBaseAddress)
        {
            if (string.IsNullOrWhiteSpace(b2cKeyVaultUrl))
                throw new ArgumentException($"'{nameof(b2cKeyVaultUrl)}' cannot be null or whitespace.", nameof(b2cKeyVaultUrl));
            if (string.IsNullOrEmpty(environment))
                throw new ArgumentException($"'{nameof(environment)}' cannot be null or empty.", nameof(environment));
            if (string.IsNullOrEmpty(user))
                throw new ArgumentException($"'{nameof(user)}' cannot be null or empty.", nameof(user));
            if (string.IsNullOrEmpty(tokenBaseAddress))
                throw new ArgumentException($"'{nameof(tokenBaseAddress)}' cannot be null or empty.", nameof(tokenBaseAddress));

            var secretsConfiguration = BuildSecretsConfiguration(b2cKeyVaultUrl);

            TokenBaseAddress = tokenBaseAddress;

            RopcUrl = secretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(environment, "ropc-auth-url"))!;
            FrontendAppId = secretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(environment, "frontend-app-id"))!;
            BackendBffScope = secretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(environment, "backend-bff-app-scope"))!;

            Username = secretsConfiguration.GetValue<string>(BuildB2CUserSecretName(environment, user, "username"))!;
            Password = secretsConfiguration.GetValue<string>(BuildB2CUserSecretName(environment, user, "password"))!;
        }

        /// <summary>
        /// The URL for the configured Resource Owner Password Credentials (ROPC) flow in the B2C.
        /// This is the URL used to retrieve the user token from B2C.
        /// </summary>
        public string RopcUrl { get; }

        /// <summary>
        /// The base address for the endpoint that augments the external token with permissions.
        /// </summary>
        public string TokenBaseAddress { get; }

        /// <summary>
        /// The frontend application id.
        /// This is the app for which we want to retrieve the user token.
        /// </summary>
        public string FrontendAppId { get; }

        /// <summary>
        /// The scope that needs to be granted access to gain access to backend.
        /// </summary>
        public string BackendBffScope { get; }

        /// <summary>
        /// The username of the user for whom we want to retrieve an access token.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// The password of the user for whom we want to retrieve an access token.
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Encapsulates creation using a configuration root.
        /// </summary>
        public static B2CUserTokenConfiguration CreateFromConfiguration(IConfigurationRoot root)
        {
            var b2cKeyVaultUrl = root.GetValue<string>("AZURE_B2CSECRETS_KEYVAULT_URL")!;
            var environment =
                root.GetValue<string>("ENVIRONMENT_SHORT")! +
                root.GetValue<string>("ENVIRONMENT_INSTANCE");
            var user =
                root.GetValue<string>("USER")!;
            var tokenBaseAddress =
                root.GetValue<string>("TOKEN_BASEADDRESS")!;

            return new B2CUserTokenConfiguration(b2cKeyVaultUrl, environment, user, tokenBaseAddress);
        }

        private static string BuildB2CEnvironmentSecretName(string environment, string secret)
        {
            return $"B2C-{environment}-{secret}";
        }

        private static string BuildB2CUserSecretName(string environment, string user, string secret)
        {
            return $"B2C-{environment}-{user}-{secret}";
        }

        /// <summary>
        /// Load settings from key vault secrets.
        /// </summary>
        private static IConfigurationRoot BuildSecretsConfiguration(string keyVaultUrl)
        {
            return new ConfigurationBuilder()
                .AddAuthenticatedAzureKeyVault(keyVaultUrl)
                .Build();
        }
    }
}
