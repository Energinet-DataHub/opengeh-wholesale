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

using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations.Fixtures;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Attributes;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations
{
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
            Fixture.ScenarioState.CalculationInput = new DomainTests.Clients.v3.BatchRequestDto
            {
                ProcessType = DomainTests.Clients.v3.ProcessType.BalanceFixing,
                GridAreaCodes = new List<string> { "543" },
                StartDate = new DateTimeOffset(2022, 1, 11, 23, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2022, 1, 12, 23, 0, 0, TimeSpan.Zero),
            };
        }

        [ScenarioStep(1)]
        [SubsystemFact]
        public void AndGiven_SubscribedIntegrationEvents()
        {
            Fixture.ScenarioState.SubscribedIntegrationEventNames.Add(CalculationResultCompleted.EventName);
            Fixture.ScenarioState.SubscribedIntegrationEventNames.Add(EnergyResultProducedV2.EventName);
        }

        [ScenarioStep(2)]
        [SubsystemFact]
        public async Task When_CalculationIsStarted()
        {
            Fixture.ScenarioState.CalculationId =
                await Fixture.StartCalculationAsync(Fixture.ScenarioState.CalculationInput);

            // Assert
            Fixture.ScenarioState.CalculationId.Should().NotBeEmpty();
        }

        [ScenarioStep(3)]
        [SubsystemFact]
        public async Task Then_CalculationIsCompletedWithinWaitTime()
        {
            var actualWaitResult = await Fixture.WaitForCalculationStateAsync(
                Fixture.ScenarioState.CalculationId,
                waitForState: DomainTests.Clients.v3.BatchState.Completed,
                waitTimeLimit: TimeSpan.FromMinutes(21));

            Fixture.ScenarioState.Batch = actualWaitResult.Batch;

            // Assert
            using var assertionScope = new AssertionScope();
            actualWaitResult.IsState.Should().BeTrue();
            actualWaitResult.Batch.Should().NotBeNull();

            actualWaitResult.Batch!.ExecutionState.Should().Be(DomainTests.Clients.v3.BatchState.Completed);
        }

        [ScenarioStep(4)]
        [SubsystemFact]
        public void AndThen_CalculationDurationIsLessThanOrEqualToTimeLimit()
        {
            var calculationTimeLimit = TimeSpan.FromMinutes(18);
            var actualCalculationDuration =
                Fixture.ScenarioState.Batch!.ExecutionTimeEnd - Fixture.ScenarioState.Batch.ExecutionTimeStart;

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

            Fixture.ScenarioState.ReceivedCalculationResultCompleted =
                actualReceivedIntegrationEvents.OfType<CalculationResultCompleted>().ToList();
            Fixture.ScenarioState.ReceivedEnergyResultProducedV2 =
                actualReceivedIntegrationEvents.OfType<EnergyResultProducedV2>().ToList();
            Fixture.ScenarioState.ReceivedAmountPerChargeResultProducedV1 = actualReceivedIntegrationEvents
                .OfType<AmountPerChargeResultProducedV1>().ToList();
            Fixture.ScenarioState.ReceivedMonthlyAmountPerChargeResultProducedV1 = actualReceivedIntegrationEvents
                .OfType<MonthlyAmountPerChargeResultProducedV1>().ToList();

            // Assert
            using var assertionScope = new AssertionScope();
            // => Not empty
            Fixture.ScenarioState.ReceivedCalculationResultCompleted.Should().NotBeEmpty();
            Fixture.ScenarioState.ReceivedEnergyResultProducedV2.Should().NotBeEmpty();
            // => Empty
            Fixture.ScenarioState.ReceivedAmountPerChargeResultProducedV1.Should().BeEmpty();
            Fixture.ScenarioState.ReceivedMonthlyAmountPerChargeResultProducedV1.Should().BeEmpty();
        }

        [ScenarioStep(6)]
        [SubsystemFact]
        public void AndThen_ReceivedEnergyResultProducedEventsCountIsEqualToExpected()
        {
            var expected = 104;

            // Assert
            using var assertionScope = new AssertionScope();
            Fixture.ScenarioState.ReceivedCalculationResultCompleted.Count.Should().Be(expected);
            Fixture.ScenarioState.ReceivedEnergyResultProducedV2.Count.Should().Be(expected);
        }

        [ScenarioStep(7)]
        [SubsystemFact]
        public void AndThen_ReceivedEnergyResultProducedEventsContainAllTimeSeriesTypes()
        {
            var expected = Enum.GetNames(typeof(TimeSeriesType)).ToList();

            var actualTimeSeriesTypesForCalculationResultCompleted = Fixture.ScenarioState
                .ReceivedCalculationResultCompleted
                .Select(x => Enum.GetName(x.TimeSeriesType))
                .Distinct()
                .ToList();
            var actualTimeSeriesTypesForEnergyResultProducedV2 = Fixture.ScenarioState.ReceivedEnergyResultProducedV2
                .Select(x => Enum.GetName(x.TimeSeriesType))
                .Distinct()
                .ToList();

            // Assert
            using var assertionScope = new AssertionScope();
            foreach (var timeSeriesType in expected)
            {
                actualTimeSeriesTypesForCalculationResultCompleted.Should().Contain(timeSeriesType);
                actualTimeSeriesTypesForEnergyResultProducedV2.Should().Contain(timeSeriesType);
            }
        }

        [ScenarioStep(8)]
        [SubsystemFact]
        public void AndThen_ReceivedEnergyResultProducedEventsContainExpectedTuplesOfTimeSeriesTypeAndAggregationLevel()
        {
            IEnumerable<(string TimeSeriesType, string AggregationLevel)> expected =
                new List<(string, string)>
                {
                    ("NonProfiledConsumption", "AggregationPerGridarea"),
                    ("NonProfiledConsumption", "AggregationPerEnergysupplierPerGridarea"),
                    ("NonProfiledConsumption", "AggregationPerBalanceresponsiblepartyPerGridarea"),
                    ("NonProfiledConsumption", "AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea"),
                    ("Production", "AggregationPerGridarea"),
                    ("Production", "AggregationPerEnergysupplierPerGridarea"),
                    ("Production", "AggregationPerBalanceresponsiblepartyPerGridarea"),
                    ("Production", "AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea"),
                    ("FlexConsumption", "AggregationPerGridarea"),
                    ("FlexConsumption", "AggregationPerEnergysupplierPerGridarea"),
                    ("FlexConsumption", "AggregationPerBalanceresponsiblepartyPerGridarea"),
                    ("FlexConsumption", "AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea"),
                    ("NetExchangePerGa", "AggregationPerGridarea"),
                    ("NetExchangePerNeighboringGa", "AggregationPerGridarea"),
                    ("GridLoss", "AggregationPerGridarea"),
                    ("NegativeGridLoss", "AggregationPerGridarea"),
                    ("PositiveGridLoss", "AggregationPerGridarea"),
                    ("TotalConsumption", "AggregationPerGridarea"),
                    ("TempFlexConsumption", "AggregationPerGridarea"),
                    ("TempProduction", "AggregationPerGridarea"),
                };

            // Assert
            using var assertionScope = new AssertionScope();
            foreach (var tuple in expected)
            {
                Fixture.ScenarioState.ReceivedCalculationResultCompleted
                    .Should()
                    .Contain(item =>
                        Enum.GetName(item.TimeSeriesType) == tuple.TimeSeriesType
                        && Enum.GetName(item.AggregationLevelCase) == tuple.AggregationLevel);

                Fixture.ScenarioState.ReceivedEnergyResultProducedV2
                    .Should()
                    .Contain(item =>
                        Enum.GetName(item.TimeSeriesType) == tuple.TimeSeriesType
                        && Enum.GetName(item.AggregationLevelCase) == tuple.AggregationLevel);
            }
        }
    }
}
