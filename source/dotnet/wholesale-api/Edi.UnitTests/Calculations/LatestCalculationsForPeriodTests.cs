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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Edi.Calculations;
using Energinet.DataHub.Wholesale.Edi.Exceptions;
using Energinet.DataHub.Wholesale.EDI.Models;
using Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;
using FluentAssertions;
using FluentAssertions.Execution;
using NodaTime;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Calculations;

public class LatestCalculationsForPeriodTests
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

        var sut = new LatestCalculationsForPeriod(_dateTimeZone);

        // Act
        var actual = sut
            .FindLatestCalculationsForPeriod(periodStart, periodEnd, new List<CalculationDto>() { calculation });

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(1);
        AssertCalculationsCoversWholePeriod(actual, periodStart, periodEnd);
        actual.Should().ContainSingle(c => c.Period.Start.InUtc().Day == 1 && c.Period.End.InUtc().Day == 15)
            .Which.CalculationId.Should().Be(calculation.CalculationId);
    }

    [Fact]
    public void FindLatestCalculations_WithACalculationLargerThenRequestedPeriod_LatestCalculationsForPeriod2()
    {
        // Arrange
        var calculationPeriodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var calculationPeriodEnd = Instant.FromUtc(2024, 1, 15, 23, 0, 0);
        var periodStart = Instant.FromUtc(2024, 1, 5, 23, 0, 0);
        var periodEnd = Instant.FromUtc(2024, 1, 10, 23, 0, 0);
        var calculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(calculationPeriodStart)
            .WithPeriodEnd(calculationPeriodEnd)
            .Build();

        var sut = new LatestCalculationsForPeriod(_dateTimeZone);

        // Act
        var actual = sut
            .FindLatestCalculationsForPeriod(periodStart, periodEnd, new List<CalculationDto>() { calculation });

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(1);
        AssertCalculationsCoversWholePeriod(actual, periodStart, periodEnd);
        actual.Should().ContainSingle(c => c.Period.Start == periodStart && c.Period.End == periodEnd)
            .Which.CalculationId.Should().Be(calculation.CalculationId);
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

        var sut = new LatestCalculationsForPeriod(_dateTimeZone);

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
        actual.Should().ContainSingle(c => c.Period.Start.InUtc().Day == 1 && c.Period.End.InUtc().Day == 15)
            .Which.CalculationId.Should().Be(firstCalculation.CalculationId);
        actual.Should().ContainSingle(c => c.Period.Start.InUtc().Day == 16 && c.Period.End.InUtc().Day == 31)
            .Which.CalculationId.Should().Be(secondCalculation.CalculationId);
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

        var sut = new LatestCalculationsForPeriod(_dateTimeZone);

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
        actual.Should().ContainSingle(c => c.Period.Start.InUtc().Day == 1 && c.Period.End.InUtc().Day == 9)
            .Which.CalculationId.Should().Be(firstCalculation.CalculationId);
        actual.Should().ContainSingle(c => c.Period.Start.InUtc().Day == 10 && c.Period.End.InUtc().Day == 31)
            .Which.CalculationId.Should().Be(secondCalculation.CalculationId);
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

        var sut = new LatestCalculationsForPeriod(_dateTimeZone);

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
        actual.Should().ContainSingle(c => c.Period.Start.InUtc().Day == 10 && c.Period.End.InUtc().Day == 31)
            .Which.CalculationId.Should().Be(firstCalculation.CalculationId);
        actual.Should().ContainSingle(c => c.Period.Start.InUtc().Day == 1 && c.Period.End.InUtc().Day == 9)
            .Which.CalculationId.Should().Be(secondCalculation.CalculationId);
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

        var sut = new LatestCalculationsForPeriod(_dateTimeZone);

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
        actual.Should().ContainSingle(c => c.Period.Start.InUtc().Day == 1 && c.Period.End.InUtc().Day == 9)
            .Which.CalculationId.Should().Be(firstCalculation.CalculationId);
        actual.Should().ContainSingle(c => c.Period.Start.InUtc().Day == 10 && c.Period.End.InUtc().Day == 20)
            .Which.CalculationId.Should().Be(secondCalculation.CalculationId);
        actual.Should().ContainSingle(c => c.Period.Start.InUtc().Day == 21 && c.Period.End.InUtc().Day == 31)
            .Which.CalculationId.Should().Be(firstCalculation.CalculationId);
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

        var sut = new LatestCalculationsForPeriod(_dateTimeZone);

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

        actual.Should().ContainSingle(c => c.Period.Start.InUtc().Day == 1 && c.Period.End.InUtc().Day == 4)
            .Which.CalculationId.Should().Be(firstCalculation.CalculationId);
        actual.Should().ContainSingle(c => c.Period.Start.InUtc().Day == 5 && c.Period.End.InUtc().Day == 10)
            .Which.CalculationId.Should().Be(secondCalculation.CalculationId);
        actual.Should().ContainSingle(c => c.Period.Start.InUtc().Day == 11 && c.Period.End.InUtc().Day == 19)
            .Which.CalculationId.Should().Be(firstCalculation.CalculationId);
        actual.Should().ContainSingle(c => c.Period.Start.InUtc().Day == 20 && c.Period.End.InUtc().Day == 25)
            .Which.CalculationId.Should().Be(thirdCalculation.CalculationId);
        actual.Should().ContainSingle(c => c.Period.Start.InUtc().Day == 26 && c.Period.End.InUtc().Day == 31)
            .Which.CalculationId.Should().Be(firstCalculation.CalculationId);
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

        var sut = new LatestCalculationsForPeriod(_dateTimeZone);

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

        var sut = new LatestCalculationsForPeriod(_dateTimeZone);

        // Act
        var actual = sut
            .FindLatestCalculationsForPeriod(
                periodStart,
                periodEnd,
                new List<CalculationDto>() { });

        // Assert
        actual.Should().BeEmpty();
    }

    private void AssertCalculationsCoversWholePeriod(IReadOnlyCollection<CalculationForPeriod> actual, Instant periodStart, Instant periodEnd)
    {
        CalculationForPeriod? lastCalculation = null;
        foreach (var calculationForPeriod in actual.OrderBy(x => x.Period.Start))
        {
            if (lastCalculation == null)
            {
                calculationForPeriod.Period.Start.Should().Be(periodStart);
            }
            else
            {
                calculationForPeriod.Period.Start.Should().Be(lastCalculation.Period.End.Plus(Duration.FromDays(1)));
            }

            lastCalculation = calculationForPeriod;
        }

        lastCalculation!.Period.End.Should().Be(periodEnd);
    }
}
