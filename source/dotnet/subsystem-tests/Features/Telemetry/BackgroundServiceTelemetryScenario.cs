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

using Azure.Monitor.Query;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.Telemetry.Fixtures;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.Telemetry.States;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Attributes;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Telemetry;

/// <summary>
/// Verify telemetry is configured correctly so traces are logged from background services.
/// </summary>
[TestCaseOrderer(
    ordererTypeName: "Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Orderers.ScenarioStepOrderer",
    ordererAssemblyName: "Energinet.DataHub.Wholesale.SubsystemTests")]
public class BackgroundServiceTelemetryScenario : SubsystemTestsBase<TelemetryScenarioFixture<BackgroundServiceTelemetryScenarioState>>
{
    public BackgroundServiceTelemetryScenario(LazyFixtureFactory<TelemetryScenarioFixture<BackgroundServiceTelemetryScenarioState>> lazyFixtureFactory)
        : base(lazyFixtureFactory)
    {
    }

    [ScenarioStep(0)]
    [SubsystemFact]
    public void Given_CalculationInput()
    {
        Fixture.ScenarioState.CalculationInput = new Clients.v3.CalculationRequestDto
        {
            CalculationType = Clients.v3.CalculationType.Aggregation,
            GridAreaCodes = new List<string> { "543" },
            StartDate = new DateTimeOffset(2022, 1, 13, 23, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2022, 1, 14, 23, 0, 0, TimeSpan.Zero),
        };
    }

    [ScenarioStep(1)]
    [SubsystemFact]
    public async Task AndGiven_CalculationIsStarted()
    {
        Fixture.ScenarioState.CalculationId = await Fixture.StartCalculationAsync(Fixture.ScenarioState.CalculationInput);

        // Assert
        Fixture.ScenarioState.CalculationId.Should().NotBeEmpty();
    }

    [ScenarioStep(2)]
    [SubsystemFact]
    public void When_ExpectedTelemetryEvents()
    {
        // From 'StartCalculationHandler' (handled in Timer Trigger request pipeline within Orchestration host)
        Fixture.ScenarioState.ExpectedTelemetryEvents.Add(new AppTraceMatch
        {
            AppVersionContains = "PR:",
            Subsystem = "wholesale",
            MessageContains = $"Calculation with id {Fixture.ScenarioState.CalculationId} started",
        });
        // From 'IntegrationEventProvider' (handled in background service)
        Fixture.ScenarioState.ExpectedTelemetryEvents.Add(new AppTraceMatch
        {
            AppVersionContains = "PR:",
            Subsystem = "wholesale",
            MessageContains = $"Published results for succeeded energy calculation {Fixture.ScenarioState.CalculationId} to the service bus",
        });
    }

    [ScenarioStep(3)]
    [SubsystemFact]
    public async Task Then_TelemetryEventsAreLoggedWithinWaitTime()
    {
        var query = $@"
                AppTraces
                | where AppRoleName contains ""-wholsal-""
                | where Message contains ""{Fixture.ScenarioState.CalculationId}""
                | extend parsedProp = parse_json(Properties)
                | project TimeGenerated, OperationId, ParentId, Type, AppVersion, Subsystem=parsedProp.Subsystem, EventName=parsedProp.EventName, Message
                | order by TimeGenerated asc";

        var wasEventsLogged = await Fixture.WaitForTelemetryEventsAsync(
            Fixture.ScenarioState.ExpectedTelemetryEvents.AsReadOnly(),
            query,
            queryTimeRange: new QueryTimeRange(TimeSpan.FromMinutes(15)),
            waitTimeLimit: TimeSpan.FromMinutes(15),
            delay: TimeSpan.FromSeconds(60));

        wasEventsLogged.Should().BeTrue($"{nameof(Fixture.ScenarioState.ExpectedTelemetryEvents)} was not logged to Application Insights within time limit.");
    }
}
