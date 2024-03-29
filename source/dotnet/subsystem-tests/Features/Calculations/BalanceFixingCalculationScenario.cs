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

using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations.Fixtures;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Attributes;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations;

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
        Fixture.ScenarioState.CalculationInput = new Clients.v3.CalculationRequestDto
        {
            CalculationType = Clients.v3.CalculationType.BalanceFixing,
            GridAreaCodes = new List<string> { "543" },
            StartDate = new DateTimeOffset(2022, 1, 11, 23, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2022, 1, 12, 23, 0, 0, TimeSpan.Zero),
        };
    }

    [ScenarioStep(1)]
    [SubsystemFact]
    public void AndGiven_SubscribedIntegrationEvents()
    {
        Fixture.ScenarioState.SubscribedIntegrationEventNames.Add(EnergyResultProducedV2.EventName);
        Fixture.ScenarioState.SubscribedIntegrationEventNames.Add(GridLossResultProducedV1.EventName);
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
        var (isCompletedOrFailed, calculation) = await Fixture.WaitForCalculationCompletedOrFailedAsync(
            Fixture.ScenarioState.CalculationId,
            waitTimeLimit: TimeSpan.FromMinutes(21));

        Fixture.ScenarioState.Calculation = calculation;

        // Assert
        using var assertionScope = new AssertionScope();
        isCompletedOrFailed.Should().BeTrue();
        calculation.Should().NotBeNull();

        calculation!.ExecutionState.Should().Be(Clients.v3.CalculationState.Completed);
    }

    [ScenarioStep(4)]
    [SubsystemFact]
    public void AndThen_CalculationDurationIsLessThanOrEqualToTimeLimit()
    {
        var calculationTimeLimit = TimeSpan.FromMinutes(18);
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
        // Skip waiting if calculation did not complete
        if (Fixture.ScenarioState.Calculation != null
            && Fixture.ScenarioState.Calculation.ExecutionState == Clients.v3.CalculationState.Completed)
        {
            var actualReceivedIntegrationEvents = await Fixture.WaitForIntegrationEventsAsync(
                Fixture.ScenarioState.CalculationId,
                Fixture.ScenarioState.SubscribedIntegrationEventNames.AsReadOnly(),
                waitTimeLimit: TimeSpan.FromMinutes(8));

            Fixture.ScenarioState.ReceivedEnergyResultProducedV2 =
                actualReceivedIntegrationEvents.OfType<EnergyResultProducedV2>().ToList();
            Fixture.ScenarioState.ReceivedGridLossProducedV1 =
                actualReceivedIntegrationEvents.OfType<GridLossResultProducedV1>().ToList();
            Fixture.ScenarioState.ReceivedAmountPerChargeResultProducedV1 = actualReceivedIntegrationEvents
                .OfType<AmountPerChargeResultProducedV1>().ToList();
            Fixture.ScenarioState.ReceivedMonthlyAmountPerChargeResultProducedV1 = actualReceivedIntegrationEvents
                .OfType<MonthlyAmountPerChargeResultProducedV1>().ToList();
        }

        // Assert
        using var assertionScope = new AssertionScope();
        // => Not empty
        Fixture.ScenarioState.ReceivedEnergyResultProducedV2.Should().NotBeEmpty();
        Fixture.ScenarioState.ReceivedGridLossProducedV1.Should().NotBeEmpty();
        // => Empty
        Fixture.ScenarioState.ReceivedAmountPerChargeResultProducedV1.Should().BeEmpty();
        Fixture.ScenarioState.ReceivedMonthlyAmountPerChargeResultProducedV1.Should().BeEmpty();
    }

    [ScenarioStep(6)]
    [SubsystemFact]
    public void AndThen_ReceivedEnergyResultProducedEventsCountIsEqualToExpected()
    {
        var expected = 100;

        // Assert
        using var assertionScope = new AssertionScope();
        Fixture.ScenarioState.ReceivedEnergyResultProducedV2.Count.Should().Be(expected);
        Fixture.ScenarioState.ReceivedGridLossProducedV1.Count.Should().Be(2);
    }

    [ScenarioStep(7)]
    [SubsystemFact]
    public void AndThen_ReceivedEnergyResultProducedEventsContainAllTimeSeriesTypes()
    {
        List<string> expected =
        [
            "Production",
            "FlexConsumption",
            "TotalConsumption",
            "NonProfiledConsumption",
            "NetExchangePerGa",
            "NetExchangePerNeighboringGa"
        ];

        var actualTimeSeriesTypesForEnergyResultProducedV2 = Fixture.ScenarioState.ReceivedEnergyResultProducedV2
            .Select(x => Enum.GetName(x.TimeSeriesType))
            .Distinct()
            .ToList();

        // Assert
        using var assertionScope = new AssertionScope();
        foreach (var timeSeriesType in expected)
        {
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
                ("TotalConsumption", "AggregationPerGridarea"),
            };

        // Assert
        using var assertionScope = new AssertionScope();
        foreach (var tuple in expected)
        {
            Fixture.ScenarioState.ReceivedEnergyResultProducedV2
                .Should()
                .Contain(item =>
                    Enum.GetName(item.TimeSeriesType) == tuple.TimeSeriesType
                    && Enum.GetName(item.AggregationLevelCase) == tuple.AggregationLevel);
        }
    }

    [ScenarioStep(9)]
    [SubsystemFact]
    public void AndThen_OneSpecificEnergyResultProducedEventContainsVersion()
    {
        // Assert
        var actualVersion = Fixture.ScenarioState.ReceivedEnergyResultProducedV2.First().CalculationResultVersion;

        using var assertionScope = new AssertionScope();
        actualVersion.Should().BeGreaterThan(0);

        // Convert version (ticks) to datetime and assert that it is not older than 3 hours
        new DateTime(actualVersion).Subtract(DateTime.Now).Hours.Should().BeLessThan(3);
    }

    [ScenarioStep(10)]
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

    [ScenarioStep(11)]
    [SubsystemFact]
    public async Task AndThen_ReceivedEnergyResultProducedV2EventContainsExpectedTimeSeriesPoints()
    {
        // Arrange
        var expectedTimeSeriesPoints = await Fixture.ParseEnergyResultTimeSeriesPointsFromCsvAsync("Non_profiled_consumption_es_brp_ga_GA_543 for 5790001102357.csv");

        var energyResults = Fixture.ScenarioState.ReceivedEnergyResultProducedV2
            .Where(x => x.TimeSeriesType == EnergyResultProducedV2.Types.TimeSeriesType.NonProfiledConsumption)
            .Where(x => x.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea != null)
            .Where(x => x.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.EnergySupplierId == "5790001102357")
            .Where(x => x.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.GridAreaCode == "543")
            .ToList();

        // Assert
        Assert.Single(energyResults);
        energyResults.First().TimeSeriesPoints.Should().BeEquivalentTo(expectedTimeSeriesPoints);
    }

    [ScenarioStep(12)]
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
}
