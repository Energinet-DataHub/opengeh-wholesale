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

using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Infrastructure;
using Energinet.DataHub.Wholesale.Infrastructure.Integration;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using Xunit;
using ProcessType = Energinet.DataHub.Wholesale.Contracts.ProcessType;
using QuantityQuality = Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate.QuantityQuality;
using TimeSeriesPoint = Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesPoint;
using TimeSeriesType = Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.Integration;

public class CalculationResultReadyIntegrationEventFactoryTests
{
    [Fact]
    public void CreateCalculationResultCompletedForGridArea_WhenQuantityQualityCalculated_ExceptionIsThrown()
    {
        // Arrange
        var sut = new CalculationResultReadyIntegrationEventFactory();
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
        Assert.Throws<ArgumentException>(() => sut.CreateCalculationResultCompletedForGridArea(
            processStepResultDto,
            processCompletedEventDto));
    }

    [Fact]
    public void CreateCalculationResultCompletedForGridArea_WhenCreating_ResultIsForTotalGridArea()
    {
        // Arrange
        var sut = new CalculationResultReadyIntegrationEventFactory();
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
        var actual = sut.CreateCalculationResultCompletedForGridArea(
            processStepResultDto,
            processCompletedEventDto);

        // Assert
        actual.AggregationPerBalanceresponsiblepartyPerGridarea.Should().BeNull();
        actual.AggregationPerEnergysupplierPerGridarea.Should().BeNull();
        actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.Should().BeNull();
        actual.AggregationPerGridarea.Should().NotBeNull();
    }

    [Fact]
    public void CreateCalculationResultCompletedForGridArea_WhenCreating_PropertiesAreMappedCorrectly()
    {
        // Arrange
        var sut = new CalculationResultReadyIntegrationEventFactory();
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
        var actual = sut.CreateCalculationResultCompletedForGridArea(
            processStepResultDto,
            processCompletedEventDto);

        // Assert
        actual.AggregationPerGridarea.GridAreaCode.Should().Be(processCompletedEventDto.GridAreaCode);
        actual.BatchId.Should().Be(processCompletedEventDto.BatchId.ToString());
        actual.Resolution.Should().Be(Resolution.Quarter);
        actual.QuantityUnit.Should().Be(QuantityUnit.Kwh);
        actual.PeriodEndUtc.Should().Be(processCompletedEventDto.PeriodEnd.ToTimestamp());
        actual.PeriodStartUtc.Should().Be(processCompletedEventDto.PeriodStart.ToTimestamp());
        actual.TimeSeriesType.Should().Be(TimeSeriesTypeMapper.MapTimeSeriesType(processStepResultDto.TimeSeriesType));

        actual.TimeSeriesPoints[0].Quantity.Units.Should().Be(10);
        actual.TimeSeriesPoints[0].Quantity.Nanos.Should().Be(101000000);
        actual.TimeSeriesPoints[0].Time.Should().Be(timeSeriesPoint.Time.ToTimestamp());
        actual.TimeSeriesPoints[0].QuantityQuality.Should().Be(QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality));
    }

    [Fact]
    public void CreateCalculationResultCompletedForEnergySupplier_WhenCreating_ResultIsForTotalEnergySupplier()
    {
        // Arrange
        var sut = new CalculationResultReadyIntegrationEventFactory();
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
        var actual = sut.CreateCalculationResultCompletedForEnergySupplier(
            processStepResultDto,
            processCompletedEventDto,
            "AGlnNumber");

        // Assert
        actual.AggregationPerBalanceresponsiblepartyPerGridarea.Should().BeNull();
        actual.AggregationPerGridarea.Should().BeNull();
        actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.Should().BeNull();
        actual.AggregationPerEnergysupplierPerGridarea.Should().NotBeNull();
    }

    [Fact]
    public void CreateCalculationResultCompletedForEnergySupplier_WhenCreating_PropertiesAreMappedCorrectly()
    {
        // Arrange
        var sut = new CalculationResultReadyIntegrationEventFactory();
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
        var actual = sut.CreateCalculationResultCompletedForEnergySupplier(
            processStepResultDto,
            processCompletedEventDto,
            expectedGln);

        // Assert
        actual.AggregationPerEnergysupplierPerGridarea.GridAreaCode.Should().Be(processCompletedEventDto.GridAreaCode);
        actual.AggregationPerEnergysupplierPerGridarea.EnergySupplierGlnOrEic.Should().Be(expectedGln);
        actual.BatchId.Should().Be(processCompletedEventDto.BatchId.ToString());
        actual.Resolution.Should().Be(Resolution.Quarter);
        actual.QuantityUnit.Should().Be(QuantityUnit.Kwh);
        actual.PeriodEndUtc.Should().Be(processCompletedEventDto.PeriodEnd.ToTimestamp());
        actual.PeriodStartUtc.Should().Be(processCompletedEventDto.PeriodStart.ToTimestamp());
        actual.TimeSeriesType.Should().Be(TimeSeriesTypeMapper.MapTimeSeriesType(processStepResultDto.TimeSeriesType));

        actual.TimeSeriesPoints[0].Quantity.Units.Should().Be(10);
        actual.TimeSeriesPoints[0].Quantity.Nanos.Should().Be(101000000);
        actual.TimeSeriesPoints[0].Time.Should().Be(timeSeriesPoint.Time.ToTimestamp());
        actual.TimeSeriesPoints[0].QuantityQuality.Should().Be(QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality));
    }

    [Fact]
    public void CreateCalculationResultCompletedForEnergySupplier_WhenQuantityQualityCalculated_ExceptionIsThrown()
    {
        // Arrange
        var sut = new CalculationResultReadyIntegrationEventFactory();
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
        Assert.Throws<ArgumentException>(() => sut.CreateCalculationResultCompletedForEnergySupplier(
            processStepResultDto,
            processCompletedEventDto,
            "AGlnNumber"));
    }
}
