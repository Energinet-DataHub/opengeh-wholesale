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

public class CalculationPeriodCalculatorTests
{
    private DateTimeZone _dateTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!;

    [Fact]
    public void FindLatestCalculations_WithACalculationForWholePeriod_LatestCalculationsForPeriod()
    {
        // Arrange
        var periodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var periodEnd = Instant.FromUtc(2024, 1, 15, 23, 0, 0);
        var calculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(periodStart)
            .WithPeriodEnd(periodEnd)
            .Build();

        var sut = new CalculationPeriodCalculator(_dateTimeZone);

        // Act
        var actual = sut
            .FindLatestCalculationsForPeriod(periodStart, periodEnd, new List<CalculationDto>() { calculation });

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(1);
        AssertCalculationsCoversWholePeriod(actual, periodStart, periodEnd);
    }

    [Fact]
    public void FindLatestCalculations_WithMultipleCalculationForWholePeriod_LatestCalculationsForPeriod()
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

        var sut = new CalculationPeriodCalculator(_dateTimeZone);

        // Act
        var actual = sut
            .FindLatestCalculationsForPeriod(
                firstPeriodStart,
                secondPeriodEnd,
                new List<CalculationDto>() { firstCalculation, secondCalculation, });

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(2);
        AssertCalculationsCoversWholePeriod(actual, firstPeriodStart, secondPeriodEnd);
    }

    [Fact]
    public void FindLatestCalculations_WithMultipleCalculationForWholePeriod_ThrowException()
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

        var sut = new CalculationPeriodCalculator(_dateTimeZone);

        // Act
        var actual = sut
            .FindLatestCalculationsForPeriod(
                firstPeriodStart,
                secondPeriodEnd,
                new List<CalculationDto>() { firstCalculation, secondCalculation, });

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(2);
        AssertCalculationsCoversWholePeriod(actual, firstPeriodStart, secondPeriodEnd);
    }

    [Fact]
    public void FindLatestCalculations_WithCalculationsOverlappingInTheEnd_LatestCalculationsForEachDayInPeriod()
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

        var sut = new CalculationPeriodCalculator(_dateTimeZone);

        // Act
        var actual = sut
            .FindLatestCalculationsForPeriod(
                firstPeriodStart,
                secondPeriodEnd,
                new List<CalculationDto>() { firstCalculation, secondCalculation, });

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(2);
        AssertCalculationsCoversWholePeriod(actual, firstPeriodStart, secondPeriodEnd);
        AssertCalculationsAreLatest(actual, new List<CalculationDto>() { firstCalculation, secondCalculation });
    }

    [Fact]
    public void FindLatestCalculations_WithCalculationsOverlappingInTheBeginning_LatestCalculationsForEachDayInPeriod()
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

        var sut = new CalculationPeriodCalculator(_dateTimeZone);

        // Act
        var actual = sut
            .FindLatestCalculationsForPeriod(
                secondPeriodStart,
                firstPeriodEnd,
                new List<CalculationDto>() { firstCalculation, secondCalculation, });

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(2);
        AssertCalculationsCoversWholePeriod(actual, secondPeriodStart, firstPeriodEnd);
        AssertCalculationsAreLatest(actual, new List<CalculationDto>() { firstCalculation, secondCalculation });
    }

    [Fact]
    public void
        FindLatestCalculations_WithNewerCalculationInMiddleOfExistingCalculation_LatestCalculationsForEachDayInPeriod()
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

        var sut = new CalculationPeriodCalculator(_dateTimeZone);

        // Act
        var actual = sut
            .FindLatestCalculationsForPeriod(
                firstPeriodStart,
                firstPeriodEnd,
                new List<CalculationDto>() { firstCalculation, secondCalculation, });

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(3);
        AssertCalculationsCoversWholePeriod(actual, firstPeriodStart, firstPeriodEnd);
        AssertCalculationsAreLatest(actual, new List<CalculationDto>() { firstCalculation, secondCalculation });
    }

    [Fact]
    public void
        FindLatestCalculations_WithTwoNewerCalculationSeperatedOnTopExistingCalculation_LatestCalculationsForEachDayInPeriod()
    {
        // Arrange
        var firstPeriodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var firstPeriodEnd = Instant.FromUtc(2024, 1, 31, 23, 0, 0);
        var secondPeriodStart = Instant.FromUtc(2024, 1, 5, 23, 0, 0);
        var secondPeriodEnd = Instant.FromUtc(2024, 1, 10, 23, 0, 0);
        var thirdPeriodStart = Instant.FromUtc(2024, 1, 20, 23, 0, 0);
        var thirdPeriodEnd = Instant.FromUtc(2024, 1, 25, 23, 0, 0);
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
        var thirdCalculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(thirdPeriodStart)
            .WithPeriodEnd(thirdPeriodEnd)
            .WithVersion(3)
            .Build();

        var sut = new CalculationPeriodCalculator(_dateTimeZone);

        // Act
        var actual = sut
            .FindLatestCalculationsForPeriod(
                firstPeriodStart,
                firstPeriodEnd,
                new List<CalculationDto>() { firstCalculation, secondCalculation, thirdCalculation, });

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(5);
        AssertCalculationsCoversWholePeriod(actual, firstPeriodStart, firstPeriodEnd);
        AssertCalculationsAreLatest(
            actual,
            new List<CalculationDto>() { firstCalculation, secondCalculation, thirdCalculation, });
    }

    [Fact]
    public void FindLatestCalculations_WithMissingCalculationForPartOfThePeriod_ThrowsMissingCalculationException()
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

        var sut = new CalculationPeriodCalculator(_dateTimeZone);

        // Act
        var actual = () => sut
            .FindLatestCalculationsForPeriod(
                firstPeriodStart,
                secondPeriodEnd,
                new List<CalculationDto>() { firstCalculation, secondCalculation, });

        // Assert
        actual.Should().ThrowExactly<MissingCalculationException>();
    }

    [Fact]
    public void FindLatestCalculations_WithMissingCalculationForTheWholePeriod_LatestCalculationForPeriodsIsEmpty()
    {
        // Arrange
        var periodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var periodEnd = Instant.FromUtc(2024, 1, 14, 23, 0, 0);

        var sut = new CalculationPeriodCalculator(_dateTimeZone);

        // Act
        var actual = sut
            .FindLatestCalculationsForPeriod(
                periodStart,
                periodEnd,
                new List<CalculationDto>() { });

        // Assert
        actual.Should().BeEmpty();
    }

    private void AssertCalculationsAreLatest(IReadOnlyCollection<LatestCalculationForPeriod> actual, IReadOnlyCollection<CalculationDto> calculations)
    {
        foreach (var latestCalculation in actual.OrderByDescending(x => x.CalculationVersion))
        {
            calculations.Should().ContainSingle(x =>
                x.Version >= latestCalculation.CalculationVersion
                && x.PeriodStart.ToInstant() <= latestCalculation.PeriodStart
                && x.PeriodEnd.ToInstant() >= latestCalculation.PeriodEnd);
        }
    }

    private void AssertCalculationsCoversWholePeriod(IReadOnlyCollection<LatestCalculationForPeriod> actual, Instant periodStart, Instant periodEnd)
    {
        LatestCalculationForPeriod? lastCalculation = null;
        foreach (var calculationForPeriod in actual.OrderBy(x => x.PeriodStart))
        {
            if (lastCalculation == null)
            {
                calculationForPeriod.PeriodStart.Should().Be(periodStart);
            }
            else
            {
                calculationForPeriod.PeriodStart.Should().Be(lastCalculation.PeriodEnd.Plus(Duration.FromDays(1)));
            }

            lastCalculation = calculationForPeriod;
        }

        lastCalculation!.PeriodEnd.Should().Be(periodEnd);
    }
}
