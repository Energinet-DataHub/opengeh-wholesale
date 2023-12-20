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
    /// On the new subsctiptions we don't get the values from a key vault, but instead as environment variables.
    /// </summary>
    public class B2CUserTokenConfiguration
    {
        /// <summary>
        /// Constructor to be used for execution on the new Azure subscriptions (environments).
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

            Username = username;
            Password = password;

            RopcUrl = ropcAuthUrl;
            FrontendAppId = frontendAppId;
            BackendBffScope = backendBffAppScope;
        }

        /// <summary>
        /// The base address for the endpoint that augments the external token with permissions.
        /// </summary>
        public string TokenBaseAddress { get; }

        /// <summary>
        /// The username of the user for whom we want to retrieve an access token.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// The password of the user for whom we want to retrieve an access token.
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// The URL for the configured Resource Owner Password Credentials (ROPC) flow in the B2C.
        /// This is the URL used to retrieve the user token from B2C.
        /// </summary>
        public string RopcUrl { get; }

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
        /// Encapsulates creation using a configuration root.
        /// </summary>
        public static B2CUserTokenConfiguration CreateFromConfiguration(IConfigurationRoot root)
        {
            var tokenBaseAddress = root.GetValue<string>("TOKEN_BASEADDRESS")!;

            var username = root.GetValue<string>("DH_E2E_USERNAME")!;
            var password = root.GetValue<string>("DH_E2E_PASSWORD")!;

            var ropcAuthUrl = root.GetValue<string>("B2C_ROPC_AUTH_URL")!;
            var frontendAppId = root.GetValue<string>("B2C_FRONTEND_APP_ID")!;
            var backendBffAppScope = root.GetValue<string>("B2C_BACKEND_BFF_APP_SCOPE")!;

            return new B2CUserTokenConfiguration(tokenBaseAddress, username, password, ropcAuthUrl, frontendAppId, backendBffAppScope);
        }
    }
}
