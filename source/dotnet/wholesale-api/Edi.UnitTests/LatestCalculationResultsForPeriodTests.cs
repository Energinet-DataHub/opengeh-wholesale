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
using Energinet.DataHub.Wholesale.Edi;
using Energinet.DataHub.Wholesale.Edi.Exceptions;
using Energinet.DataHub.Wholesale.Edi.Models;
using Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;
using FluentAssertions;
using FluentAssertions.Execution;
using NodaTime;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests;

public class LatestCalculationResultsForPeriodTests
{
    private DateTimeZone _dateTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!;

    [Fact]
    public void LatestCalculationForPeriods_WithACalculationForWholePeriod_LatestCalculationsForPeriod()
    {
        // Arrange
        var periodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var periodEnd = Instant.FromUtc(2024, 1, 15, 23, 0, 0);
        var calculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(periodStart)
            .WithPeriodEnd(periodEnd)
            .Build();

        var sut = new LatestCalculationResultsForPeriod(periodStart, periodEnd, _dateTimeZone, new List<CalculationDto>() { calculation });

        // Act
        var actual = sut.LatestCalculationForPeriods;

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(1);
        actual.First().PeriodStart.Should().Be(periodStart);
        actual.First().PeriodEnd.Should().Be(periodEnd);
    }

    [Fact]
    public void LatestCalculationForPeriods_WithMultipleCalculationForWholePeriod_LatestCalculationsForPeriod()
    {
        // Arrange
        var firstPeriodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var firstPeriodEnd = Instant.FromUtc(2024, 1, 15, 23, 0, 0);
        var secondPeriodStart = Instant.FromUtc(2024, 1, 16, 23, 0, 0);
        var secondPeriodEnd = Instant.FromUtc(2024, 1, 31, 23, 0, 0);
        var firstCalculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(firstPeriodStart)
            .WithPeriodEnd(firstPeriodEnd)
            .Build();
        var secondCalculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(secondPeriodStart)
            .WithPeriodEnd(secondPeriodEnd)
            .Build();

        var sut = new LatestCalculationResultsForPeriod(
            firstPeriodStart,
            secondPeriodEnd,
            _dateTimeZone,
            new List<CalculationDto>() { firstCalculation, secondCalculation });

        // Act
        var actual = sut.LatestCalculationForPeriods;

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(2);
        AssertCalculationsForWholePeriod(actual, firstPeriodStart, secondPeriodEnd);
    }

    [Fact]
    public void LatestCalculationForPeriods_WithCalculationsOverlappingInTheEnd_LatestCalculationsForEachDayInPeriod()
    {
        // Arrange
        var firstPeriodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var firstPeriodEnd = Instant.FromUtc(2024, 1, 20, 23, 0, 0);
        var secondPeriodStart = Instant.FromUtc(2024, 1, 10, 23, 0, 0);
        var secondPeriodEnd = Instant.FromUtc(2024, 1, 31, 23, 0, 0);
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

        var sut = new LatestCalculationResultsForPeriod(
            firstPeriodStart,
            secondPeriodEnd,
            _dateTimeZone,
            new List<CalculationDto>() { firstCalculation, secondCalculation });

        // Act
        var actual = sut.LatestCalculationForPeriods;

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(2);
        AssertCalculationsForWholePeriod(actual, firstPeriodStart, secondPeriodEnd);
        AssertCalculationsAreLatest(actual, new List<CalculationDto>() { firstCalculation, secondCalculation });
    }

    [Fact]
    public void LatestCalculationForPeriods_WithCalculationsOverlappingInTheBeginning_LatestCalculationsForEachDayInPeriod()
    {
        // Arrange
        var firstPeriodStart = Instant.FromUtc(2024, 1, 10, 23, 0, 0);
        var firstPeriodEnd = Instant.FromUtc(2024, 1, 31, 23, 0, 0);
        var secondPeriodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var secondPeriodEnd = Instant.FromUtc(2024, 1, 20, 23, 0, 0);
        var firstCalculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(firstPeriodStart)
            .WithPeriodEnd(firstPeriodEnd)
            .WithVersion(2)
            .Build();
        var secondCalculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(secondPeriodStart)
            .WithPeriodEnd(secondPeriodEnd)
            .WithVersion(1)
            .Build();

        var sut = new LatestCalculationResultsForPeriod(
            secondPeriodStart,
            firstPeriodEnd,
            _dateTimeZone,
            new List<CalculationDto>() { firstCalculation, secondCalculation });

        // Act
        var actual = sut.LatestCalculationForPeriods;

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(2);
        AssertCalculationsForWholePeriod(actual, secondPeriodStart, firstPeriodEnd);
        AssertCalculationsAreLatest(actual, new List<CalculationDto>() { firstCalculation, secondCalculation });
    }

    [Fact]
    public void LatestCalculationForPeriods_WithNewerCalculationInMiddleOfExistingCalculation_LatestCalculationsForEachDayInPeriod()
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

        var sut = new LatestCalculationResultsForPeriod(
            firstPeriodStart,
            firstPeriodEnd,
            _dateTimeZone,
            new List<CalculationDto>() { firstCalculation, secondCalculation });

        // Act
        var actual = sut.LatestCalculationForPeriods;

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(3);
        AssertCalculationsForWholePeriod(actual, firstPeriodStart, firstPeriodEnd);
        AssertCalculationsAreLatest(actual, new List<CalculationDto>() { firstCalculation, secondCalculation });
    }

    [Fact]
    public void LatestCalculationForPeriods_WithMissingCalculationForPartOfThePeriod_ThrowsMissingCalculationException()
    {
        // Arrange
        var firstPeriodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var firstPeriodEnd = Instant.FromUtc(2024, 1, 14, 23, 0, 0);
        var secondPeriodStart = Instant.FromUtc(2024, 1, 16, 23, 0, 0);
        var secondPeriodEnd = Instant.FromUtc(2024, 1, 31, 23, 0, 0);
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

        // Act
        var actual = () => new LatestCalculationResultsForPeriod(
            firstPeriodStart,
            secondPeriodEnd,
            _dateTimeZone,
            new List<CalculationDto>() { firstCalculation, secondCalculation });

        // Assert
        actual.Should().ThrowExactly<MissingCalculationException>();
    }

    [Fact]
    public void LatestCalculationForPeriods_WithMissingCalculationForTheWholePeriod_LatestCalculationForPeriodsIsEmpty()
    {
        // Arrange
        var periodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var periodEnd = Instant.FromUtc(2024, 1, 14, 23, 0, 0);

        // Act
        var actual = new LatestCalculationResultsForPeriod(
            periodStart,
            periodEnd,
            _dateTimeZone,
            new List<CalculationDto>());

        // Assert
        actual.LatestCalculationForPeriods.Should().BeEmpty();
    }

    [Fact]
    public void GetLatestCalculationsResultsPerDay_WithCalculationResults_ReturnLatestCalculationResultsPerDayWithVersion()
    {
        // Arrange
        var periodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var periodEnd = Instant.FromUtc(2024, 1, 31, 23, 0, 0);
        var calculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(periodStart)
            .WithPeriodEnd(periodEnd)
            .Build();

        var calculationResults = CalculationResultBuilder.AggregatedTimeSeries(calculation)
            .Build();

        var sut = new LatestCalculationResultsForPeriod(
            periodStart,
            periodEnd,
            _dateTimeZone,
            new List<CalculationDto>() { calculation });

        // Act
        var actual = sut.GetLatestCalculationsResultsPerDay(new List<AggregatedTimeSeries>() { calculationResults });

        // Assert
        using var assertionScope = new AssertionScope();
        AssertCalculationResultsForWholePeriod(actual, periodStart, periodEnd);
    }

    [Fact]
    public void GetLatestCalculationsResultsPerDay_WithCalculationResultsFromTwoCalculation_ReturnLatestCalculationResultsPerDay()
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

        var calculationResultsFromFirstCalculation = CalculationResultBuilder
            .AggregatedTimeSeries(firstCalculation)
            .Build();

        var calculationResultsFromSecondCalculation = CalculationResultBuilder
            .AggregatedTimeSeries(secondCalculation)
            .Build();

        var sut = new LatestCalculationResultsForPeriod(
            firstPeriodStart,
            secondPeriodEnd,
            _dateTimeZone,
            new List<CalculationDto>() { firstCalculation, secondCalculation });

        // Act
        var actual = sut
            .GetLatestCalculationsResultsPerDay(new List<AggregatedTimeSeries>()
            {
                calculationResultsFromFirstCalculation,
                calculationResultsFromSecondCalculation,
            });

        // Assert
        using var assertionScope = new AssertionScope();
        AssertCalculationResultsForWholePeriod(actual, firstPeriodStart, secondPeriodEnd);
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

        var sut = new LatestCalculationResultsForPeriod(
            periodStart,
            periodEnd,
            _dateTimeZone,
            new List<CalculationDto>() { calculation });

        // Act
        var actual = sut.GetLatestCalculationsResultsPerDay(new List<AggregatedTimeSeries>() { });

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

        var calculationResults = CalculationResultBuilder
            .AggregatedTimeSeries(calculation)
            .Build();

        var sut = new LatestCalculationResultsForPeriod(
            periodStart,
            periodEnd,
            _dateTimeZone,
            new List<CalculationDto>());

        // Act
        var actual = sut.GetLatestCalculationsResultsPerDay(new List<AggregatedTimeSeries>() { calculationResults });

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public void GetLatestCalculationsResultsPerDay_WithMissingCalculationResultForCalculation_ThrowsMissingCalculationResultException()
    {
        // Arrange
        var periodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var periodEnd = Instant.FromUtc(2024, 1, 31, 23, 0, 0);
        var calculationWithMissingCalculationResults = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(periodStart)
            .WithPeriodEnd(periodEnd)
            .Build();

        var sut = new LatestCalculationResultsForPeriod(
            periodStart,
            periodEnd,
            _dateTimeZone,
            new List<CalculationDto>() { calculationWithMissingCalculationResults });

        var anotherCalculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(periodStart)
            .WithPeriodEnd(periodEnd)
            .Build();

        var calculationResultFromAnotherCalculation = CalculationResultBuilder
            .AggregatedTimeSeries(anotherCalculation)
            .Build();

        // Act
        var actual = () => sut.GetLatestCalculationsResultsPerDay(new List<AggregatedTimeSeries>() { calculationResultFromAnotherCalculation });

        // Assert
        actual.Should().ThrowExactly<MissingCalculationResultException>();
    }

    private void AssertCalculationsForWholePeriod(IReadOnlyCollection<LatestCalculationForPeriod> actual, Instant periodStart, Instant periodEnd)
    {
        var currentInstant = periodStart;
        while (currentInstant <= periodEnd)
        {
            actual.Should().ContainSingle(x => x.PeriodStart <= currentInstant && x.PeriodEnd >= currentInstant);
            currentInstant = currentInstant.Plus(Duration.FromDays(1));
        }
    }

    private void AssertCalculationResultsForWholePeriod(IReadOnlyCollection<AggregatedTimeSeriesResult> actual, Instant periodStart, Instant periodEnd)
    {
        var currentInstant = periodStart;
        while (currentInstant <= periodEnd)
        {
            var currentZonedInstant = new ZonedDateTime(currentInstant, _dateTimeZone);
            var timeSeriesPointsPerDay = actual
                .SelectMany(x => x.TimeSeriesPoints)
                .Where(x => new ZonedDateTime(x.Time.ToInstant(), _dateTimeZone).Date == currentZonedInstant.Date);

            if (currentInstant != periodEnd)
            {
                timeSeriesPointsPerDay.Count().Should().Be(24 * 4, $"There should be 24 * 4 time series points {currentZonedInstant}");
            }
            else
            {
                timeSeriesPointsPerDay.Should().BeEmpty();
            }

            currentInstant = currentInstant.Plus(Duration.FromDays(1));
        }
    }

    private void AssertCalculationsAreLatest(IReadOnlyCollection<LatestCalculationForPeriod> actual, List<CalculationDto> calculations)
    {
        foreach (var latestCalculation in actual.OrderByDescending(x => x.CalculationVersion))
        {
            calculations.Should().ContainSingle(x =>
                x.Version >= latestCalculation.CalculationVersion
                && x.PeriodStart.ToInstant() <= latestCalculation.PeriodStart
                && x.PeriodEnd.ToInstant() >= latestCalculation.PeriodEnd);
        }
    }

    private void AssertCalculationResultsAreLatest(IReadOnlyCollection<AggregatedTimeSeriesResult> actual, List<CalculationDto> calculations)
    {
        foreach (var latestCalculation in actual.OrderByDescending(x => x.Version))
        {
            calculations.Should().ContainSingle(x =>
                x.Version >= latestCalculation.Version
                && x.PeriodStart.ToInstant() <= latestCalculation.TimeSeriesPoints.Min(x => x.Time.ToInstant())
                && x.PeriodEnd.ToInstant() >= latestCalculation.TimeSeriesPoints.Max(x => x.Time.ToInstant()));
        }
    }
}
