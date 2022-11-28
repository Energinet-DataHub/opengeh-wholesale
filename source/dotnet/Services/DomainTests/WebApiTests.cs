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

        // This is just to be able to verify everything works with regards to settings and executing the tests after deployment.
        // If needed, this test can be removed when the actual domain test has been implemented.
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
    }
}
