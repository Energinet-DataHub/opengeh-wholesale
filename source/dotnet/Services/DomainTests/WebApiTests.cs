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
using Energinet.DataHub.Wholesale.DomainTests.Fixtures;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.DomainTests
{
    /// <summary>
    /// Contains tests where we operate at the level of a "domain", so basically what in some context has been named "domain tests".
    /// However, with the technique displayed here we perform these tests in a live environment.
    /// </summary>
    public class WebApiTests : IClassFixture<WebApiFixture>
    {
        public WebApiTests(WebApiFixture fixture)
        {
            Fixture = fixture;
        }

        private WebApiFixture Fixture { get; }

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
                BaseAddress = Fixture.Configuration.WebApiBaseAddress,
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
            using var httpClient = new HttpClient
            {
                BaseAddress = Fixture.Configuration.WebApiBaseAddress,
            };
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

            // Act
            var batchResult = await Fixture.WholesaleClient.GetBatchAsync(Fixture.Configuration.ExistingBatchId);

            // Assert
            batchResult.Should().NotBeNull();
            batchResult!.BatchNumber.Should().Be(Fixture.Configuration.ExistingBatchId);
        }
    }
}
