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

using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Edi.Calculations;
using Energinet.DataHub.Wholesale.Edi.Exceptions;
using Energinet.DataHub.Wholesale.Edi.Models;
using Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;
using FluentAssertions;
using FluentAssertions.Execution;
using NodaTime;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Calculations;

public class CalculationResultPeriodCalculatorTests
{
    private DateTimeZone _dateTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!;

    [Fact]
    public void
        GetLatestCalculationsResultsPerDay_WithCalculationResults_ReturnLatestCalculationResultsPerDayWithVersion()
    {
        // Arrange
        var expectedQuantityPerTimeSeriePoint = 2;
        var calculationId = Guid.NewGuid();

        var periodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var periodEnd = Instant.FromUtc(2024, 1, 10, 23, 0, 0);

        var calculation = LatestCalculationForPeriodBuilder
            .LatestCalculationForPeriod()
            .ForPeriod(periodStart, periodEnd.Minus(Duration.FromDays(1)))
            .WithCalculationId(calculationId)
            .WithVersion(1)
            .Build();

        var calculationResults = AggregatedTimeSeriesBuilder
            .AggregatedTimeSeries()
            .ForPeriod(periodStart, periodEnd)
            .WithCalculationId(calculationId)
            .WithTimeSeriePointQuantity(expectedQuantityPerTimeSeriePoint)
            .Build();

        var sut = new CalculationResultPeriodCalculator();

        // Act
        var actual = sut.GetLatestCalculationsResultsPerDay(
            new List<LatestCalculationForPeriod>() { calculation },
            new List<AggregatedTimeSeries>() { calculationResults });

        // Assert
        using var assertionScope = new AssertionScope();
        AssertCalculationResultsCoversWholePeriod(actual, periodStart, periodEnd);

        actual.Should().ContainSingle(c => c.PeriodStart.InUtc().Day == 1 && c.PeriodEnd.InUtc().Day == 10)
            .Which.TimeSeriesPoints.Sum(x => x.Quantity).Should().Be(expectedQuantityPerTimeSeriePoint * 4 * 24m * 9);
    }

    [Fact]
    public void
        GetLatestCalculationsResultsPerDay_WithCalculationResultsFromTwoCalculation_ReturnLatestCalculationResultsPerDay()
    {
        // Arrange
        var expectedQuantityPerTimeSeriePointForFirstCalculation = 2;
        var expectedQuantityPerTimeSeriePointForSecondCalculation = 5;
        var periodStartForFirstCalculationResult = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var periodEndForFirstCalculationResult = Instant.FromUtc(2024, 1, 6, 23, 0, 0);
        var periodStartForSecondCalculationResult = Instant.FromUtc(2024, 1, 3, 23, 0, 0);
        var periodEndForSecondCalculationResult = Instant.FromUtc(2024, 1, 4, 23, 0, 0);
        var firstCalculationId = Guid.NewGuid();
        var secondCalculationId = Guid.NewGuid();

        var firstCalculationPartOne = LatestCalculationForPeriodBuilder
                .LatestCalculationForPeriod()
                .ForPeriod(periodStartForFirstCalculationResult, periodStartForSecondCalculationResult.Minus(Duration.FromDays(1)))
                .WithCalculationId(firstCalculationId)
                .WithVersion(1)
                .Build();
        var secondCalculation = LatestCalculationForPeriodBuilder
                .LatestCalculationForPeriod()
                .ForPeriod(periodStartForSecondCalculationResult, periodEndForSecondCalculationResult.Minus(Duration.FromDays(1)))
                .WithCalculationId(secondCalculationId)
                .WithVersion(2)
                .Build();
        var firstCalculationPartTwo = LatestCalculationForPeriodBuilder
                .LatestCalculationForPeriod()
                .ForPeriod(periodEndForSecondCalculationResult, periodEndForFirstCalculationResult.Minus(Duration.FromDays(1)))
                .WithCalculationId(firstCalculationId)
                .WithVersion(1)
                .Build();

        var calculationResultsForWholePeriodFromFirstCalculation = AggregatedTimeSeriesBuilder
            .AggregatedTimeSeries()
            .ForPeriod(periodStartForFirstCalculationResult, periodEndForFirstCalculationResult)
            .WithCalculationId(firstCalculationId)
            .WithTimeSeriePointQuantity(expectedQuantityPerTimeSeriePointForFirstCalculation)
            .Build();

        var calculationResultsForDay3FromSecondCalculation = AggregatedTimeSeriesBuilder
            .AggregatedTimeSeries()
            .ForPeriod(periodStartForSecondCalculationResult, periodEndForSecondCalculationResult)
            .WithCalculationId(secondCalculationId)
            .WithTimeSeriePointQuantity(expectedQuantityPerTimeSeriePointForSecondCalculation)
            .Build();

        var sut = new CalculationResultPeriodCalculator();

        // Act
        var actual = sut.GetLatestCalculationsResultsPerDay(
            new List<LatestCalculationForPeriod>() { firstCalculationPartOne, secondCalculation, firstCalculationPartTwo },
            new List<AggregatedTimeSeries>() { calculationResultsForWholePeriodFromFirstCalculation, calculationResultsForDay3FromSecondCalculation, });

        // Assert
        using var assertionScope = new AssertionScope();
        AssertCalculationResultsCoversWholePeriod(actual, periodStartForFirstCalculationResult, periodEndForFirstCalculationResult);

        actual.Should().ContainSingle(c => c.PeriodStart.InUtc().Day == 1 && c.PeriodEnd.InUtc().Day == 3)
            .Which.TimeSeriesPoints.Sum(x => x.Quantity).Should().Be(expectedQuantityPerTimeSeriePointForFirstCalculation * 4 * 24m * 2);
        actual.Should().ContainSingle(c => c.PeriodStart.InUtc().Day == 3 && c.PeriodEnd.InUtc().Day == 4)
            .Which.TimeSeriesPoints.Sum(x => x.Quantity).Should().Be(expectedQuantityPerTimeSeriePointForSecondCalculation * 4 * 24m);
        actual.Should().ContainSingle(c => c.PeriodStart.InUtc().Day == 4 && c.PeriodEnd.InUtc().Day == 6)
            .Which.TimeSeriesPoints.Sum(x => x.Quantity).Should().Be(expectedQuantityPerTimeSeriePointForFirstCalculation * 4 * 24m * 2);
    }

    [Fact]
    public void GetLatestCalculationsResultsPerDay_WithNoCalculationResult_ReturnsEmptyCalculationResults()
    {
        // Arrange
        var periodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var periodEnd = Instant.FromUtc(2024, 1, 31, 23, 0, 0);
        var calculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(periodStart)
            .WithPeriodEnd(periodEnd)
            .Build();

        var calculationPeriodCalculator = new CalculationPeriodCalculator(_dateTimeZone);
        var latestCalculations = calculationPeriodCalculator
            .FindLatestCalculationsForPeriod(
                periodStart,
                periodEnd,
                new List<CalculationDto>() { calculation });

        var sut = new CalculationResultPeriodCalculator();

        // Act
        var actual = sut.GetLatestCalculationsResultsPerDay(
            latestCalculations,
            new List<AggregatedTimeSeries>() { });

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public void GetLatestCalculationsResultsPerDay_WithNoCalculation_ReturnsEmptyCalculationResults()
    {
        // Arrange
        var periodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var periodEnd = Instant.FromUtc(2024, 1, 31, 23, 0, 0);
        var calculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(periodStart)
            .WithPeriodEnd(periodEnd)
            .Build();

        var calculationResults = AggregatedTimeSeriesBuilder
            .AggregatedTimeSeries(calculation)
            .Build();

        var calculationPeriodCalculator = new CalculationPeriodCalculator(_dateTimeZone);
        var latestCalculations = calculationPeriodCalculator
            .FindLatestCalculationsForPeriod(
                periodStart,
                periodEnd,
                new List<CalculationDto>() { });

        var sut = new CalculationResultPeriodCalculator();

        // Act
        var actual = sut.GetLatestCalculationsResultsPerDay(
            latestCalculations,
            new List<AggregatedTimeSeries>() { calculationResults });

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public void
        GetLatestCalculationsResultsPerDay_WithMissingCalculationResultForCalculation_ThrowsMissingCalculationResultException()
    {
        // Arrange
        var periodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var periodEnd = Instant.FromUtc(2024, 1, 31, 23, 0, 0);
        var calculationWithMissingCalculationResults = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(periodStart)
            .WithPeriodEnd(periodEnd)
            .Build();

        var calculationPeriodCalculator = new CalculationPeriodCalculator(_dateTimeZone);
        var latestCalculations = calculationPeriodCalculator
            .FindLatestCalculationsForPeriod(
                periodStart,
                periodEnd,
                new List<CalculationDto>() { calculationWithMissingCalculationResults });

        var anotherCalculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(periodStart)
            .WithPeriodEnd(periodEnd)
            .Build();

        var calculationResultFromAnotherCalculation = AggregatedTimeSeriesBuilder
            .AggregatedTimeSeries(anotherCalculation)
            .Build();

        var sut = new CalculationResultPeriodCalculator();

        // Act
        var actual = () => sut.GetLatestCalculationsResultsPerDay(
            latestCalculations,
            new List<AggregatedTimeSeries>() { calculationResultFromAnotherCalculation });

        // Assert
        actual.Should().ThrowExactly<MissingCalculationResultException>();
    }

    private void AssertCalculationResultsCoversWholePeriod(
        IReadOnlyCollection<AggregatedTimeSeriesResult> actual,
        Instant periodStart,
        Instant periodEnd)
    {
        AggregatedTimeSeriesResult? lastCalculationResult = null;
        foreach (var calculationForPeriod in actual.OrderBy(x => x.PeriodStart))
        {
            if (lastCalculationResult == null)
            {
                calculationForPeriod.PeriodStart.Should().Be(periodStart);
            }
            else
            {
                calculationForPeriod.PeriodStart.Should().Be(lastCalculationResult.PeriodEnd);
            }

            lastCalculationResult = calculationForPeriod;
        }

        lastCalculationResult!.PeriodEnd.Should().Be(periodEnd);
    }
}
