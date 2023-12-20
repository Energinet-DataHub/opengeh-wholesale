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
        /// Constructor to be used for execution on the old Azure subscriptions (environments).
        /// Ensure secrets are retrieved and ready for use.
        /// </summary>
        private B2CUserTokenConfiguration(string b2cKeyVaultUrl, string environment, string user, string tokenBaseAddress)
        {
            if (string.IsNullOrWhiteSpace(b2cKeyVaultUrl))
                throw new ArgumentException($"Cannot be null or whitespace.", nameof(b2cKeyVaultUrl));
            if (string.IsNullOrEmpty(environment))
                throw new ArgumentException($"Cannot be null or empty.", nameof(environment));
            if (string.IsNullOrEmpty(user))
                throw new ArgumentException($"Cannot be null or empty.", nameof(user));
            if (string.IsNullOrEmpty(tokenBaseAddress))
                throw new ArgumentException($"Cannot be null or empty.", nameof(tokenBaseAddress));

            var secretsConfiguration = BuildSecretsConfiguration(b2cKeyVaultUrl);

            TokenBaseAddress = tokenBaseAddress;

            RopcUrl = secretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(environment, "ropc-auth-url"))!;
            FrontendAppId = secretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(environment, "frontend-app-id"))!;
            BackendBffScope = secretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(environment, "backend-bff-app-scope"))!;

            Username = secretsConfiguration.GetValue<string>(BuildB2CUserSecretName(environment, user, "username"))!;
            Password = secretsConfiguration.GetValue<string>(BuildB2CUserSecretName(environment, user, "password"))!;
        }

        /// <summary>
        /// Constructor to be used for execution on the new Azure subscriptions (environments).
        /// Ensure secrets are retrieved and ready for use.
        /// </summary>
        private B2CUserTokenConfiguration(
            string tokenBaseAddress,
            string username,
            string password,
            string ropcAuthUrl,
            string frontendAppId,
            string backendBffAppScope)
        {
            if (string.IsNullOrEmpty(tokenBaseAddress))
                throw new ArgumentException($"Cannot be null or empty.", nameof(tokenBaseAddress));
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException($"Cannot be null or empty.", nameof(username));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException($"Cannot be null or empty.", nameof(password));
            if (string.IsNullOrEmpty(ropcAuthUrl))
                throw new ArgumentException($"Cannot be null or empty.", nameof(ropcAuthUrl));
            if (string.IsNullOrEmpty(frontendAppId))
                throw new ArgumentException($"Cannot be null or empty.", nameof(frontendAppId));
            if (string.IsNullOrEmpty(backendBffAppScope))
                throw new ArgumentException($"Cannot be null or empty.", nameof(backendBffAppScope));

            TokenBaseAddress = tokenBaseAddress;

            RopcUrl = ropcAuthUrl;
            FrontendAppId = frontendAppId;
            BackendBffScope = backendBffAppScope;

            Username = username;
            Password = password;
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
            var tokenBaseAddress = root.GetValue<string>("TOKEN_BASEADDRESS")!;

            var user = root.GetValue<string>("USER");

            // If 'user' is not set we are executing on the new subscriptions
            if (user == null)
            {
                // On the new subsctiptions we don't get the values from a key vault, but instead as environment variables
                var username = root.GetValue<string>("DH_E2E_USERNAME")!;
                var password = root.GetValue<string>("DH_E2E_PASSWORD")!;

                var ropcAuthUrl = root.GetValue<string>("DH_E2E_USERNAME")!;
                var frontendAppId = root.GetValue<string>("DH_E2E_PASSWORD")!;
                var backendBffAppScope = root.GetValue<string>("DH_E2E_USERNAME")!;
                return new B2CUserTokenConfiguration(tokenBaseAddress, username, password, ropcAuthUrl, frontendAppId, backendBffAppScope);
            }
            else
            {
                var b2cKeyVaultUrl = root.GetValue<string>("AZURE_B2CSECRETS_KEYVAULT_URL")!;

                var environment =
                    root.GetValue<string>("ENVIRONMENT_SHORT")! +
                    root.GetValue<string>("ENVIRONMENT_INSTANCE");

                return new B2CUserTokenConfiguration(b2cKeyVaultUrl, environment, user, tokenBaseAddress);
            }
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
