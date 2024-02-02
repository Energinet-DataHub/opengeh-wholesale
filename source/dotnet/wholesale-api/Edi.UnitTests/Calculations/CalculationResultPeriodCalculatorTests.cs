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
                new List<CalculationDto>() { calculation, });

        var sut = new CalculationResultPeriodCalculator();

        // Act
        var actual = sut.GetLatestCalculationsResultsPerDay(
            latestCalculations,
            new List<AggregatedTimeSeries>() { calculationResults });

        // Assert
        using var assertionScope = new AssertionScope();
        AssertCalculationResultsCoversWholePeriod(actual, periodStart, periodEnd);
    }

    [Fact]
    public void
        GetLatestCalculationsResultsPerDay_WithCalculationResultsFromTwoCalculation_ReturnLatestCalculationResultsPerDay()
    {
        // Arrange
        var expectedQuantityPerTimeSeriePointForFirstCalculation = 2;
        var expectedQuantityPerTimeSeriePointForSecondCalculation = 5;
        var day1 = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var day6 = Instant.FromUtc(2024, 1, 6, 23, 0, 0);
        var day3 = Instant.FromUtc(2024, 1, 3, 23, 0, 0);
        var day4 = Instant.FromUtc(2024, 1, 4, 23, 0, 0);
        var firstCalculationId = Guid.NewGuid();
        var secondCalculationId = Guid.NewGuid();

        var firstLatestCalculation = LatestCalculationForPeriodBuilder
                .LatestCalculationForPeriod()
                .ForPeriod(day1, day3.Minus(Duration.FromDays(1)))
                .WithCalculationId(firstCalculationId)
                .WithVersion(1)
                .Build();
        var secondLatestCalculation = LatestCalculationForPeriodBuilder
                .LatestCalculationForPeriod()
                .ForPeriod(day3, day4.Minus(Duration.FromDays(1)))
                .WithCalculationId(secondCalculationId)
                .WithVersion(2)
                .Build();
        var thirdLatestCalculation = LatestCalculationForPeriodBuilder
                .LatestCalculationForPeriod()
                .ForPeriod(day4, day6)
                .WithCalculationId(firstCalculationId)
                .WithVersion(1)
                .Build();

        var calculationResultsForWholePeriodFromFirstCalculation = AggregatedTimeSeriesBuilder
            .AggregatedTimeSeries()
            .ForPeriod(day1, day6)
            .WithCalculationId(firstCalculationId)
            .WithTimeSeriePointQuantity(expectedQuantityPerTimeSeriePointForFirstCalculation)
            .Build();

        var calculationResultsForDay3FromSecondCalculation = AggregatedTimeSeriesBuilder
            .AggregatedTimeSeries()
            .ForPeriod(day3, day4)
            .WithCalculationId(secondCalculationId)
            .WithTimeSeriePointQuantity(expectedQuantityPerTimeSeriePointForSecondCalculation)
            .Build();

        var sut = new CalculationResultPeriodCalculator();

        // Act
        var actual = sut.GetLatestCalculationsResultsPerDay(
            new List<LatestCalculationForPeriod>() { firstLatestCalculation, secondLatestCalculation, thirdLatestCalculation },
            new List<AggregatedTimeSeries>() { calculationResultsForWholePeriodFromFirstCalculation, calculationResultsForDay3FromSecondCalculation, });

        // Assert
        using var assertionScope = new AssertionScope();
        AssertCalculationResultsCoversWholePeriod(actual, day1, day6);

        actual.Should().ContainSingle(c => c.PeriodStart.InUtc().Day == 1 && c.PeriodEnd.InUtc().Day == 2)
            .Which.TimeSeriesPoints.Sum(x => x.Quantity).Should().Be(expectedQuantityPerTimeSeriePointForFirstCalculation * 4 * 24m);
        actual.Should().ContainSingle(c => c.PeriodStart.InUtc().Day == day3.InUtc().Day && c.PeriodEnd.InUtc().Day == day4.InUtc().Day)
            .Which.TimeSeriesPoints.Sum(x => x.Quantity).Should().Be(expectedQuantityPerTimeSeriePointForSecondCalculation * 4 * 24m);
        actual.Should().ContainSingle(c => c.PeriodStart.InUtc().Day == 5 && c.PeriodEnd.InUtc().Day == 6)
            .Which.TimeSeriesPoints.Sum(x => x.Quantity).Should().Be(expectedQuantityPerTimeSeriePointForFirstCalculation * 4 * 24m);
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
