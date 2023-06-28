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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Types;
using Energinet.DataHub.Wholesale.Events.UnitTests.Fixtures;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Xunit;
using QuantityQuality =
    Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;
using TimeSeriesPoint =
    Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesPoint;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Factories;

public class CalculationResultCompletedFactoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public void CreateCalculationResultCompletedForGridArea_WhenCreating_ResultIsForTotalGridArea(
        CalculationResult anyCalculationResult,
        CalculationResultCompletedFactory sut)
    {
        // Act
        var actual = sut.CreateForGridArea(anyCalculationResult);

        // Assert
        actual.AggregationPerBalanceresponsiblepartyPerGridarea.Should().BeNull();
        actual.AggregationPerEnergysupplierPerGridarea.Should().BeNull();
        actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.Should().BeNull();
        actual.AggregationPerGridarea.Should().NotBeNull();
    }

    [Theory]
    [InlineAutoMoqData]
    public void CreateCalculationResultCompletedForGridArea_WhenCreating_PropertiesAreMappedCorrectly(
        CalculationResultBuilder calculationResultBuilder,
        CalculationResultCompletedFactory sut)
    {
        // Arrange
        var timeSeriesPoint = new TimeSeriesPoint(DateTimeOffset.Now, 10.101000000m, QuantityQuality.Estimated);
        var calculationResult = calculationResultBuilder.WithTimeSeriesPoints(new[] { timeSeriesPoint }).Build();

        // Act
        var actual = sut.CreateForGridArea(calculationResult);

        // Assert
        actual.AggregationPerGridarea.GridAreaCode.Should().Be(calculationResult.GridArea);
        actual.BatchId.Should().Be(calculationResult.BatchId.ToString());
        actual.Resolution.Should().Be(Resolution.Quarter);
        actual.QuantityUnit.Should().Be(QuantityUnit.Kwh);
        actual.FromGridAreaCode.Should().Be(calculationResult.FromGridArea);
        actual.PeriodEndUtc.Should().Be(calculationResult.PeriodEnd.ToTimestamp());
        actual.PeriodStartUtc.Should().Be(calculationResult.PeriodStart.ToTimestamp());
        actual.TimeSeriesType.Should().Be(TimeSeriesTypeMapper.MapTimeSeriesType(calculationResult.TimeSeriesType));

        actual.TimeSeriesPoints[0].Quantity.Units.Should().Be(10);
        actual.TimeSeriesPoints[0].Quantity.Nanos.Should().Be(101000000);
        actual.TimeSeriesPoints[0].Time.Should().Be(timeSeriesPoint.Time.ToTimestamp());
        actual.TimeSeriesPoints[0].QuantityQuality.Should()
            .Be(QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality));
    }

    [Theory]
    [InlineAutoMoqData]
    public void CreateCalculationResultCompletedForEnergySupplier_WhenCreating_ResultIsForTotalEnergySupplier(
        CalculationResult anyCalculationResult,
        CalculationResultCompletedFactory sut)
    {
        // Act
        var actual = sut.CreateForEnergySupplier(
            anyCalculationResult,
            "AGlnNumber");

        // Assert
        actual.AggregationPerBalanceresponsiblepartyPerGridarea.Should().BeNull();
        actual.AggregationPerGridarea.Should().BeNull();
        actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.Should().BeNull();
        actual.AggregationPerEnergysupplierPerGridarea.Should().NotBeNull();
    }

    [Theory]
    [InlineAutoMoqData]
    public void CreateCalculationResultCompletedForEnergySupplier_WhenCreating_PropertiesAreMappedCorrectly(
        CalculationResultBuilder calculationResultBuilder,
        CalculationResultCompletedFactory sut)
    {
        // Arrange
        var timeSeriesPoint = new TimeSeriesPoint(DateTimeOffset.Now, 10.101000000m, QuantityQuality.Estimated);
        var calculationResult = calculationResultBuilder.WithTimeSeriesPoints(new[] { timeSeriesPoint }).Build();
        var expectedGln = "TheGln";

        // Act
        var actual = sut.CreateForEnergySupplier(calculationResult, expectedGln);

        // Assert
        actual.AggregationPerEnergysupplierPerGridarea.GridAreaCode.Should().Be(calculationResult.GridArea);
        actual.AggregationPerEnergysupplierPerGridarea.EnergySupplierGlnOrEic.Should().Be(expectedGln);
        actual.BatchId.Should().Be(calculationResult.BatchId.ToString());
        actual.Resolution.Should().Be(Resolution.Quarter);
        actual.QuantityUnit.Should().Be(QuantityUnit.Kwh);
        actual.PeriodEndUtc.Should().Be(calculationResult.PeriodEnd.ToTimestamp());
        actual.PeriodStartUtc.Should().Be(calculationResult.PeriodStart.ToTimestamp());
        actual.TimeSeriesType.Should().Be(TimeSeriesTypeMapper.MapTimeSeriesType(calculationResult.TimeSeriesType));

        actual.TimeSeriesPoints[0].Quantity.Units.Should().Be(10);
        actual.TimeSeriesPoints[0].Quantity.Nanos.Should().Be(101000000);
        actual.TimeSeriesPoints[0].Time.Should().Be(timeSeriesPoint.Time.ToTimestamp());
        actual.TimeSeriesPoints[0].QuantityQuality.Should()
            .Be(QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality));
    }

    [Theory]
    [InlineAutoMoqData]
    public void CreateCalculationResultCompletedForBalanceResponsibleParty_ReturnsResultForBalanceResponsibleParty(
        CalculationResult anyCalculationResult,
        CalculationResultCompletedFactory sut)
    {
        // Act
        var actual = sut.CreateForBalanceResponsibleParty(anyCalculationResult, "ABrpGlnNumber");

        // Assert
        actual.AggregationPerGridarea.Should().BeNull();
        actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.Should().BeNull();
        actual.AggregationPerEnergysupplierPerGridarea.Should().BeNull();
        actual.AggregationPerBalanceresponsiblepartyPerGridarea.Should().NotBeNull();
    }

    [Theory]
    [InlineAutoMoqData]
    public void CreateCalculationResultCompletedForBalanceResponsibleParty_WhenCreating_PropertiesAreMappedCorrectly(
        CalculationResultBuilder calculationResultBuilder,
        CalculationResultCompletedFactory sut)
    {
        // Arrange
        var timeSeriesPoint = new TimeSeriesPoint(DateTimeOffset.Now, 10.101000000m, QuantityQuality.Estimated);
        var calculationResult = calculationResultBuilder.WithTimeSeriesPoints(new[] { timeSeriesPoint }).Build();
        var expectedGln = "TheBrpGln";

        // Act
        var actual = sut.CreateForBalanceResponsibleParty(calculationResult, expectedGln);

        // Assert
        actual.AggregationPerBalanceresponsiblepartyPerGridarea.GridAreaCode.Should()
            .Be(calculationResult.GridArea);
        actual.AggregationPerBalanceresponsiblepartyPerGridarea.BalanceResponsiblePartyGlnOrEic.Should()
            .Be(expectedGln);
        actual.BatchId.Should().Be(calculationResult.BatchId.ToString());
        actual.Resolution.Should().Be(Resolution.Quarter);
        actual.QuantityUnit.Should().Be(QuantityUnit.Kwh);
        actual.PeriodEndUtc.Should().Be(calculationResult.PeriodEnd.ToTimestamp());
        actual.PeriodStartUtc.Should().Be(calculationResult.PeriodStart.ToTimestamp());
        actual.TimeSeriesType.Should().Be(TimeSeriesTypeMapper.MapTimeSeriesType(calculationResult.TimeSeriesType));

        actual.TimeSeriesPoints[0].Quantity.Units.Should().Be(10);
        actual.TimeSeriesPoints[0].Quantity.Nanos.Should().Be(101000000);
        actual.TimeSeriesPoints[0].Time.Should().Be(timeSeriesPoint.Time.ToTimestamp());
        actual.TimeSeriesPoints[0].QuantityQuality.Should()
            .Be(QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality));
    }

    [Theory]
    [InlineAutoMoqData]
    public void CreateCalculationResultCompletedForEnergySupplierByBalanceResponsibleParty_ReturnsResultForEnergySupplierByBalanceResponsibleParty(
        CalculationResult anyCalculationResult,
        CalculationResultCompletedFactory sut)
    {
        // Act
        var actual = sut.CreateForEnergySupplierByBalanceResponsibleParty(
            anyCalculationResult,
            "AEsGlnNumber",
            "ABrpGlnNumber");

        // Assert
        actual.AggregationPerGridarea.Should().BeNull();
        actual.AggregationPerEnergysupplierPerGridarea.Should().BeNull();
        actual.AggregationPerBalanceresponsiblepartyPerGridarea.Should().BeNull();
        actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.Should().NotBeNull();
    }

    [Theory]
    [InlineAutoMoqData]
    public void CreateCalculationResultCompletedForEnergySupplierByBalanceResponsibleParty_WhenCreating_PropertiesAreMappedCorrectly(
        CalculationResultBuilder calculationResultBuilder,
        CalculationResultCompletedFactory sut)
    {
        // Arrange
        var timeSeriesPoint = new TimeSeriesPoint(DateTimeOffset.Now, 10.101000000m, QuantityQuality.Estimated);
        var calculationResult = calculationResultBuilder.WithTimeSeriesPoints(new[] { timeSeriesPoint }).Build();
        var expectedBrpGln = "TheBrpGln";
        var expectedEsGln = "TheEsGln";

        // Act
        var actual = sut.CreateForEnergySupplierByBalanceResponsibleParty(
            calculationResult,
            expectedEsGln,
            expectedBrpGln);

        // Assert
        actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.GridAreaCode.Should()
            .Be(calculationResult.GridArea);
        actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.BalanceResponsiblePartyGlnOrEic
            .Should().Be(expectedBrpGln);
        actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.EnergySupplierGlnOrEic.Should()
            .Be(expectedEsGln);
        actual.BatchId.Should().Be(calculationResult.BatchId.ToString());
        actual.Resolution.Should().Be(Resolution.Quarter);
        actual.QuantityUnit.Should().Be(QuantityUnit.Kwh);
        actual.PeriodEndUtc.Should().Be(calculationResult.PeriodEnd.ToTimestamp());
        actual.PeriodStartUtc.Should().Be(calculationResult.PeriodStart.ToTimestamp());
        actual.TimeSeriesType.Should().Be(TimeSeriesTypeMapper.MapTimeSeriesType(calculationResult.TimeSeriesType));

        actual.TimeSeriesPoints[0].Quantity.Units.Should().Be(10);
        actual.TimeSeriesPoints[0].Quantity.Nanos.Should().Be(101000000);
        actual.TimeSeriesPoints[0].Time.Should().Be(timeSeriesPoint.Time.ToTimestamp());
        actual.TimeSeriesPoints[0].QuantityQuality.Should()
            .Be(QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality));
    }
}
