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

[ExecutionContext(AzureEnvironment.AnyDev)]
[TestCaseOrderer(
    ordererTypeName: "Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Orderers.ScenarioStepOrderer",
    ordererAssemblyName: "Energinet.DataHub.Wholesale.SubsystemTests")]
public class BalanceFixingCalculationScenario : SubsystemTestsBase<CalculationScenarioFixture>
{
    public BalanceFixingCalculationScenario(LazyFixtureFactory<CalculationScenarioFixture> lazyFixtureFactory)
        : base(lazyFixtureFactory)
    {
    }

    [ScenarioStep(0)]
    [SubsystemFact]
    public void Given_CalculationInput()
    {
        Fixture.ScenarioState.CalculationInput = new StartCalculationRequestDto(
            CalculationType: Common.Interfaces.Models.CalculationType.BalanceFixing,
            GridAreaCodes: ["543"],
            StartDate: new DateTimeOffset(2022, 1, 11, 23, 0, 0, TimeSpan.Zero),
            EndDate: new DateTimeOffset(2022, 1, 12, 23, 0, 0, TimeSpan.Zero),
            ScheduledAt: DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(4)),
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
    public async Task When_CalculationIsScheduledToStartLater()
    {
        Fixture.ScenarioState.CalculationId =
            await Fixture.StartCalculationAsync(Fixture.ScenarioState.CalculationInput!);

        // Assert
        Fixture.ScenarioState.CalculationId.Should().NotBeEmpty();
    }

    [ScenarioStep(3)]
    [SubsystemFact]
    public async Task Then_CalculationIsStartedAtScheduledTime()
    {
        var allowedTimeDifference = TimeSpan.FromMinutes(1);
        var scheduledAt = Fixture.ScenarioState.CalculationInput!.ScheduledAt;
        var maxWaitTime = scheduledAt - DateTimeOffset.UtcNow + allowedTimeDifference;

        var (isStarted, _) = await Fixture.WaitForScheduledCalculationToStartAsync(
            Fixture.ScenarioState.CalculationId,
            waitTimeLimit: maxWaitTime,
            checkInterval: TimeSpan.FromSeconds(10));

        var now = DateTimeOffset.UtcNow;

        using var assertionScope = new AssertionScope();

        // => Verify the calculation was actually started within the tolerated time
        isStarted.Should().BeTrue($"because the calculation should be started within {maxWaitTime.TotalSeconds} seconds");
        now.Should().BeCloseTo(
            scheduledAt,
            precision: allowedTimeDifference,
            $"because the scheduled calculation should be started within {allowedTimeDifference.TotalSeconds} seconds of the scheduled time");
    }

    [ScenarioStep(4)]
    [SubsystemFact]
    public async Task AndThen_CalculationIsCompletedWithinWaitTime()
    {
        var (isCompletedOrFailed, calculation) = await Fixture.WaitForCalculationCompletedOrFailedAsync(
            Fixture.ScenarioState.CalculationId,
            waitTimeLimit: TimeSpan.FromMinutes(21));

        Fixture.ScenarioState.Calculation = calculation;

        // Assert
        using var assertionScope = new AssertionScope();
        isCompletedOrFailed.Should().BeTrue("because calculation should complete within time limit.");
        calculation.Should().NotBeNull();
        calculation!.OrchestrationState.Should()
            .BeOneOf(CalculationOrchestrationStateExtensions.CalculationJobCompletedStates);
    }

    [ScenarioStep(5)]
    [SubsystemFact]
    public void AndThen_CalculationDurationIsLessThanOrEqualToTimeLimit()
    {
        // TODO JVM: Reduce time limit to 18 minutes, when only saving to unity catalog
        var calculationTimeLimit = TimeSpan.FromMinutes(30);
        var actualCalculationDuration =
            Fixture.ScenarioState.Calculation!.ExecutionTimeEnd - Fixture.ScenarioState.Calculation.ExecutionTimeStart;

        // Assert
        actualCalculationDuration.Should().BeGreaterThan(TimeSpan.Zero);
        actualCalculationDuration.Should().BeLessThanOrEqualTo(calculationTimeLimit);
    }

    [ScenarioStep(6)]
    [SubsystemFact]
    public async Task AndThen_IntegrationEventsAreReceivedWithinWaitTime()
    {
        // Skip waiting if calculation did not complete
        if (Fixture.ScenarioState.Calculation != null
            && Fixture.ScenarioState.Calculation.OrchestrationState.IsCalculationJobCompleted())
        {
            var actualReceivedIntegrationEvents = await Fixture.WaitForIntegrationEventsAsync(
                Fixture.ScenarioState.CalculationId,
                Fixture.ScenarioState.SubscribedIntegrationEventNames.AsReadOnly(),
                waitTimeLimit: TimeSpan.FromMinutes(8));

            Fixture.ScenarioState.ReceivedGridLossProducedV1 =
                actualReceivedIntegrationEvents.OfType<GridLossResultProducedV1>().ToList();
            Fixture.ScenarioState.ReceivedCalculationCompletedV1 = actualReceivedIntegrationEvents
                .OfType<CalculationCompletedV1>().ToList();
        }

        // Assert
        using var assertionScope = new AssertionScope();
        // => Not empty
        Fixture.ScenarioState.ReceivedGridLossProducedV1.Should().NotBeEmpty();
        Fixture.ScenarioState.ReceivedCalculationCompletedV1.Should().NotBeEmpty();
    }

    [ScenarioStep(7)]
    [SubsystemFact]
    public void AndThen_ReceivedGridLossProducedEventsCountIsEqualToExpected()
    {
        var expected = 2;

        // Assert
        using var assertionScope = new AssertionScope();
        Fixture.ScenarioState.ReceivedGridLossProducedV1.Count.Should().Be(expected);
    }

    [ScenarioStep(8)]
    [SubsystemFact]
    public void AndThen_ReceivedGridLossProducedV1ContainsOnlyOneConsumptionAndOneProductionMeteringPointType()
    {
        var actualMeteringPointTypesForGridLossProducedV1 = Fixture.ScenarioState.ReceivedGridLossProducedV1
            .Select(x => Enum.GetName(x.MeteringPointType))
            .ToList();

        // Assert
        using var assertionScope = new AssertionScope();
        actualMeteringPointTypesForGridLossProducedV1.Should().ContainSingle(x => x == GridLossResultProducedV1.Types.MeteringPointType.Consumption.ToString());
        actualMeteringPointTypesForGridLossProducedV1.Should().ContainSingle(x => x == GridLossResultProducedV1.Types.MeteringPointType.Production.ToString());
    }

    [ScenarioStep(9)]
    [SubsystemFact]
    public async Task AndThen_ReceivedReceivedGridLossProducedV1EventContainsExpectedTimeSeriesPoints()
    {
        // Arrange
        var expectedTimeSeriesPoints = await Fixture.ParseGridLossTimeSeriesPointsFromCsvAsync("Positive_gridLoss 543.csv");
        var energyResults = Fixture.ScenarioState.ReceivedGridLossProducedV1
            .Where(x => x.MeteringPointType == GridLossResultProducedV1.Types.MeteringPointType.Consumption)
            .Where(x => x.MeteringPointId == "571313154312753911")
            .Select(x => x.TimeSeriesPoints)
            .ToList();

        // Assert
        Assert.Single(energyResults);
        energyResults.First().Should().BeEquivalentTo(expectedTimeSeriesPoints);
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

        using var assertionScope = new AssertionScope();
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
}
