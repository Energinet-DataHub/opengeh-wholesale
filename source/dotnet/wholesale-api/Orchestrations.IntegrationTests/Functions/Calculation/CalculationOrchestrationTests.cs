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

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Energinet.DataHub.EnergySupplying.RequestResponse.InboxEvents;
using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.DurableTask;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NodaTime;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Functions.Calculation;

[Collection(nameof(OrchestrationsAppCollectionFixture))]
public class CalculationOrchestrationTests : IAsyncLifetime
{
    public CalculationOrchestrationTests(
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
        // => Databricks Jobs API
        var jobId = Random.Shared.Next(1, 1000);
        var runId = Random.Shared.Next(1000, 2000);

        Fixture.MockServer
            .MockJobsList(jobId)
            .MockJobsGet(jobId)
            .MockJobsRunNow(runId)
            .MockJobsRunsGet(runId, "TERMINATED", "SUCCESS");

        // => Databricks SQL Statement API
        var chunkIndex = 0;
        var statementId = Guid.NewGuid().ToString();
        var path = "GetDatabricksDataPath";

        // This is the calculationId returned in the energyResult from the mocked databricks.
        // It should match the ID returned by the http client calling 'api/StartCalculation'.
        // The mocked response waits for this to not be null before responding, so it must be updated
        // when we have the actual id.
        Guid? calculationId = null;

        Fixture.MockServer
            .MockEnergySqlStatements(statementId, chunkIndex)
            .MockEnergySqlStatementsResultChunks(statementId, chunkIndex, path)
            // ReSharper disable once AccessToModifiedClosure -- We need to modify calculation id in outer scope
            // when we get a response from 'api/StartCalculation'
            .MockEnergySqlStatementsResultStream(path, () => calculationId);

        // => Request
        using var request = CreateStartCalculationRequest();

        // Act
        var beforeOrchestrationCreated = DateTime.UtcNow;
        using var actualResponse = await Fixture.AppHostManager.HttpClient.SendAsync(request);

        // Assert
        // => Verify endpoint response
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        calculationId = await actualResponse.Content.ReadFromJsonAsync<Guid>();

        // => Verify expected behaviour by searching the orchestration history
        var orchestrationStatus = await Fixture.DurableClient.FindOrchestationStatusAsync(createdTimeFrom: beforeOrchestrationCreated);

        // => Function has the expected calculation id
        var calculationMetadata = orchestrationStatus.CustomStatus.ToObject<CalculationMetadata>();
        calculationMetadata!.Id.Should().Be(calculationId.Value);

        // => Wait for the orchestration to reach the "ActorMessagesEnqueuing" state
        await Fixture.DurableClient.WaitForCustomStatusAsync<CalculationMetadata>(orchestrationStatus.InstanceId, (status) => status.OrchestrationProgress == "ActorMessagesEnqueuing");

        // => Raise "ActorMessagesEnqueued" event to the orchestrator
        await Fixture.DurableClient.RaiseEventAsync(
            orchestrationStatus.InstanceId,
            MessagesEnqueuedV1.EventName,
            new MessagesEnqueuedV1
            {
                CalculationId = calculationId.ToString(),
                OrchestrationInstanceId = orchestrationStatus.InstanceId,
            });

        // => Wait for completion, this should be fairly quick, since we have mocked databricks
        var completeOrchestrationStatus = await Fixture.DurableClient.WaitForInstanceCompletedAsync(
            orchestrationStatus.InstanceId,
            TimeSpan.FromMinutes(3));

        // => Expect history
        using var assertionScope = new AssertionScope();

        var activities = completeOrchestrationStatus.History
            .OrderBy(item => item["Timestamp"])
            .Select(item => item.ToObject<OrchestrationHistoryItem>())
            .ToList();

        activities.Should().NotBeNull().And.Equal(
        [
            new OrchestrationHistoryItem("ExecutionStarted", FunctionName: "CalculationOrchestration"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "CreateCalculationRecordActivity"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "StartCalculationActivity"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "GetJobStatusActivity"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "UpdateCalculationStatusActivity"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "CreateCompletedCalculationActivity"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "SendCalculationResultsActivity"),
            new OrchestrationHistoryItem("TimerCreated"), // Wait for raised event (ActorMessagesEnqueued)
            new OrchestrationHistoryItem("EventRaised", Name: "MessagesEnqueuedV1"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "UpdateCalculationOrchestrationStateActivity"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "UpdateCalculationOrchestrationStateActivity"),
            new OrchestrationHistoryItem("ExecutionCompleted"),
        ]);

        // => Verify that the durable function completed successfully
        var last = completeOrchestrationStatus.History
            .OrderBy(item => item["Timestamp"])
            .Last();
        last.Value<string>("EventType").Should().Be("ExecutionCompleted");
        last.Value<string>("Result").Should().Be("Success");

        // => Verify that the expected message was sent on the ServiceBus
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
                return erp.CalculationId == calculationId.Value.ToString();
            })
            .VerifyCountAsync(1);

        var wait = verifyServiceBusMessages.Wait(TimeSpan.FromMinutes(1));
        wait.Should().BeTrue("We did not send the expected message on the ServiceBus");
    }

    /// <summary>
    /// Verify the job status monitor (loop) is working with the expected job status state changes.
    /// </summary>
    [Fact]
    public async Task MockJobsRunsGetLifeCycleScenario_WhenCallingStartCalculationEndPoint_CalculationJobCompletesWithExpectedGetJobStatusActivity()
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

        // => Request
        using var request = CreateStartCalculationRequest();

        // Act
        var beforeOrchestrationCreated = DateTime.UtcNow;
        using var actualResponse = await Fixture.AppHostManager.HttpClient.SendAsync(request);

        // Assert
        // => Verify endpoint response
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var calculationId = await actualResponse.Content.ReadFromJsonAsync<Guid>();

        // => Verify expected behaviour by searching the orchestration history
        var orchestrationStatus = await Fixture.DurableClient.FindOrchestationStatusAsync(createdTimeFrom: beforeOrchestrationCreated);

        // => Expect calculation id
        var calculationMetadata = orchestrationStatus.CustomStatus.ToObject<CalculationMetadata>();
        calculationMetadata!.Id.Should().Be(calculationId);

        // => Wait for calculation job to be completed
        var completeOrchestrationStatus = await Fixture.DurableClient.WaitForCustomStatusAsync<CalculationMetadata>(
            orchestrationStatus.InstanceId,
            status => status.JobStatus == CalculationState.Completed,
            TimeSpan.FromMinutes(1)); // We will loop at least twice to get job status

        // => Expect history
        using var assertionScope = new AssertionScope();
        var first = completeOrchestrationStatus.History.First();
        first.Value<string>("FunctionName").Should().Be("CalculationOrchestration");

        // => Job status (loop)
        var getJobStatusResults = completeOrchestrationStatus.History
            .Where(item => item.Value<string>("FunctionName") == "GetJobStatusActivity")
            .OrderBy(item => item["Timestamp"])
            .Select(item => item.Value<string>("Result"))
            .ToList();

        getJobStatusResults.Should().NotBeNull().And.Equal(
        [
            ((int)CalculationState.Pending).ToString(),
            ((int)CalculationState.Running).ToString(),
            ((int)CalculationState.Completed).ToString(),
        ]);
    }

    /// <summary>
    /// Verify the job status monitor (loop) breaks if we reach expiry time.
    /// </summary>
    [Fact]
    public async Task MockJobsRunsGetAsRunning_WhenCallingStartCalculationEndpointAndCalculationTimeoutIsExceeded_OrchestrationCompletesWithExpectedGetJobStatusActivity()
    {
        // Arrange
        // => Databricks Jobs API
        var jobId = Random.Shared.Next(1, 1000);
        var runId = Random.Shared.Next(1000, 2000);

        Fixture.MockServer
            .MockJobsList(jobId)
            .MockJobsGet(jobId)
            .MockJobsRunNow(runId)
            .MockJobsRunsGet(runId, "RUNNING", "EXCLUDED");

        // => Request
        using var request = CreateStartCalculationRequest();

        // Act
        var beforeOrchestrationCreated = DateTime.UtcNow;
        using var actualResponse = await Fixture.AppHostManager.HttpClient.SendAsync(request);

        // Assert
        // => Verify endpoint response
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var calculationId = await actualResponse.Content.ReadFromJsonAsync<Guid>();

        // => Verify expected behaviour by searching the orchestration history
        var orchestrationStatus = await Fixture.DurableClient.FindOrchestationStatusAsync(createdTimeFrom: beforeOrchestrationCreated);

        // => Expect calculation id
        var calculationMetadata = orchestrationStatus.CustomStatus.ToObject<CalculationMetadata>();
        calculationMetadata!.Id.Should().Be(calculationId);

        // => Wait for completion
        var completeOrchestrationStatus = await Fixture.DurableClient.WaitForInstanceCompletedAsync(
            orchestrationStatus.InstanceId,
            TimeSpan.FromMinutes(1)); // We will loop at least until expiry time has been reached

        // => Expect history
        using var assertionScope = new AssertionScope();
        var first = completeOrchestrationStatus.History.First();
        first.Value<string>("FunctionName").Should().Be("CalculationOrchestration");

        var last = completeOrchestrationStatus.History.Last();
        var lastResult = new { EventType = last.Value<string>("EventType"), Result = last.Value<string>("Result") };
        lastResult.EventType.Should().Be("ExecutionCompleted");
        lastResult.Result.Should().Be("Error: Job status 'Running'");
    }

    private static HttpRequestMessage CreateStartCalculationRequest()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/StartCalculation");

        var dateTimeZone = DateTimeZoneProviders.Tzdb["Europe/Copenhagen"];
        var dateAtMidnight = new LocalDate(2024, 5, 17)
            .AtMidnight()
            .InZoneStrictly(dateTimeZone)
            .ToDateTimeOffset();

        // Input parameters
        var requestDto = new StartCalculationRequestDto(
            CalculationType.Aggregation,
            GridAreaCodes: ["256", "512"],
            StartDate: dateAtMidnight,
            EndDate: dateAtMidnight.AddDays(2));

        request.Content = new StringContent(
            JsonConvert.SerializeObject(requestDto),
            Encoding.UTF8,
            "application/json");

        var token = CreateFakeInternalToken();
        request.Headers.Add("Authorization", $"Bearer {token}");

        return request;
    }

    /// <summary>
    /// Create a fake token which is used by the 'UserMiddleware' to create
    /// the 'UserContext'.
    /// </summary>
    private static string CreateFakeInternalToken()
    {
        var kid = "049B6F7F-F5A5-4D2C-A407-C4CD170A759F";
        RsaSecurityKey testKey = new(RSA.Create()) { KeyId = kid };

        var issuer = "https://test.datahub.dk";
        var audience = Guid.Empty.ToString();
        var validFrom = DateTime.UtcNow;
        var validTo = DateTime.UtcNow.AddMinutes(15);

        var userClaim = new Claim(JwtRegisteredClaimNames.Sub, "A1AAB954-136A-444A-94BD-E4B615CA4A78");
        var actorClaim = new Claim(JwtRegisteredClaimNames.Azp, "A1DEA55A-3507-4777-8CF3-F425A6EC2094");

        var internalToken = new JwtSecurityToken(
            issuer,
            audience,
            new[] { userClaim, actorClaim },
            validFrom,
            validTo,
            new SigningCredentials(testKey, SecurityAlgorithms.RsaSha256));

        var handler = new JwtSecurityTokenHandler();
        var writtenToken = handler.WriteToken(internalToken);
        return writtenToken;
    }
}
