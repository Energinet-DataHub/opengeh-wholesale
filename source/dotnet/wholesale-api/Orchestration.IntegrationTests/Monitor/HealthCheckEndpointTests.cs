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
using Energinet.DataHub.Wholesale.Orchestration.IntegrationTests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.Orchestration.IntegrationTests.Monitor
{
    /// <summary>
    /// Tests verifying the configuration and behaviour of Health Checks.
    /// </summary>
    [Collection(nameof(OrchestrationAppCollectionFixture))]
    public class HealthCheckEndpointTests : IAsyncLifetime
    {
        public HealthCheckEndpointTests(OrchestrationAppFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.SetTestOutputHelper(testOutputHelper);

            Fixture.AppHostManager.ClearHostLog();
        }

        private OrchestrationAppFixture Fixture { get; }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            Fixture.SetTestOutputHelper(null!);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Verify the response contains JSON in a format that the Health Checks UI supports.
        /// </summary>
        [Theory]
        [InlineData("live")]
        [InlineData("ready")]
        public async Task FunctionApp_WhenCallingHealthCheck_ReturnOKAndExpectedContent(string healthCheckEndpoint)
        {
            // Act
            using var actualResponse = await Fixture.AppHostManager.HttpClient.GetAsync($"api/monitor/{healthCheckEndpoint}");

            // Assert
            using var assertionScope = new AssertionScope();

            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            actualResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

            var content = await actualResponse.Content.ReadAsStringAsync();
            content.Should().StartWith("{\"status\":\"Healthy\"");
        }
    }
}
