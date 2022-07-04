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
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixture.WebApi;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.WebApi;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.WebApi
{
    [Collection(nameof(WholesaleWebApiCollectionFixture))]
    public class HealthCheckTests :
        WebApiTestBase<WholesaleWebApiFixture>,
        IClassFixture<WholesaleWebApiFixture>,
        IClassFixture<WebApiFactory>,
        IAsyncLifetime
    {
        private readonly HttpClient _client;

        public HealthCheckTests(
            WholesaleWebApiFixture wholesaleWebApiFixture,
            WebApiFactory factory,
            ITestOutputHelper testOutputHelper)
            : base(wholesaleWebApiFixture, testOutputHelper)
        {
            _client = factory.CreateClient();
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _client.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task When_RequestLivenessStatus_Then_ResponseIsOkAndHealthy()
        {
            // Arrange + Act
            var actualResponse = await _client.GetAsync(HealthChecksConstants.LiveHealthCheckEndpointRoute);

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var actualContent = await actualResponse.Content.ReadAsStringAsync();
            actualContent.Should().Be(Enum.GetName(typeof(HealthStatus), HealthStatus.Healthy));
        }

        [Fact]
        public async Task When_RequestReadinessStatus_Then_ResponseIsOkAndHealthy()
        {
            // Arrange + Act
            var actualResponse = await _client.GetAsync(HealthChecksConstants.ReadyHealthCheckEndpointRoute);

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var actualContent = await actualResponse.Content.ReadAsStringAsync();
            actualContent.Should().Be(Enum.GetName(typeof(HealthStatus), HealthStatus.Healthy));
        }
    }
}
