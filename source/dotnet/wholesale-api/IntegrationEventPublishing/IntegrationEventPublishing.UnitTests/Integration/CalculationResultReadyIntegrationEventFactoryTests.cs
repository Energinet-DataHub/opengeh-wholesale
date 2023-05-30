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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.Processes.Model;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Infrastructure;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Infrastructure.Integration;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using Xunit;
using ProcessType = Energinet.DataHub.Wholesale.Common.Models.ProcessType;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;
using TimeSeriesPoint = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesPoint;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.IntegrationEventPublishing.UnitTests.Integration;

public class CalculationResultReadyIntegrationEventFactoryTests
{
    [Fact]
    public void CreateCalculationResultCompletedForGridArea_WhenQuantityQualityCalculated_ExceptionIsThrown()
    {
        // Arrange
        var sut = new CalculationResultCompletedIntegrationEventFactory();
        var processStepResultDto = new ProcessStepResult(
            TimeSeriesType.Production,
            new TimeSeriesPoint[] { new(DateTimeOffset.Now, 10.0m, QuantityQuality.Calculated) });
        var processCompletedEventDto = new ProcessCompletedEventDto(
            "805",
            Guid.NewGuid(),
            ProcessType.Aggregation,
            Instant.FromUtc(2022, 5, 1, 0, 0),
            Instant.FromUtc(2022, 5, 2, 0, 0));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => sut.CreateForGridArea(
            processStepResultDto,
            processCompletedEventDto));
    }

    [Fact]
    public void CreateCalculationResultCompletedForGridArea_WhenCreating_ResultIsForTotalGridArea()
    {
        // Arrange
        var sut = new CalculationResultCompletedIntegrationEventFactory();
        var processStepResultDto = new ProcessStepResult(
            TimeSeriesType.Production,
            new TimeSeriesPoint[] { new(DateTimeOffset.Now, 10.0m, QuantityQuality.Estimated) });
        var processCompletedEventDto = new ProcessCompletedEventDto(
            "805",
            Guid.NewGuid(),
            ProcessType.Aggregation,
            Instant.FromUtc(2022, 5, 1, 0, 0),
            Instant.FromUtc(2022, 5, 2, 0, 0));

        // Act
        var actual = sut.CreateForGridArea(
            processStepResultDto,
            processCompletedEventDto);

        // Assert
        AssertionExtensions.Should((object)actual.AggregationPerBalanceresponsiblepartyPerGridarea).BeNull();
        AssertionExtensions.Should((object)actual.AggregationPerEnergysupplierPerGridarea).BeNull();
        AssertionExtensions.Should((object)actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea).BeNull();
        AssertionExtensions.Should((object)actual.AggregationPerGridarea).NotBeNull();
    }

    [Fact]
    public void CreateCalculationResultCompletedForGridArea_WhenCreating_PropertiesAreMappedCorrectly()
    {
        // Arrange
        var sut = new CalculationResultCompletedIntegrationEventFactory();
        var timeSeriesPoint = new TimeSeriesPoint(DateTimeOffset.Now, 10.101000000m, QuantityQuality.Estimated);
        var processStepResultDto = new ProcessStepResult(
            TimeSeriesType.Production,
            new[] { timeSeriesPoint });
        var processCompletedEventDto = new ProcessCompletedEventDto(
            "805",
            Guid.NewGuid(),
            ProcessType.Aggregation,
            Instant.FromUtc(2022, 5, 1, 0, 0),
            Instant.FromUtc(2022, 5, 2, 0, 0));

        // Act
        var actual = sut.CreateForGridArea(
            processStepResultDto,
            processCompletedEventDto);

        // Assert
        AssertionExtensions.Should((string)actual.AggregationPerGridarea.GridAreaCode).Be(processCompletedEventDto.GridAreaCode);
        AssertionExtensions.Should((string)actual.BatchId).Be(processCompletedEventDto.BatchId.ToString());
        actual.Resolution.Should().Be(Resolution.Quarter);
        actual.QuantityUnit.Should().Be(QuantityUnit.Kwh);
        actual.PeriodEndUtc.Should().Be(processCompletedEventDto.PeriodEnd.ToTimestamp());
        actual.PeriodStartUtc.Should().Be(processCompletedEventDto.PeriodStart.ToTimestamp());
        actual.TimeSeriesType.Should().Be(TimeSeriesTypeMapper.MapTimeSeriesType(processStepResultDto.TimeSeriesType));

        AssertionExtensions.Should((long)actual.TimeSeriesPoints[0].Quantity.Units).Be(10);
        AssertionExtensions.Should((int)actual.TimeSeriesPoints[0].Quantity.Nanos).Be(101000000);
        actual.TimeSeriesPoints[0].Time.Should().Be(timeSeriesPoint.Time.ToTimestamp());
        actual.TimeSeriesPoints[0].QuantityQuality.Should().Be(QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality));
    }

    [Fact]
    public void CreateCalculationResultCompletedForEnergySupplier_WhenCreating_ResultIsForTotalEnergySupplier()
    {
        // Arrange
        var sut = new CalculationResultCompletedIntegrationEventFactory();
        var processStepResultDto = new ProcessStepResult(
            TimeSeriesType.Production,
            new TimeSeriesPoint[] { new(DateTimeOffset.Now, 10.0m, QuantityQuality.Estimated) });
        var processCompletedEventDto = new ProcessCompletedEventDto(
            "805",
            Guid.NewGuid(),
            ProcessType.Aggregation,
            Instant.FromUtc(2022, 5, 1, 0, 0),
            Instant.FromUtc(2022, 5, 2, 0, 0));

        // Act
        var actual = sut.CreateForEnergySupplier(
            processStepResultDto,
            processCompletedEventDto,
            "AGlnNumber");

        // Assert
        AssertionExtensions.Should((object)actual.AggregationPerBalanceresponsiblepartyPerGridarea).BeNull();
        AssertionExtensions.Should((object)actual.AggregationPerGridarea).BeNull();
        AssertionExtensions.Should((object)actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea).BeNull();
        AssertionExtensions.Should((object)actual.AggregationPerEnergysupplierPerGridarea).NotBeNull();
    }

    [Fact]
    public void CreateCalculationResultCompletedForEnergySupplier_WhenCreating_PropertiesAreMappedCorrectly()
    {
        // Arrange
        var sut = new CalculationResultCompletedIntegrationEventFactory();
        var timeSeriesPoint = new TimeSeriesPoint(DateTimeOffset.Now, 10.101000000m, QuantityQuality.Estimated);
        var processStepResultDto = new ProcessStepResult(
            TimeSeriesType.Production,
            new[] { timeSeriesPoint });
        var processCompletedEventDto = new ProcessCompletedEventDto(
            "805",
            Guid.NewGuid(),
            ProcessType.Aggregation,
            Instant.FromUtc(2022, 5, 1, 0, 0),
            Instant.FromUtc(2022, 5, 2, 0, 0));
        var expectedGln = "TheGln";

        // Act
        var actual = sut.CreateForEnergySupplier(
            processStepResultDto,
            processCompletedEventDto,
            expectedGln);

        // Assert
        AssertionExtensions.Should((string)actual.AggregationPerEnergysupplierPerGridarea.GridAreaCode).Be(processCompletedEventDto.GridAreaCode);
        AssertionExtensions.Should((string)actual.AggregationPerEnergysupplierPerGridarea.EnergySupplierGlnOrEic).Be(expectedGln);
        AssertionExtensions.Should((string)actual.BatchId).Be(processCompletedEventDto.BatchId.ToString());
        actual.Resolution.Should().Be(Resolution.Quarter);
        actual.QuantityUnit.Should().Be(QuantityUnit.Kwh);
        actual.PeriodEndUtc.Should().Be(processCompletedEventDto.PeriodEnd.ToTimestamp());
        actual.PeriodStartUtc.Should().Be(processCompletedEventDto.PeriodStart.ToTimestamp());
        actual.TimeSeriesType.Should().Be(TimeSeriesTypeMapper.MapTimeSeriesType(processStepResultDto.TimeSeriesType));

        AssertionExtensions.Should((long)actual.TimeSeriesPoints[0].Quantity.Units).Be(10);
        AssertionExtensions.Should((int)actual.TimeSeriesPoints[0].Quantity.Nanos).Be(101000000);
        actual.TimeSeriesPoints[0].Time.Should().Be(timeSeriesPoint.Time.ToTimestamp());
        actual.TimeSeriesPoints[0].QuantityQuality.Should().Be(QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality));
    }

    [Fact]
    public void CreateCalculationResultCompletedForEnergySupplier_WhenQuantityQualityCalculated_ExceptionIsThrown()
    {
        // Arrange
        var sut = new CalculationResultCompletedIntegrationEventFactory();
        var processStepResultDto = new ProcessStepResult(
            TimeSeriesType.Production,
            new TimeSeriesPoint[] { new(DateTimeOffset.Now, 10.0m, QuantityQuality.Calculated) });
        var processCompletedEventDto = new ProcessCompletedEventDto(
            "805",
            Guid.NewGuid(),
            ProcessType.Aggregation,
            Instant.FromUtc(2022, 5, 1, 0, 0),
            Instant.FromUtc(2022, 5, 2, 0, 0));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => sut.CreateForEnergySupplier(
            processStepResultDto,
            processCompletedEventDto,
            "AGlnNumber"));
    }

    [Fact]
    public void CreateCalculationResultCompletedForBalanceResponsibleParty_ReturnsResultForBalanceResponsibleParty()
    {
        // Arrange
        var sut = new CalculationResultCompletedIntegrationEventFactory();
        var processStepResultDto = new ProcessStepResult(
            TimeSeriesType.Production,
            new TimeSeriesPoint[] { new(DateTimeOffset.Now, 10.0m, QuantityQuality.Estimated) });
        var processCompletedEventDto = new ProcessCompletedEventDto(
            "805",
            Guid.NewGuid(),
            ProcessType.Aggregation,
            Instant.FromUtc(2022, 5, 1, 0, 0),
            Instant.FromUtc(2022, 5, 2, 0, 0));

        // Act
        var actual = sut.CreateForBalanceResponsibleParty(
            processStepResultDto,
            processCompletedEventDto,
            "ABrpGlnNumber");

        // Assert
        AssertionExtensions.Should((object)actual.AggregationPerGridarea).BeNull();
        AssertionExtensions.Should((object)actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea).BeNull();
        AssertionExtensions.Should((object)actual.AggregationPerEnergysupplierPerGridarea).BeNull();
        AssertionExtensions.Should((object)actual.AggregationPerBalanceresponsiblepartyPerGridarea).NotBeNull();
    }

    [Fact]
    public void CreateCalculationResultCompletedForBalanceResponsibleParty_WhenCreating_PropertiesAreMappedCorrectly()
    {
        // Arrange
        var sut = new CalculationResultCompletedIntegrationEventFactory();
        var timeSeriesPoint = new TimeSeriesPoint(DateTimeOffset.Now, 10.101000000m, QuantityQuality.Estimated);
        var processStepResultDto = new ProcessStepResult(
            TimeSeriesType.Production,
            new[] { timeSeriesPoint });
        var processCompletedEventDto = new ProcessCompletedEventDto(
            "805",
            Guid.NewGuid(),
            ProcessType.Aggregation,
            Instant.FromUtc(2022, 5, 1, 0, 0),
            Instant.FromUtc(2022, 5, 2, 0, 0));
        var expectedGln = "TheBrpGln";

        // Act
        var actual = sut.CreateForBalanceResponsibleParty(
            processStepResultDto,
            processCompletedEventDto,
            expectedGln);

        // Assert
        AssertionExtensions.Should((string)actual.AggregationPerBalanceresponsiblepartyPerGridarea.GridAreaCode).Be(processCompletedEventDto.GridAreaCode);
        AssertionExtensions.Should((string)actual.AggregationPerBalanceresponsiblepartyPerGridarea.BalanceResponsiblePartyGlnOrEic).Be(expectedGln);
        AssertionExtensions.Should((string)actual.BatchId).Be(processCompletedEventDto.BatchId.ToString());
        actual.Resolution.Should().Be(Resolution.Quarter);
        actual.QuantityUnit.Should().Be(QuantityUnit.Kwh);
        actual.PeriodEndUtc.Should().Be(processCompletedEventDto.PeriodEnd.ToTimestamp());
        actual.PeriodStartUtc.Should().Be(processCompletedEventDto.PeriodStart.ToTimestamp());
        actual.TimeSeriesType.Should().Be(TimeSeriesTypeMapper.MapTimeSeriesType(processStepResultDto.TimeSeriesType));

        AssertionExtensions.Should((long)actual.TimeSeriesPoints[0].Quantity.Units).Be(10);
        AssertionExtensions.Should((int)actual.TimeSeriesPoints[0].Quantity.Nanos).Be(101000000);
        actual.TimeSeriesPoints[0].Time.Should().Be(timeSeriesPoint.Time.ToTimestamp());
        actual.TimeSeriesPoints[0].QuantityQuality.Should().Be(QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality));
    }

    [Fact]
    public void CreateCalculationResultCompletedForBalanceResponsibleParty_WhenQuantityQualityCalculated_ExceptionIsThrown()
    {
        // Arrange
        var sut = new CalculationResultCompletedIntegrationEventFactory();
        var processStepResultDto = new ProcessStepResult(
            TimeSeriesType.Production,
            new TimeSeriesPoint[] { new(DateTimeOffset.Now, 10.0m, QuantityQuality.Calculated) });
        var processCompletedEventDto = new ProcessCompletedEventDto(
            "805",
            Guid.NewGuid(),
            ProcessType.Aggregation,
            Instant.FromUtc(2022, 5, 1, 0, 0),
            Instant.FromUtc(2022, 5, 2, 0, 0));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => sut.CreateForBalanceResponsibleParty(
            processStepResultDto,
            processCompletedEventDto,
            "ABrpGlnNumber"));
    }

    [Fact]
    public void CreateCalculationResultCompletedForEnergySupplierByBalanceResponsibleParty_ReturnsResultForEnergySupplierByBalanceResponsibleParty()
    {
        // Arrange
        var sut = new CalculationResultCompletedIntegrationEventFactory();
        var processStepResultDto = new ProcessStepResult(
            TimeSeriesType.NonProfiledConsumption,
            new TimeSeriesPoint[] { new(DateTimeOffset.Now, 10.0m, QuantityQuality.Estimated) });
        var processCompletedEventDto = new ProcessCompletedEventDto(
            "805",
            Guid.NewGuid(),
            ProcessType.Aggregation,
            Instant.FromUtc(2022, 5, 1, 0, 0),
            Instant.FromUtc(2022, 5, 2, 0, 0));

        // Act
        var actual = sut.CreateForEnergySupplierByBalanceResponsibleParty(
            processStepResultDto,
            processCompletedEventDto,
            "AEsGlnNumber",
            "ABrpGlnNumber");

        // Assert
        AssertionExtensions.Should((object)actual.AggregationPerGridarea).BeNull();
        AssertionExtensions.Should((object)actual.AggregationPerEnergysupplierPerGridarea).BeNull();
        AssertionExtensions.Should((object)actual.AggregationPerBalanceresponsiblepartyPerGridarea).BeNull();
        AssertionExtensions.Should((object)actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea).NotBeNull();
    }

    [Fact]
    public void CreateCalculationResultCompletedForEnergySupplierByBalanceResponsibleParty_WhenCreating_PropertiesAreMappedCorrectly()
    {
        // Arrange
        var sut = new CalculationResultCompletedIntegrationEventFactory();
        var timeSeriesPoint = new TimeSeriesPoint(DateTimeOffset.Now, 10.101000000m, QuantityQuality.Estimated);
        var processStepResultDto = new ProcessStepResult(
            TimeSeriesType.NonProfiledConsumption,
            new[] { timeSeriesPoint });
        var processCompletedEventDto = new ProcessCompletedEventDto(
            "805",
            Guid.NewGuid(),
            ProcessType.Aggregation,
            Instant.FromUtc(2022, 5, 1, 0, 0),
            Instant.FromUtc(2022, 5, 2, 0, 0));
        var expectedBrpGln = "TheBrpGln";
        var expectedEsGln = "TheEsGln";

        // Act
        var actual = sut.CreateForEnergySupplierByBalanceResponsibleParty(
            processStepResultDto,
            processCompletedEventDto,
            expectedEsGln,
            expectedBrpGln);

        // Assert
        AssertionExtensions.Should((string)actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.GridAreaCode).Be(processCompletedEventDto.GridAreaCode);
        AssertionExtensions.Should((string)actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.BalanceResponsiblePartyGlnOrEic).Be(expectedBrpGln);
        AssertionExtensions.Should((string)actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.EnergySupplierGlnOrEic).Be(expectedEsGln);
        AssertionExtensions.Should((string)actual.BatchId).Be(processCompletedEventDto.BatchId.ToString());
        actual.Resolution.Should().Be(Resolution.Quarter);
        actual.QuantityUnit.Should().Be(QuantityUnit.Kwh);
        actual.PeriodEndUtc.Should().Be(processCompletedEventDto.PeriodEnd.ToTimestamp());
        actual.PeriodStartUtc.Should().Be(processCompletedEventDto.PeriodStart.ToTimestamp());
        actual.TimeSeriesType.Should().Be(TimeSeriesTypeMapper.MapTimeSeriesType(processStepResultDto.TimeSeriesType));

        AssertionExtensions.Should((long)actual.TimeSeriesPoints[0].Quantity.Units).Be(10);
        AssertionExtensions.Should((int)actual.TimeSeriesPoints[0].Quantity.Nanos).Be(101000000);
        actual.TimeSeriesPoints[0].Time.Should().Be(timeSeriesPoint.Time.ToTimestamp());
        actual.TimeSeriesPoints[0].QuantityQuality.Should().Be(QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality));
    }

    [Fact]
    public void CreateCalculationResultCompletedForEnergySupplierByBalanceResponsibleParty_WhenQuantityQualityCalculated_ExceptionIsThrown()
    {
        // Arrange
        var sut = new CalculationResultCompletedIntegrationEventFactory();
        var processStepResultDto = new ProcessStepResult(
            TimeSeriesType.NonProfiledConsumption,
            new TimeSeriesPoint[] { new(DateTimeOffset.Now, 10.0m, QuantityQuality.Calculated) });
        var processCompletedEventDto = new ProcessCompletedEventDto(
            "805",
            Guid.NewGuid(),
            ProcessType.Aggregation,
            Instant.FromUtc(2022, 5, 1, 0, 0),
            Instant.FromUtc(2022, 5, 2, 0, 0));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => sut.CreateForEnergySupplierByBalanceResponsibleParty(
            processStepResultDto,
            processCompletedEventDto,
            "AEsGlnNumer",
            "ABrpGlnNumber"));
    }
}
