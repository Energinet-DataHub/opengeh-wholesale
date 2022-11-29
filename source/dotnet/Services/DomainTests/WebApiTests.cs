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

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures;
using FluentAssertions;
using Microsoft.Identity.Client;

namespace Energinet.DataHub.Wholesale.DomainTests
{
    /// <summary>
    /// Contains tests where we operate at the level of a "domain", so basically what in some context has been named "domain tests".
    /// However, with the technique displayed here we perform these tests in a live environment.
    /// </summary>
    public class WebApiTests
    {
        public WebApiTests()
        {
            Configuration = new WholesaleDomainConfiguration();
        }

        private WholesaleDomainConfiguration Configuration { get; }

        /// <summary>
        /// This is just to be able to verify everything works with regards to settings and executing the tests after deployment.
        /// If needed, this test can be removed when the actual domain test has been implemented.
        /// </summary>
        [DomainFact]
        public async Task When_RequestReadinessStatus_Then_ResponseIsOkAndHealthy()
        {
            // Arrange
            using var httpClient = new HttpClient
            {
                BaseAddress = Configuration.WebApiBaseAddress,
            };

            // Act
            using var actualResponse = await httpClient.GetAsync("monitor/ready");

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await actualResponse.Content.ReadAsStringAsync();
            content.Should().Contain("Healthy");
        }

        /// <summary>
        /// This shows our request will fail if we call Web API without a valid access token
        /// </summary>
        [DomainFact]
        public async Task When_RequestWithoutAccessToken_Then_ResponseIsUnauthorized()
        {
            // Arrange
            using var httpClient = await CreateHttpClientAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, "v2/batch?batchId=1");

            // Act
            using var actualResponse = await httpClient.SendAsync(request);

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [DomainFact]
        public async Task When_RequestingExistingBatchIdWithAccessToken_Then_ResponseIsOk()
        {
            // Arrange
            using var httpClient = await CreateHttpClientAsync(retrieveAccessToken: true);
            var existingBatchIdInU001 = "a68d4452-943b-4f45-b32f-5f84607c6b6b";
            var request = new HttpRequestMessage(HttpMethod.Get, $"v2/batch?batchId={existingBatchIdInU001}");

            // Act
            using var actualResponse = await httpClient.SendAsync(request);

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Create a http client with an access token if <paramref name="retrieveAccessToken"/> is 'true'.
        /// </summary>
        private async Task<HttpClient> CreateHttpClientAsync(bool retrieveAccessToken = false)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = Configuration.WebApiBaseAddress,
            };

            if (retrieveAccessToken)
            {
                var accessToken = await GetUserAccessTokenAsync();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return httpClient;
        }

        /// <summary>
        /// See also "Test the ROPC flow": https://learn.microsoft.com/en-us/azure/active-directory-b2c/add-ropc-policy?#test-the-ropc-flow
        /// </summary>
        private async Task<string> GetUserAccessTokenAsync()
        {
            var form = new MultipartFormDataContent
            {
                { new StringContent(Configuration.UserTokenConfiguration.Username), "username" },
                { new StringContent(Configuration.UserTokenConfiguration.Password), "password" },
                { new StringContent("password"), "grant_type" },
                { new StringContent($"openid {Configuration.UserTokenConfiguration.FrontendAppId} offline_access"), "scope" },
                { new StringContent(Configuration.UserTokenConfiguration.FrontendAppId), "client_id" },
                { new StringContent("token id_token"), "response_type" },
            };

            using var httpClient = new HttpClient();

            var httpResponse = await httpClient.PostAsync(Configuration.UserTokenConfiguration.RopcUrl, form);
            httpResponse.EnsureSuccessStatusCode();

            var tokenResult = await httpResponse.Content.ReadFromJsonAsync<AccessTokenResult>();
            return tokenResult!.AccessToken;
        }
    }
}
