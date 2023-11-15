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
using Energinet.DataHub.Wholesale.DomainTests.Fixtures;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Attributes;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.LazyFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.DomainTests
{
    /// <summary>
    /// Contains tests with focus on verifying calculations initiated and monitored using the Web API running in a live environment.
    /// </summary>
    public class CalculationFeatureTests
    {
        /// <summary>
        /// These tests uses an authorized Wholesale client to perform requests.
        /// </summary>
        public class Given_CalculationCompleted : DomainTestsBase<CalculationFixture>
        {
            public Given_CalculationCompleted(LazyFixtureFactory<CalculationFixture> lazyFixtureFactory)
                : base(lazyFixtureFactory)
            {
            }

            [DomainFact]
            public void WhenCreatingEnergyCalculationBatch_BatchIsEventuallyCompleted()
            {
                Fixture.Output.BalanceFixingCalculationIsComplete.Should().BeTrue();
            }

            [DomainFact]
            public void WhenCreatingWholesaleCalculationBatch_BatchIsEventuallyCompleted()
            {
                Fixture.Output.WholesaleFixingCalculationIsComplete.Should().BeTrue();
            }

            [DomainFact]
            public void WhenBalanceFixingHasCompleted_HasReceivedExpectedNumberOfResults()
            {
                Fixture.Output.CalculationResultCompletedFromBalanceFixing.Count.Should().Be(112);
                Fixture.Output.EnergyResultProducedFromBalanceFixing.Count.Should().Be(112);
            }

            [DomainFact]
            public void WhenWholesaleFixingHasCompleted_HasReceivedExpectedNumberOfEnergyResults()
            {
                Fixture.Output.CalculationResultCompletedFromWholesaleFixing.Count.Should().Be(137);
                Fixture.Output.EnergyResultProducedFromWholesaleFixing.Count.Should().Be(137);
            }

            [DomainFact]
            public void WhenWholesaleFixingHasCompleted_HasReceivedExpectedNumberOfAmountPerChargeResults()
            {
                Fixture.Output.AmountPerChargeResultProduced.Count.Should().Be(28);
            }

            [DomainFact]
            public void WhenWholesaleFixingHasCompleted_HasReceivedExpectedNumberOfMonthlyAmountPerChargeResults()
            {
                Fixture.Output.MonthlyAmountPerChargeResultProduced.Count.Should().Be(16);
            }

            [DomainFact]
            public void WhenEnergyCalculationBatchIsComplete_MessagesReceivedContainAllTimeSeriesTypes()
            {
                var actualForCalculationResultCompleted = GetTimeSeriesTypes(Fixture.Output.CalculationResultCompletedFromBalanceFixing);
                var actualForEnergyResultProduced = GetTimeSeriesTypes(Fixture.Output.EnergyResultProducedFromBalanceFixing);
                foreach (var expectedTimeSeriesType in ExpectedTimeSeriesTypesForBalanceFixing)
                {
                    actualForCalculationResultCompleted.Should().Contain(expectedTimeSeriesType);
                    actualForEnergyResultProduced.Should().Contain(expectedTimeSeriesType);
                }
            }

            [DomainFact]
            public void WhenWholesaleCalculationBatchIsComplete_MessagesReceivedContainAllTimeSeriesTypes()
            {
                var actualForCalculationResultCompleted = GetTimeSeriesTypes(Fixture.Output.CalculationResultCompletedFromWholesaleFixing);
                var actualForEnergyResultProduced = GetTimeSeriesTypes(Fixture.Output.EnergyResultProducedFromWholesaleFixing);
                foreach (var expectedTimeSeriesType in ExpectedTimeSeriesTypesForWholesaleFixing)
                {
                    actualForCalculationResultCompleted.Should().Contain(expectedTimeSeriesType);
                    actualForEnergyResultProduced.Should().Contain(expectedTimeSeriesType);
                }
            }

            [DomainFact]
            public void WhenBalanceFixingBatchMessagesReceived_ContainsExpectedResultTypes()
            {
                using (new AssertionScope())
                {
                    foreach (var (timeSeriesType, aggregationLevel) in
                             ExpectedTimeSeriesTypeAndAggregationLevelForBalanceFixing())
                    {
                        CheckIfExistsInCalculationResults(
                            Fixture.Output.CalculationResultCompletedFromBalanceFixing,
                            timeSeriesType,
                            aggregationLevel).Should().BeTrue();
                        CheckIfExistsInCalculationResults(
                            Fixture.Output.EnergyResultProducedFromBalanceFixing,
                            timeSeriesType,
                            aggregationLevel).Should().BeTrue();
                    }
                }
            }

            [DomainFact]
            public void WhenWholesaleFixingBatchIsReceivedOnTopicSubscription_MessagesReceivedContainExpectedResultTypes()
            {
                using (new AssertionScope())
                {
                    foreach (var (timeSeriesType, aggregationLevel) in ExpectedTimeSeriesTypeAndAggregationLevelForWholesaleFixing())
                    {
                        CheckIfExistsInCalculationResults(Fixture.Output.CalculationResultCompletedFromWholesaleFixing, timeSeriesType, aggregationLevel).Should().BeTrue();
                        CheckIfExistsInCalculationResults(Fixture.Output.EnergyResultProducedFromWholesaleFixing, timeSeriesType, aggregationLevel).Should().BeTrue();
                    }
                }
            }

            private List<string> ExpectedTimeSeriesTypesForBalanceFixing { get; } = Enum.GetNames(typeof(TimeSeriesType)).ToList();

            private List<string> ExpectedTimeSeriesTypesForWholesaleFixing
                => ExpectedTimeSeriesTypesForBalanceFixing.Where(s => s != nameof(TimeSeriesType.NetExchangePerNeighboringGa)).ToList();

            private List<string?> GetTimeSeriesTypes(List<CalculationResultCompleted> calculationResults)
            {
                return calculationResults.Select(o => Enum.GetName(o.TimeSeriesType)).Distinct().ToList();
            }

            private List<string?> GetTimeSeriesTypes(List<EnergyResultProducedV2> calculationResults)
            {
                return calculationResults.Select(o => Enum.GetName(o.TimeSeriesType)).Distinct().ToList();
            }

            private bool CheckIfExistsInCalculationResults(
                List<CalculationResultCompleted>? calculationResults,
                string timeSeriesType,
                string aggregationLevel)
            {
                ArgumentNullException.ThrowIfNull(calculationResults);

                return calculationResults.Any(
                    obj => Enum.GetName(obj.TimeSeriesType) == timeSeriesType
                           && Enum.GetName(obj.AggregationLevelCase) == aggregationLevel);
            }

            private bool CheckIfExistsInCalculationResults(
                List<EnergyResultProducedV2>? calculationResults,
                string timeSeriesType,
                string aggregationLevel)
            {
                ArgumentNullException.ThrowIfNull(calculationResults);

                return calculationResults.Any(
                    obj => Enum.GetName(obj.TimeSeriesType) == timeSeriesType
                           && Enum.GetName(obj.AggregationLevelCase) == aggregationLevel);
            }

            private static List<(string TimeSeriesType, string AggregationLevel)> ExpectedTimeSeriesTypeAndAggregationLevelForBalanceFixing()
            {
                return new List<(string, string)>
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
            }

            private static List<(string TimeSeriesType, string AggregationLevel)> ExpectedTimeSeriesTypeAndAggregationLevelForWholesaleFixing()
            {
                return new List<(string, string)>
                {
                    ("NonProfiledConsumption", "AggregationPerGridarea"),
                    ("NonProfiledConsumption", "AggregationPerEnergysupplierPerGridarea"),
                    ("Production", "AggregationPerGridarea"),
                    ("Production", "AggregationPerEnergysupplierPerGridarea"),
                    ("FlexConsumption", "AggregationPerGridarea"),
                    ("FlexConsumption", "AggregationPerEnergysupplierPerGridarea"),
                    ("NetExchangePerGa", "AggregationPerGridarea"),
                    ("GridLoss", "AggregationPerGridarea"),
                    ("NegativeGridLoss", "AggregationPerGridarea"),
                    ("PositiveGridLoss", "AggregationPerGridarea"),
                    ("TotalConsumption", "AggregationPerGridarea"),
                    ("TempFlexConsumption", "AggregationPerGridarea"),
                    ("TempProduction", "AggregationPerGridarea"),
                };
            }
        }

        [TestCaseOrderer(
            ordererTypeName: "Energinet.DataHub.Wholesale.DomainTests.Fixtures.Orderers.PriorityOrderer",
            ordererAssemblyName: "Energinet.DataHub.Wholesale.DomainTests")]
        public class BalanceFixingCalculationScenario : DomainTestsBase<CalculationScenarioFixture>
        {
            public BalanceFixingCalculationScenario(LazyFixtureFactory<CalculationScenarioFixture> lazyFixtureFactory)
                : base(lazyFixtureFactory)
            {
            }

            [Priority(0)]
            [DomainFact]
            public void Given_CalculationInput()
            {
                var startDate = new DateTimeOffset(2022, 1, 11, 23, 0, 0, TimeSpan.Zero);
                var endDate = new DateTimeOffset(2022, 1, 12, 23, 0, 0, TimeSpan.Zero);
                var batchRequestDto = new Clients.v3.BatchRequestDto
                {
                    ProcessType = Clients.v3.ProcessType.BalanceFixing,
                    GridAreaCodes = new List<string> { "543" },
                    StartDate = startDate,
                    EndDate = endDate,
                };

                Fixture.ScenarioState.CalculationInput = batchRequestDto;
            }

            [Priority(1)]
            [DomainFact]
            public void AndGiven_SubscribedIntegrationEvents()
            {
                Fixture.ScenarioState.SubscribedIntegrationEventNames.Add(CalculationResultCompleted.EventName);
                Fixture.ScenarioState.SubscribedIntegrationEventNames.Add(EnergyResultProducedV2.EventName);
            }

            [Priority(2)]
            [DomainFact]
            public async Task When_CalculationIsStarted()
            {
                Fixture.ScenarioState.CalculationId = await Fixture.StartCalculationAsync(Fixture.ScenarioState.CalculationInput);

                // Assert
                Fixture.ScenarioState.CalculationId.Should().NotBeEmpty();
            }

            [Priority(3)]
            [DomainFact]
            public async Task Then_CalculationIsCompletedWithinWaitTime()
            {
                var actualWaitResult = await Fixture.WaitForCalculationStateAsync(
                    Fixture.ScenarioState.CalculationId,
                    waitForState: Clients.v3.BatchState.Completed,
                    waitTimeLimit: TimeSpan.FromMinutes(20));

                Fixture.ScenarioState.Batch = actualWaitResult.Batch;

                // Assert
                using var assertionScope = new AssertionScope();
                actualWaitResult.IsState.Should().BeTrue();
                actualWaitResult.Batch.Should().NotBeNull();

                actualWaitResult.Batch!.ExecutionState.Should().Be(Clients.v3.BatchState.Completed);
            }

            [Priority(4)]
            [DomainFact]
            public void AndThen_CalculationDurationIsLessThanOrEqualToTimeLimit()
            {
                var calculationTimeLimit = TimeSpan.FromMinutes(13);
                var actualCalculationDuration =
                    Fixture.ScenarioState.Batch!.ExecutionTimeEnd - Fixture.ScenarioState.Batch.ExecutionTimeStart;

                // Assert
                actualCalculationDuration.Should().BeGreaterThan(TimeSpan.Zero);
                actualCalculationDuration.Should().BeLessThanOrEqualTo(calculationTimeLimit);
            }

            [Priority(5)]
            [DomainFact]
            public async Task AndThen_IntegrationEventsAreReceivedWithinWaitTime()
            {
                var actualReceivedIntegrationEvents = await Fixture.WaitForIntegrationEventsAsync(
                    Fixture.ScenarioState.CalculationId,
                    Fixture.ScenarioState.SubscribedIntegrationEventNames.AsReadOnly(),
                    waitTimeLimit: TimeSpan.FromMinutes(8));

                Fixture.ScenarioState.ReceivedCalculationResultCompleted = actualReceivedIntegrationEvents.OfType<CalculationResultCompleted>().ToList();
                Fixture.ScenarioState.ReceivedEnergyResultProducedV2 = actualReceivedIntegrationEvents.OfType<EnergyResultProducedV2>().ToList();

                // Assert
                using var assertionScope = new AssertionScope();
                Fixture.ScenarioState.ReceivedCalculationResultCompleted.Should().NotBeEmpty();
                Fixture.ScenarioState.ReceivedEnergyResultProducedV2.Should().NotBeEmpty();
            }

            [Priority(6)]
            [DomainFact]
            public void AndThen_ReceivedIntegrationEventsCountIsEqualToExpected()
            {
                var expectedIntegrationEventsPerTypeCount = 112;

                // Assert
                using var assertionScope = new AssertionScope();
                Fixture.ScenarioState.ReceivedCalculationResultCompleted.Count.Should().Be(expectedIntegrationEventsPerTypeCount);
                Fixture.ScenarioState.ReceivedEnergyResultProducedV2.Count.Should().Be(expectedIntegrationEventsPerTypeCount);
            }

            [Priority(7)]
            [DomainFact]
            public void AndThen_ReceivedIntegrationEventsContainAllTimeSeriesTypes()
            {
                var expectedTimeSeriesTypes = Enum.GetNames(typeof(TimeSeriesType)).ToList();

                var actualTimeSeriesTypesForCalculationResultCompleted = Fixture.ScenarioState.ReceivedCalculationResultCompleted
                    .Select(x => Enum.GetName(x.TimeSeriesType))
                    .Distinct()
                    .ToList();
                var actualTimeSeriesTypesForEnergyResultProducedV2 = Fixture.ScenarioState.ReceivedEnergyResultProducedV2
                    .Select(x => Enum.GetName(x.TimeSeriesType))
                    .Distinct()
                    .ToList();

                // Assert
                using var assertionScope = new AssertionScope();
                foreach (var timeSeriesType in expectedTimeSeriesTypes)
                {
                    actualTimeSeriesTypesForCalculationResultCompleted.Should().Contain(timeSeriesType);
                    actualTimeSeriesTypesForEnergyResultProducedV2.Should().Contain(timeSeriesType);
                }
            }

            [Priority(8)]
            [DomainFact]
            public void AndThen_ReceivedIntegrationEventsContainExpectedTuplesOfTimeSeriesTypeAndAggregationLevel()
            {
                IEnumerable<(string TimeSeriesType, string AggregationLevel)> expectedTuplesOfTimeSeriesTypeAndAggregationLevel =
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
                foreach (var tuple in expectedTuplesOfTimeSeriesTypeAndAggregationLevel)
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
}
