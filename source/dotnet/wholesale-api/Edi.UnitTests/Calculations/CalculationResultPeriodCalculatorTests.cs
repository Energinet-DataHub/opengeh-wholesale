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

namespace Energinet.DataHub.Wholesale.EDI.UnitTests;

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
        var firstPeriodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var firstPeriodEnd = Instant.FromUtc(2024, 1, 31, 23, 0, 0);
        var secondPeriodStart = Instant.FromUtc(2024, 1, 10, 23, 0, 0);
        var secondPeriodEnd = Instant.FromUtc(2024, 1, 20, 23, 0, 0);
        var firstCalculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(firstPeriodStart)
            .WithPeriodEnd(firstPeriodEnd)
            .WithVersion(1)
            .Build();
        var secondCalculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(secondPeriodStart)
            .WithPeriodEnd(secondPeriodEnd)
            .WithVersion(2)
            .Build();

        var calculationResultsFromFirstCalculation = AggregatedTimeSeriesBuilder
            .AggregatedTimeSeries(firstCalculation)
            .Build();

        var calculationResultsFromSecondCalculation = AggregatedTimeSeriesBuilder
            .AggregatedTimeSeries(secondCalculation)
            .Build();

        var calculationPeriodCalculator = new CalculationPeriodCalculator(_dateTimeZone);
        var latestCalculations = calculationPeriodCalculator
            .FindLatestCalculationsForPeriod(
                firstPeriodStart,
                secondPeriodEnd,
                new List<CalculationDto>() { firstCalculation, secondCalculation });

        var sut = new CalculationResultPeriodCalculator();

        // Act
        var actual = sut.GetLatestCalculationsResultsPerDay(
            latestCalculations,
            new List<AggregatedTimeSeries>() { calculationResultsFromFirstCalculation, calculationResultsFromSecondCalculation, });

        // Assert
        using var assertionScope = new AssertionScope();
        AssertCalculationResultsCoversWholePeriod(actual, firstPeriodStart, secondPeriodEnd);
        AssertCalculationResultsAreLatest(actual, new List<CalculationDto>() { firstCalculation, secondCalculation });
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

    private void AssertCalculationResultsAreLatest(
        IReadOnlyCollection<AggregatedTimeSeriesResult> actual,
        IReadOnlyCollection<CalculationDto> calculations)
    {
        foreach (var latestCalculationResult in actual.OrderByDescending(x => x.Version))
        {
            calculations.Should().ContainSingle(x =>
                x.Version >= latestCalculationResult.Version
                && x.PeriodStart.ToInstant() <= latestCalculationResult.TimeSeriesPoints.Min(x => x.Time.ToInstant())
                && x.PeriodEnd.ToInstant() >= latestCalculationResult.TimeSeriesPoints.Max(x => x.Time.ToInstant()));
        }
    }
}
