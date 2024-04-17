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
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions.Asserters;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Azure.Databricks.Client.Models;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using NodaTime;
using WireMock.Server;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Functions.Calculation;

[Collection(nameof(OrchestrationsAppCollectionFixture))]
public class CalculationOrchestrationTests : IAsyncLifetime
{
    private readonly DateTimeZone _dateTimeZone;
    private readonly WireMockServer _serverStub;

    public CalculationOrchestrationTests(
        OrchestrationsAppFixture fixture,
        ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.SetTestOutputHelper(testOutputHelper);

        Fixture.AppHostManager.ClearHostLog();
        _dateTimeZone = DateTimeZoneProviders.Tzdb["Europe/Copenhagen"];
        _serverStub = new WiremockFixture([OrchestrationsAppFixture.LocalhostUrl]).Server;
    }

    private OrchestrationsAppFixture Fixture { get; }

    public Task InitializeAsync()
    {
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
        var jobs = JsonConvert.SerializeObject(GenerateMockedJobs());

        _serverStub.Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/2.1/jobs/list")
                .UsingGet())
            .AtPriority(1)
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader(HeaderNames.ContentType, "application/json")
                .WithBody(Encoding.UTF8.GetBytes(jobs)));

        var job = JsonConvert.SerializeObject(GenerateMockedJob());

        _serverStub.Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/2.1/jobs/get")
                .UsingGet())
            .AtPriority(1)
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader(HeaderNames.ContentType, "application/json")
                .WithBody(Encoding.UTF8.GetBytes(job)));

        var run_now = JsonConvert.SerializeObject(GenerateMockedRunNow());

        _serverStub.Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/2.1/jobs/run-now")
                .UsingPost())
            .AtPriority(1)
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader(HeaderNames.ContentType, "application/json")
                .WithBody(Encoding.UTF8.GetBytes(run_now)));

        var run = JsonConvert.SerializeObject(GenerateMockedRun());

        _serverStub.Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/2.1/jobs/runs/get")
                .UsingGet())
            .AtPriority(1)
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader(HeaderNames.ContentType, "application/json")
                .WithBody(Encoding.UTF8.GetBytes(run)));

        // Act
        var todayAtMidnight = new LocalDate(2024, 5, 17)
            .AtMidnight()
            .InZoneStrictly(_dateTimeZone)
            .ToDateTimeOffset();

        using var actualResponse = await Fixture.AppHostManager.HttpClient.PostAsync(
            "api/StartCalculation",
            new StringContent(
                JsonConvert.SerializeObject(new BatchRequestDto(
                CalculationType.Aggregation,
                ["256", "512" ],
                todayAtMidnight,
                todayAtMidnight.AddDays(2))),
                Encoding.UTF8,
                "application/json"));

        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //Fixture.AzuriteManager.
        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("CreateCalculationRecordActivity");
        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("StartCalculationActivity");
        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("GetJobStatusActivity");

        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("UpdateCalculationExecutionStatusActivity", TimeSpan.FromMinutes(3));
        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("CreateCompletedCalculationActivity");
        await Fixture.AppHostManager.AssertFunctionWasExecutedAsync("SendCalculationResultsActivity");

        // Assert
        using var assertionScope = new AssertionScope();

        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().StartWith("{\"status\":\"Healthy\"");
    }

    private Run GenerateMockedRun()
    {
        return new Run { RunId = 512, State = new RunState { LifeCycleState = RunLifeCycleState.TERMINATED, ResultState = RunResultState.SUCCESS } };
    }

    private RunIdentifier GenerateMockedRunNow()
    {
        return new RunIdentifier { RunId = 512 };
    }

    private static object GenerateMockedJobs()
    {
        return new
        {
            jobs= new[]
            {
                GenerateMockedJob(),
            },
        };
    }

    private static Job GenerateMockedJob()
    {
        return new Job()
        {
            Settings = new JobSettings()
            {
                Name = "CalculatorJob",
            },
            JobId = 42,
        };
    }
}
