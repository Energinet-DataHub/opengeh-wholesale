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
using System.Text;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Azure.Databricks.Client.Models;
using Newtonsoft.Json;
using NodaTime;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Functions.Calculation;

[Collection(nameof(OrchestrationsAppCollectionFixture))]
public class CalculationOrchestrationTests : IAsyncLifetime
{
    private readonly DateTimeZone _dateTimeZone;

    public CalculationOrchestrationTests(
        OrchestrationsAppFixture fixture,
        ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.SetTestOutputHelper(testOutputHelper);

        _dateTimeZone = DateTimeZoneProviders.Tzdb["Europe/Copenhagen"];
    }

    private OrchestrationsAppFixture Fixture { get; }

    public Task InitializeAsync()
    {
        Fixture.AppHostManager.ClearHostLog();

        // Clear mappings etc. before each test
        Fixture.MockServer.Reset();

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Fixture.SetTestOutputHelper(null!);

        return Task.CompletedTask;
    }

    [Fact]
    public async Task FunctionApp_WhenCallingDurableFunctionEndPoint_ReturnOKAndExpectedContent()
    {
        // Arrange
        var jobId = Random.Shared.Next(1, 1000);
        var runId = Random.Shared.Next(1000, 2000);
        Fixture.MockServer
            .CatchAll()
            .MockJobsList(jobId)
            .MockJobsGet(jobId)
            .MockJobsRunNow(runId)
            .MockJobsRunsGet(runId, "TERMINATED", "SUCCESS");

        // Act
        var todayAtMidnight = new LocalDate(2024, 5, 17)
            .AtMidnight()
            .InZoneStrictly(_dateTimeZone)
            .ToDateTimeOffset();

        using var actualResponse = await Fixture.AppHostManager.HttpClient.PostAsync(
            "api/StartCalculation",
            new StringContent(
                JsonConvert.SerializeObject(new CalculationRequestDto(
                CalculationType.Aggregation,
                ["256", "512"],
                todayAtMidnight,
                todayAtMidnight.AddDays(2))),
                Encoding.UTF8,
                "application/json"));

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("CreateCalculationRecordActivity");
        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("StartCalculationActivity");
        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("GetJobStatusActivity");

        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("UpdateCalculationExecutionStatusActivity", TimeSpan.FromMinutes(5));
        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("CreateCompletedCalculationActivity", TimeSpan.FromMinutes(5));
        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("SendCalculationResultsActivity", TimeSpan.FromMinutes(5));

        // TODO: Wait for events on ServiceBus using "listener mock"
    }
}
