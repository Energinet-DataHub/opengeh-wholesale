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
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.DurableTask;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using NodaTime;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Functions.Calculation;

[Collection(nameof(OrchestrationsAppCollectionFixture))]
public class CalculationOrchestrationTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly DateTimeZone _dateTimeZone;

    public CalculationOrchestrationTests(
        OrchestrationsAppFixture fixture,
        ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        Fixture = fixture;
        Fixture.SetTestOutputHelper(testOutputHelper);

        _dateTimeZone = DateTimeZoneProviders.Tzdb["Europe/Copenhagen"];
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

    [Fact]
    public async Task FunctionApp_WhenCallingDurableFunctionEndPoint_ReturnOKAndExpectedContent()
    {
        // Arrange
        var jobId = Random.Shared.Next(1, 1000);
        var runId = Random.Shared.Next(1000, 2000);

        var chunkIndex = 0;
        var statementId = Guid.NewGuid().ToString();
        var path = "GetDatabricksDataPath";

        // This is the calculationId returned in the energyResult from the mocked databricks.
        // It should match the ID returned by the http client calling 'api/StartCalculation'
        // But we have to set up the mocked response before we reach this step, hence we have a mismatch.
        var calculationIdInMock = Guid.NewGuid();

        Fixture.MockServer
            .MockJobsList(jobId)
            .MockJobsGet(jobId)
            .MockJobsRunNow(runId)
            .MockJobsRunsGet(runId, "TERMINATED", "SUCCESS")
            .MockEnergySqlStatements(statementId, chunkIndex)
            .MockEnergySqlStatementsResultChunks(statementId, chunkIndex, path)
            .MockEnergySqlStatementsResultStream(path, calculationIdInMock);

        // Act
        var todayAtMidnight = new LocalDate(2024, 5, 17)
            .AtMidnight()
            .InZoneStrictly(_dateTimeZone)
            .ToDateTimeOffset();

        var beforeCreated = DateTime.UtcNow;
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
        var calculationId = await actualResponse.Content.ReadAsAsync<Guid>();

        // Verify activities was executed by searching the orchestration history
        var filter = new OrchestrationStatusQueryCondition()
        {
            CreatedTimeFrom = beforeCreated,
            RuntimeStatus =
            [
                OrchestrationRuntimeStatus.Running,
                OrchestrationRuntimeStatus.Completed,
            ],
        };
        // => If we only need to verify information in custom status we can do it using the instance we can get from 'ListInstancesAsync'
        var queryResult = await Fixture.DurableClient.ListInstancesAsync(filter, CancellationToken.None);
        var orchestration = queryResult.DurableOrchestrationState.Single();
        var calculationMetadata = orchestration.CustomStatus.ToObject<CalculationMetadata>();
        calculationMetadata!.Id.Should().Be(calculationId);
        // => But if we want to verify information in history or output, we must use 'GetStatusAsync'
        var completeOrchestrationStatus = await Fixture.DurableClient.GetStatusAsync(orchestration.InstanceId, showHistory: true, showHistoryOutput: true);
        var orderedHistoryEntries = completeOrchestrationStatus.History
            .OrderBy(entry => entry["Timestamp"])
            .ToList();
        // => Just showing how we can verify the execution by looking in history. Instead of using the function app log, which is sketchy, we could use history.
        orderedHistoryEntries
            .First()
            .Value<string>("FunctionName").Should().Be("Calculation");

        // Verify activities was executed by searching the function app log
        // TODO: We should refactor the test to wait for the orchestration to be completed (or failed),
        // and then verify everything in history. This allows us to implement more precise tests, as the history contains
        // more and precise information compard to the function app log.
        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("CreateCalculationRecordActivity");
        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("StartCalculationActivity");
        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("GetJobStatusActivity");

        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("UpdateCalculationExecutionStatusActivity");
        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("CreateCompletedCalculationActivity");
        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("SendCalculationResultsActivity");

        var verifyServiceBusMessages = await Fixture.ServiceBusListenerMock
            .When(msg =>
            {
                if (msg.Subject != EnergyResultProducedV2.EventName)
                {
                    return false;
                }

                var erp = EnergyResultProducedV2.Parser.ParseFrom(msg.Body);

                // This should be the calculationId in "actualResponse".
                // But the current implementation takes the calculationId from the databricks row,
                // which is mocked in this scenario. Giving us a "false" comparison here.
                return erp.CalculationId == calculationIdInMock.ToString();
            })
            .VerifyCountAsync(1);

        var wait = verifyServiceBusMessages.Wait(TimeSpan.FromMinutes(1));
        wait.Should().BeTrue("We did not receive the expected message on the ServiceBus");
    }

    /// <summary>
    /// Verify the job status monitor (loop) is working.
    /// </summary>
    [Fact]
    public async Task MockJobStatus_WhenCallingStartCalculationEndPoint_OrchestrationCompletesWithExpectedHistory()
    {
        // Arrange
        // => Databricks Jobs API
        var jobId = Random.Shared.Next(1, 1000);
        var runId = Random.Shared.Next(1000, 2000);
        Fixture.MockServer
            .MockJobsList(jobId)
            .MockJobsGet(jobId)
            .MockJobsRunNow(runId)
            .MockJobsRunsGetLifeCycleScenario(runId);

        // => Databricks SQL Statement API
        var chunkIndex = 0;
        var statementId = Guid.NewGuid().ToString();
        var path = "GetDatabricksDataPath";
        Fixture.MockServer
            .MockEnergySqlStatements(statementId, chunkIndex)
            .MockEnergySqlStatementsResultChunks(statementId, chunkIndex, path)
            .MockEnergySqlStatementsResultStream(path);

        // Act
        var dateAtMidnight = new LocalDate(2024, 5, 17)
            .AtMidnight()
            .InZoneStrictly(_dateTimeZone)
            .ToDateTimeOffset();

        var beforeCreated = DateTime.UtcNow;
        using var actualResponse = await Fixture.AppHostManager.HttpClient.PostAsync(
            "api/StartCalculation",
            new StringContent(
                JsonConvert.SerializeObject(new CalculationRequestDto(
                CalculationType.Aggregation,
                ["256", "512"],
                dateAtMidnight,
                dateAtMidnight.AddDays(2))),
                Encoding.UTF8,
                "application/json"));

        // Assert
        // => Verify endpoint response
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var calculationId = await actualResponse.Content.ReadAsAsync<Guid>();

        // => Verify expected behaviour by searching the orchestration history
        var orchestrationStatus = await Fixture.DurableClient.FindOrchestationStatusAsync(createdTimeFrom: beforeCreated);

        // => Expect calculation id
        var calculationMetadata = orchestrationStatus.CustomStatus.ToObject<CalculationMetadata>();
        calculationMetadata!.Id.Should().Be(calculationId);

        // => Wait for completion
        var completeOrchestrationStatus = await Fixture.DurableClient.WaitForInstanceCompletedAsync(
            orchestrationStatus.InstanceId,
            TimeSpan.FromMinutes(6)); // We will loop at least twice to get job status

        // => Expect history
        using var assertionScope = new AssertionScope();
        var first = completeOrchestrationStatus.History.First();
        first.Value<string>("FunctionName").Should().Be("Calculation");

        var last = completeOrchestrationStatus.History.Last();
        last.Value<string>("EventType").Should().Be("ExecutionCompleted");
        last.Value<string>("Result").Should().Be("Success");

        // => Job status (loop)
        var getJobStatus = completeOrchestrationStatus.History
            .Where(item => item.Value<string>("FunctionName") == "GetJobStatusActivity")
            .OrderBy(item => item["Timestamp"])
            .ToList();
        getJobStatus.Count().Should().Be(3);
        getJobStatus.ElementAt(0).Value<string>("Result").Should().Be("0");
        getJobStatus.ElementAt(1).Value<string>("Result").Should().Be("1");
        getJobStatus.ElementAt(2).Value<string>("Result").Should().Be("2");
    }
}
