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

using System.Globalization;
using Azure.Monitor.Query;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Energinet.DataHub.Wholesale.SubsystemTests.Clients.v3;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations.Fixtures;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Attributes;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations;

[TestCaseOrderer(
    ordererTypeName: "Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Orderers.ScenarioStepOrderer",
    ordererAssemblyName: "Energinet.DataHub.Wholesale.SubsystemTests")]
public class WholesaleFixingCalculationScenario : SubsystemTestsBase<CalculationScenarioFixture>
{
    public WholesaleFixingCalculationScenario(LazyFixtureFactory<CalculationScenarioFixture> lazyFixtureFactory, string calculationVersion)
        : base(lazyFixtureFactory)
    {
        CalculationVersion = calculationVersion;
    }

    public string CalculationVersion { get; private set; }

    [ScenarioStep(-1)]
    [SubsystemFact]
    public async Task GetTheNewestCalculationVerionBeforeANewCalculationIsStarted()
    {
        // Arrange

        // Act
        var (calculationVersion, message) = await Fixture.GetLatestCalculationVersionFromCalculationsAsync();

        // Assert
        if (calculationVersion == string.Empty)
        {
            throw new InvalidOperationException(message);
        }

        // Set the property value
        CalculationVersion = calculationVersion;
    }

    [ScenarioStep(0)]
    [SubsystemFact]
    public void Given_CalculationInput()
    {
        Fixture.ScenarioState.CalculationInput = new StartCalculationRequestDto(
            CalculationType: Common.Interfaces.Models.CalculationType.WholesaleFixing,
            GridAreaCodes: new List<string> { "804" },
            StartDate: new DateTimeOffset(2023, 1, 31, 23, 0, 0, TimeSpan.Zero),
            EndDate: new DateTimeOffset(2023, 2, 28, 23, 0, 0, TimeSpan.Zero),
            ScheduledAt: DateTimeOffset.UtcNow,
            false);
    }

    [ScenarioStep(1)]
    [SubsystemFact]
    public void AndGiven_SubscribedIntegrationEvents()
    {
        Fixture.ScenarioState.SubscribedIntegrationEventNames.Add(GridLossResultProducedV1.EventName);
        Fixture.ScenarioState.SubscribedIntegrationEventNames.Add(CalculationCompletedV1.EventName);
    }

    [ScenarioStep(2)]
    [SubsystemFact]
    public async Task When_CalculationIsStarted()
    {
        Fixture.ScenarioState.CalculationId =
            await Fixture.StartCalculationAsync(Fixture.ScenarioState.CalculationInput!);

        // Assert
        Fixture.ScenarioState.CalculationId.Should().NotBeEmpty();
    }

    [ScenarioStep(3)]
    [SubsystemFact]
    public async Task Then_CalculationExecutionIsCompletedWithinWaitTime()
    {
        var (isCompletedOrFailed, calculation) = await Fixture.WaitForCalculationCompletedOrFailedAsync(
            Fixture.ScenarioState.CalculationId,
            waitTimeLimit: TimeSpan.FromMinutes(33));

        Fixture.ScenarioState.Calculation = calculation;

        // Assert
        using var assertionScope = new AssertionScope();
        isCompletedOrFailed.Should().BeTrue("because calculation should complete within time limit.");
        calculation.Should().NotBeNull();
        calculation!.OrchestrationState.Should().BeOneOf(CalculationOrchestrationStateExtensions.CalculationJobCompletedStates);
    }

    [ScenarioStep(4)]
    [SubsystemFact]
    public void AndThen_CalculationDurationIsLessThanOrEqualToTimeLimit()
    {
        var calculationTimeLimit = TimeSpan.FromMinutes(30);
        var actualCalculationDuration =
            Fixture.ScenarioState.Calculation!.ExecutionTimeEnd - Fixture.ScenarioState.Calculation.ExecutionTimeStart;

        // Assert
        actualCalculationDuration.Should().BeGreaterThan(TimeSpan.Zero);
        actualCalculationDuration.Should().BeLessThanOrEqualTo(calculationTimeLimit);
    }

    [ScenarioStep(5)]
    [SubsystemFact]
    public async Task AndThen_IntegrationEventsAreReceivedWithinWaitTime()
    {
        var actualReceivedIntegrationEvents = await Fixture.WaitForIntegrationEventsAsync(
            Fixture.ScenarioState.CalculationId,
            Fixture.ScenarioState.SubscribedIntegrationEventNames.AsReadOnly(),
            waitTimeLimit: TimeSpan.FromMinutes(8));

        Fixture.ScenarioState.ReceivedGridLossProducedV1 = actualReceivedIntegrationEvents
            .OfType<GridLossResultProducedV1>().ToList();
        Fixture.ScenarioState.ReceivedCalculationCompletedV1 = actualReceivedIntegrationEvents
            .OfType<CalculationCompletedV1>().ToList();

        // Assert
        using var assertionScope = new AssertionScope();
        Fixture.ScenarioState.ReceivedGridLossProducedV1.Should().NotBeEmpty();
        Fixture.ScenarioState.ReceivedCalculationCompletedV1.Should().NotBeEmpty();
    }

    [ScenarioStep(6)]
    [SubsystemFact]
    public async Task AndThen_ACalculationTelemetryLogIsCreated()
    {
        var query = $@"
AppTraces
| where AppRoleName == ""dbr-calculation-engine""
| where SeverityLevel == 1 // Information
| where Message startswith_cs ""Command line arguments:""
| where OperationId != ""00000000000000000000000000000000""
| where Properties.Subsystem == ""wholesale-aggregations""
| where Properties.calculation_id == ""{Fixture.ScenarioState.CalculationId}""
| where Properties.CategoryName == ""Energinet.DataHub.package.calculator_job_args""
| count";

        // Assert
        var actual = await Fixture.QueryLogAnalyticsAsync(query, new QueryTimeRange(TimeSpan.FromMinutes(60)));

        using var assertionScope = new AssertionScope();
        actual.Value.Table.Rows[0][0].Should().Be(1); // count == 1
    }

    [ScenarioStep(7)]
    [SubsystemFact]
    public async Task AndThen_ACalculationTelemetryTraceWithASpanIsCreated()
    {
        var query = $@"
AppDependencies
| where Target == ""exchange""
| where Name == ""exchange""
| where DependencyType == ""InProc""
| where Success == true
| where ResultCode == 0
| where AppRoleName == ""dbr-calculation-engine""
| where Properties.Subsystem == ""wholesale-aggregations""
| where Properties.calculation_id == ""{Fixture.ScenarioState.CalculationId}""
| count";

        // Assert
        var actual = await Fixture.QueryLogAnalyticsAsync(query, new QueryTimeRange(TimeSpan.FromMinutes(60)));

        using var assertionScope = new AssertionScope();
        actual.Value.Table.Rows[0][0].Should().Be(1); // count == 1
    }

    [ScenarioStep(8)]
    [SubsystemFact]
    public async Task AndThen_ReceivedGridLossResultProducedV1EventContainsExpectedTimeSeriesPoints()
    {
        // Arrange
        var expectedTimeSeriesPoints = await Fixture.ParseGridLossTimeSeriesPointsFromCsvAsync("Positive_gridLoss 804.csv");
        var energyResults = Fixture.ScenarioState.ReceivedGridLossProducedV1
            .Where(x => x.MeteringPointType == GridLossResultProducedV1.Types.MeteringPointType.Consumption)
            .Where(x => x.MeteringPointId == "571313180400100657")
            .Select(x => x.TimeSeriesPoints)
            .ToList();

        // Assert
        Assert.Single(energyResults);
        energyResults.First().Should().BeEquivalentTo(expectedTimeSeriesPoints);
    }

    [ScenarioStep(9)]
    [SubsystemFact]
    public async Task AndThen_OneViewOrTableInEachPublicDataModelMustExistsAndContainData()
    {
        // Arrange
        var publicDataModelsAndTables = new List<(string ModelName, string TableName)>
        {
            new("wholesale_settlement_reports", "metering_point_periods_v1"),
            new("wholesale_results", "energy_v1"),
        };

        // Act
        var results = await Fixture.ArePublicDataModelsAccessibleAsync(publicDataModelsAndTables);

        // Assert
        using var assertionScope = new AssertionScope();
        foreach (var actual in results)
        {
            actual.IsAccessible.Should().Be(true, actual.ErrorMessage);
        }
    }

    [ScenarioStep(10)]
    [SubsystemFact]
    public void AndThen_ReceivedCalculationCompletedV1EventContainsSingleEventWithInstanceId()
    {
        // Assert
        var receivedCalculationCompletedEvent = Fixture.ScenarioState.ReceivedCalculationCompletedV1.Should().ContainSingle()
            .Subject;

        receivedCalculationCompletedEvent.InstanceId.Should().NotBeNullOrWhiteSpace();
        Fixture.ScenarioState.OrchestrationInstanceId = receivedCalculationCompletedEvent.InstanceId;
    }

    [ScenarioStep(11)]
    [SubsystemFact]
    public async Task AndThen_CalculationShouldBeInActorMessagesEnqueuingState()
    {
        // Wait for the calculation to reach the ActorMessagesEnqueuing state
        // We need to watch for ActorMessagesEnqueued or Completed as well, since the EDI subsystem
        // could have already handled the request and sent an ActorMessagesEnqueued message back to
        // the Wholesale subsystem already
        var (isSuccess, calculation) = await Fixture.WaitForOneOfCalculationStatesAsync(
            Fixture.ScenarioState.CalculationId,
            [
                CalculationOrchestrationState.ActorMessagesEnqueuing,
                CalculationOrchestrationState.ActorMessagesEnqueued,
                CalculationOrchestrationState.Completed
            ],
            waitTimeLimit: TimeSpan.FromMinutes(1));

        isSuccess.Should().BeTrue("because calculation should be in ActorMessagesEnqueuing state or later");
        calculation.Should().NotBeNull();
        calculation!.OrchestrationState.Should().BeOneOf(
            [
                CalculationOrchestrationState.ActorMessagesEnqueuing,
                CalculationOrchestrationState.ActorMessagesEnqueued,
                CalculationOrchestrationState.Completed
            ],
            "because calculation should be in ActorMessagesEnqueuing state or later");
    }

    [ScenarioStep(12)]
    [SubsystemFact]
    public async Task AndThen_ActorMessagesEnqueuedMessageIsReceived()
    {
        // Send a ActorMessagesEnqueued message to the Wholesale subsystem
        // This must not fail even if the message has already been received from the EDI subsystem
        await Fixture.SendActorMessagesEnqueuedMessageAsync(
            Fixture.ScenarioState.CalculationId,
            Fixture.ScenarioState.OrchestrationInstanceId);
    }

    [ScenarioStep(13)]
    [SubsystemFact]
    public async Task AndThen_CalculationOrchestrationIsCompleted()
    {
        // Wait for the calculation to reach the Completed state
        var (isSuccess, calculation) = await Fixture.WaitForOneOfCalculationStatesAsync(
            Fixture.ScenarioState.CalculationId,
            [CalculationOrchestrationState.Completed],
            waitTimeLimit: TimeSpan.FromMinutes(1));

        using var assertionScope = new AssertionScope();
        isSuccess.Should().BeTrue("because the calculation should be completed");
        calculation.Should().NotBeNull();
        calculation!.OrchestrationState.Should().Be(CalculationOrchestrationState.Completed);
    }

    [ScenarioStep(14)]
    [SubsystemFact]
    public async Task AndThen_CheckThatIdentityColumnOnCalculationsIsWorkingCorrectly()
    {
        // Arrange

        // Act
        var (calculationVersion, message) = await Fixture.GetCalculationVersionOfCalculationIdFromCalculationsAsync(Fixture.ScenarioState.CalculationId.ToString());

        // Assert
        if (calculationVersion == string.Empty)
        {
            throw new InvalidOperationException(message);
        }

        Assert.True(Convert.ToInt32(calculationVersion) > Convert.ToInt32(CalculationVersion));
    }
}
