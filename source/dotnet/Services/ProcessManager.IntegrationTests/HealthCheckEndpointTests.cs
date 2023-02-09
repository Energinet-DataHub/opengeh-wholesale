﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixtures.TestHelpers;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture;
using Energinet.DataHub.Wholesale.IntegrationTests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProcessManager.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.ProcessManager;

[Collection(nameof(ProcessManagerFunctionAppCollectionFixture))]
public class HealthCheckEndpointTests : FunctionAppTestBase<ProcessManagerFunctionAppFixture>
{
    public HealthCheckEndpointTests(ProcessManagerFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
    }

    [Fact(Skip = "Test")]
    public async Task When_RequestLivenessStatus_Then_ResponseIsOkAndHealthy()
    {
        // Arrange
        var requestMessage = HttpRequestGenerator.CreateHttpGetRequest($"api{HealthChecksConstants.LiveHealthCheckEndpointRoute}");

        // Act
        var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(requestMessage);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var actualContent = await actualResponse.Content.ReadAsStringAsync();
        actualContent.Should().Be(Enum.GetName(typeof(HealthStatus), HealthStatus.Healthy));
    }

    [Fact(Skip = "Test")]
    public async Task When_RequestReadinessStatus_Then_ResponseIsOkAndHealthy()
    {
        // Arrange
        var requestMessage = HttpRequestGenerator.CreateHttpGetRequest($"api{HealthChecksConstants.ReadyHealthCheckEndpointRoute}");

        // Act
        var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(requestMessage);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var actualContent = await actualResponse.Content.ReadAsStringAsync();
        actualContent.Should().Be(Enum.GetName(typeof(HealthStatus), HealthStatus.Healthy));
    }
}
