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

using System.Net.Http.Headers;

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures
{
    /// <summary>
    /// Support testing Wholesale Web API and reusing configuration and
    /// instances between tests.
    /// </summary>
    public sealed class WebApiFixture : IDisposable
    {
        public WebApiFixture()
        {
            Configuration = new WholesaleDomainConfiguration();
            UserAuthenticationClient = new B2CUserTokenAuthenticationClient(Configuration.UserTokenConfiguration);
        }

        public WholesaleDomainConfiguration Configuration { get; }

        private B2CUserTokenAuthenticationClient UserAuthenticationClient { get; }

        public void Dispose()
        {
            UserAuthenticationClient.Dispose();
        }

        /// <summary>
        /// Create a http client for calling the Wholesale Web API.
        /// If <paramref name="aquireAccessToken"/> is 'true' it will be prepared with an access token.
        /// </summary>
        public async Task<HttpClient> CreateHttpClientAsync(bool aquireAccessToken = false)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = Configuration.WebApiBaseAddress,
            };

            if (aquireAccessToken)
            {
                var accessToken = await UserAuthenticationClient.AcquireAccessTokenAsync();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return httpClient;
        }
    }
}
