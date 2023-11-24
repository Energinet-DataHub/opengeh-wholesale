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

using System.Globalization;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.DomainTests.Features.Calculations.Fixtures;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Attributes;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.LazyFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.DomainTests.Features.Calculations
{
    [TestCaseOrderer(
        ordererTypeName: "Energinet.DataHub.Wholesale.DomainTests.Fixtures.Orderers.ScenarioStepOrderer",
        ordererAssemblyName: "Energinet.DataHub.Wholesale.DomainTests")]
    public class WholesaleFixingCalculationScenario : DomainTestsBase<CalculationScenarioFixture>
    {
        public WholesaleFixingCalculationScenario(LazyFixtureFactory<CalculationScenarioFixture> lazyFixtureFactory)
            : base(lazyFixtureFactory)
        {
        }

        [ScenarioStep(0)]
        [DomainFact]
        public void Given_CalculationInput()
        {
            Fixture.ScenarioState.CalculationInput = new Clients.v3.BatchRequestDto
            {
                ProcessType = Clients.v3.ProcessType.WholesaleFixing,
                GridAreaCodes = new List<string> { "804" },
                StartDate = new DateTimeOffset(2023, 1, 31, 23, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2023, 2, 28, 23, 0, 0, TimeSpan.Zero),
            };
        }

        [ScenarioStep(1)]
        [DomainFact]
        public void AndGiven_SubscribedIntegrationEvents()
        {
            Fixture.ScenarioState.SubscribedIntegrationEventNames.Add(CalculationResultCompleted.EventName);
            Fixture.ScenarioState.SubscribedIntegrationEventNames.Add(EnergyResultProducedV2.EventName);
            Fixture.ScenarioState.SubscribedIntegrationEventNames.Add(AmountPerChargeResultProducedV1.EventName);
            Fixture.ScenarioState.SubscribedIntegrationEventNames.Add(MonthlyAmountPerChargeResultProducedV1.EventName);
        }

        [ScenarioStep(2)]
        [DomainFact]
        public async Task When_CalculationIsStarted()
        {
            Fixture.ScenarioState.CalculationId =
                await Fixture.StartCalculationAsync(Fixture.ScenarioState.CalculationInput);

            // Assert
            Fixture.ScenarioState.CalculationId.Should().NotBeEmpty();
        }

        [ScenarioStep(3)]
        [DomainFact]
        public async Task Then_CalculationIsCompletedWithinWaitTime()
        {
            var actualWaitResult = await Fixture.WaitForCalculationStateAsync(
                Fixture.ScenarioState.CalculationId,
                waitForState: Clients.v3.BatchState.Completed,
                waitTimeLimit: TimeSpan.FromMinutes(28));

            Fixture.ScenarioState.Batch = actualWaitResult.Batch;

            // Assert
            using var assertionScope = new AssertionScope();
            actualWaitResult.IsState.Should().BeTrue();
            actualWaitResult.Batch.Should().NotBeNull();

            actualWaitResult.Batch!.ExecutionState.Should().Be(Clients.v3.BatchState.Completed);
        }

        [ScenarioStep(4)]
        [DomainFact]
        public void AndThen_CalculationDurationIsLessThanOrEqualToTimeLimit()
        {
            var calculationTimeLimit = TimeSpan.FromMinutes(25);
            var actualCalculationDuration =
                Fixture.ScenarioState.Batch!.ExecutionTimeEnd - Fixture.ScenarioState.Batch.ExecutionTimeStart;

            // Assert
            actualCalculationDuration.Should().BeGreaterThan(TimeSpan.Zero);
            actualCalculationDuration.Should().BeLessThanOrEqualTo(calculationTimeLimit);
        }

        [ScenarioStep(5)]
        [DomainFact]
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
            Fixture.ScenarioState.ReceivedCalculationResultCompleted.Should().NotBeEmpty();
            Fixture.ScenarioState.ReceivedEnergyResultProducedV2.Should().NotBeEmpty();
            Fixture.ScenarioState.ReceivedAmountPerChargeResultProducedV1.Should().NotBeEmpty();
            Fixture.ScenarioState.ReceivedMonthlyAmountPerChargeResultProducedV1.Should().NotBeEmpty();
        }

        [ScenarioStep(6)]
        [DomainFact]
        public void AndThen_ReceivedEnergyResultProducedEventsCountIsEqualToExpected()
        {
            var expected = 157;

            // Assert
            using var assertionScope = new AssertionScope();
            Fixture.ScenarioState.ReceivedCalculationResultCompleted.Count.Should().Be(expected);
            Fixture.ScenarioState.ReceivedEnergyResultProducedV2.Count.Should().Be(expected);
        }

        [ScenarioStep(7)]
        [DomainFact]
        public void AndThen_ReceivedEnergyResultProducedEventsContainExpectedTimeSeriesTypes()
        {
            var expected = Enum
                .GetNames(typeof(TimeSeriesType))
                .Where(s => s != nameof(TimeSeriesType.NetExchangePerNeighboringGa))
                .ToList();

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
        [DomainFact]
        public void AndThen_ReceivedEnergyResultProducedEventsContainExpectedTuplesOfTimeSeriesTypeAndAggregationLevel()
        {
            IEnumerable<(string TimeSeriesType, string AggregationLevel)> expected =
                new List<(string, string)>
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

        [ScenarioStep(9)]
        [DomainFact]
        public void AndThen_ReceivedAmountPerChargeResultProducedEventsCountIsEqualToExpected()
        {
            var expected = 28;

            // Assert
            Fixture.ScenarioState.ReceivedAmountPerChargeResultProducedV1.Count.Should().Be(expected);
        }

        [ScenarioStep(10)]
        [DomainFact]
        public void AndThen_ReceivedMonthlyAmountPerChargeResultProducedEventsCountIsEqualToExpected()
        {
            var expected = 16;

            // Assert
            Fixture.ScenarioState.ReceivedMonthlyAmountPerChargeResultProducedV1.Count.Should().Be(expected);
        }

        /// <summary>
        /// Notice we don't verify 'TimeSeriesPoints.QuantityQualities' in this scenario.
        /// </summary>
        [ScenarioStep(11)]
        [DomainFact]
        public async Task AndThen_OneSpecificAmountPerChargeResultProducedEventContainsExpectedTimeSeriesPoints()
        {
            var expectedEnergySupplierId = "5790001687137";
            var expectedChargeCode = "40000";
            var expectedChargeType = AmountPerChargeResultProducedV1.Types.ChargeType.Tariff;
            var expectedChargeOwnerId = "5790001330552";
            var expectedSettlementMethod = AmountPerChargeResultProducedV1.Types.SettlementMethod.NonProfiled;
            var expectedTimeSeriesPoints = await Fixture.ParseTimeSeriesPointsFromCsvAsync("amount_for_es_for_hourly_tarif_40000_for_e17_e02.csv");

            // Assert
            var actualEvents = Fixture.ScenarioState.ReceivedAmountPerChargeResultProducedV1.Where(item =>
                item.EnergySupplierId == expectedEnergySupplierId
                && item.ChargeCode == expectedChargeCode
                && item.ChargeType == expectedChargeType
                && item.ChargeOwnerId == expectedChargeOwnerId
                && item.SettlementMethod == expectedSettlementMethod);

            using var assertionScope = new AssertionScope();
            actualEvents.Should().HaveCount(1);

            var actualEvent = actualEvents.First();
            actualEvent.TimeSeriesPoints.Should().HaveCount(expectedTimeSeriesPoints.Count);

            // We clear incomming 'QuantityQualities' before comparing with test data, because we don't have them in our test data file.
            actualEvent.TimeSeriesPoints
                .Select(item =>
                {
                    item.QuantityQualities.Clear();
                    return item;
                })
                .Should().BeEquivalentTo(expectedTimeSeriesPoints);
        }

        [ScenarioStep(12)]
        [DomainFact]
        public void AndThen_OneSpecificMonthlyAmountPerChargeResultProducedEventContainsExpectedMonthlyAmount()
        {
            var expectedEnergySupplierId = "5790001687137";
            var expectedChargeCode = "40000";
            var expectedChargeType = MonthlyAmountPerChargeResultProducedV1.Types.ChargeType.Tariff;
            var expectedChargeOwnerId = "5790001330552";
            var expectedAmount = new Contracts.IntegrationEvents.Common.DecimalValue(decimal.Parse("95738.23956", CultureInfo.InvariantCulture));

            // Assert
            var actualEvents = Fixture.ScenarioState.ReceivedMonthlyAmountPerChargeResultProducedV1.Where(item =>
                item.EnergySupplierId == expectedEnergySupplierId
                && item.ChargeCode == expectedChargeCode
                && item.ChargeType == expectedChargeType
                && item.ChargeOwnerId == expectedChargeOwnerId
                && object.Equals(item.Amount, expectedAmount));

            using var assertionScope = new AssertionScope();
            actualEvents.Should().HaveCount(1);
        }
    }
}
