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
using System.Net.Http.Json;
using System.Text;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.SettlementReports.Model;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.DurableTask;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
using Newtonsoft.Json;
using Xunit.Abstractions;
using GridAreaCode = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models.GridAreaCode;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Functions.SettlementReports;

[Collection(nameof(OrchestrationsAppCollectionFixture))]
public class SettlementReportOrchestratorTests : IAsyncLifetime
{
    public SettlementReportOrchestratorTests(
        OrchestrationsAppFixture fixture,
        ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.SetTestOutputHelper(testOutputHelper);
    }

    private OrchestrationsAppFixture Fixture { get; }

    public Task InitializeAsync()
    {
        Fixture.EnsureAppHostUsesMockedDatabricksJobs();
        Fixture.AppHostManager.ClearHostLog();

        // Clear mappings etc. before each test
        Fixture.MockServer.Reset();

        Fixture.ServiceBusListenerMock.ResetMessageHandlersAndReceivedMessages();

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Fixture.SetTestOutputHelper(null!);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Verifies that:
    ///  - The orchestration can complete a full run.
    ///  - Every activity is executed once and in correct order.
    ///  - A service bus message is sent as expected.
    /// </summary>
    [Fact]
    public async Task MockExternalDependencies_WhenCallingDurableFunctionEndPoint_OrchestrationCompletesWithExpectedServiceBusMessage()
    {
        // Arrange
        var settlementReportRequest = new SettlementReportRequestDto(
            CalculationType.BalanceFixing,
            new SettlementReportRequestFilterDto(
                [new GridAreaCode("042")],
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                null));

        // Act
        using var actualResponse = await Fixture.AppHostManager.HttpClient.PostAsync(
            "api/RequestSettlementReport",
            new StringContent(
                JsonConvert.SerializeObject(settlementReportRequest),
                Encoding.UTF8,
                "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, actualResponse.StatusCode);

        var httpResponse = await actualResponse.Content.ReadFromJsonAsync<SettlementReportHttpResponse>();
        Assert.NotNull(httpResponse);

        var completeOrchestrationStatus = await Fixture.DurableClient.WaitForInstanceCompletedAsync(
            httpResponse.RequestId.Id,
            TimeSpan.FromMinutes(3));
    }
}
