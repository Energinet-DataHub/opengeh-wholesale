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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using Moq;
using NodaTime;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Calculations.UnitTests.Infrastructure.CalculationAggregate;

public class CalculationTests
{
    [Fact]
    public void Ctor_CreatesImmutableGridAreaCodes()
    {
        // Arrange
        var someGridAreaCodes = new List<GridAreaCode> { new("004"), new("805") };
        var sut = new CalculationBuilder().WithGridAreaCodes(someGridAreaCodes).Build();

        // Act
        var unexpectedGridAreaCode = new GridAreaCode("777");
        someGridAreaCodes.Add(unexpectedGridAreaCode);

        // Assert
        sut.GridAreaCodes.Should().NotContain(unexpectedGridAreaCode);
    }

    [Fact]
    public void Ctor_WhenNoGridAreaCodes_ThrowsBusinessValidationException()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        var emptyGridAreaCodes = new List<GridAreaCode>();
        var actual = Assert.Throws<BusinessValidationException>(() => new Calculation(
            SystemClock.Instance.GetCurrentInstant(),
            CalculationType.BalanceFixing,
            emptyGridAreaCodes,
            Instant.FromDateTimeOffset(DateTimeOffset.Now),
            Instant.FromDateTimeOffset(DateTimeOffset.Now),
            SystemClock.Instance.GetCurrentInstant(),
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!,
            Guid.NewGuid(),
            SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().Ticks));
        actual.Message.Should().Contain("Calculation must contain at least one grid area code");
    }

    [Theory]
    [InlineAutoMoqData(CalculationType.WholesaleFixing)]
    [InlineAutoMoqData(CalculationType.FirstCorrectionSettlement)]
    [InlineAutoMoqData(CalculationType.SecondCorrectionSettlement)]
    [InlineAutoMoqData(CalculationType.ThirdCorrectionSettlement)]
    public void Ctor_WhenWholesaleAndCorrectionCalculationTypesAndPeriodIsMoreThanAMonth_ThrowsBusinessValidationException(CalculationType calculationType)
    {
        // Arrange & Act
        var actual = Assert.Throws<BusinessValidationException>(() => new CalculationBuilder()
            .WithCalculationType(calculationType)
            .WithPeriodEnd(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(32)))
            .Build());

        // Assert
        actual.Message.Should().Contain($"The period (start: 2021-12-31T23:00:00Z end: 2022-02-01T23:00:00Z) has to be an entire month when using calculation type {calculationType}");
    }

    [Theory]
    [InlineAutoMoqData(CalculationType.WholesaleFixing, 30, false)]
    [InlineAutoMoqData(CalculationType.WholesaleFixing, 31, true)]
    [InlineAutoMoqData(CalculationType.WholesaleFixing, 32, false)]
    [InlineAutoMoqData(CalculationType.FirstCorrectionSettlement, 30, false)]
    [InlineAutoMoqData(CalculationType.FirstCorrectionSettlement, 31, true)]
    [InlineAutoMoqData(CalculationType.FirstCorrectionSettlement, 32, false)]
    [InlineAutoMoqData(CalculationType.SecondCorrectionSettlement, 30, false)]
    [InlineAutoMoqData(CalculationType.SecondCorrectionSettlement, 31, true)]
    [InlineAutoMoqData(CalculationType.SecondCorrectionSettlement, 32, false)]
    [InlineAutoMoqData(CalculationType.ThirdCorrectionSettlement, 30, false)]
    [InlineAutoMoqData(CalculationType.ThirdCorrectionSettlement, 31, true)]
    [InlineAutoMoqData(CalculationType.ThirdCorrectionSettlement, 32, false)]
    [InlineAutoMoqData(CalculationType.BalanceFixing, 30, true)]
    [InlineAutoMoqData(CalculationType.Aggregation, 30, true)]
    public void Ctor_PeriodsCombinedWithCalculationTypes_AreValidOrInvalid(CalculationType calculationType, int days, bool isValid)
    {
        // Arrange & Act
        var calculationBuilder = new CalculationBuilder()
            .WithCalculationType(calculationType)
            .WithPeriodEnd(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(days)));

        // Act
        var actual = Record.Exception(() => calculationBuilder.Build());

        // Assert
        Assert.Equal(isValid, actual == null);
    }

    [Theory]
    [InlineData("2022-12-31T23:00Z", "2022-01-30T23:00Z", false)] // Does not include last day of the month
    [InlineData("2022-01-01T23:00Z", "2022-01-31T23:00Z", false)] // Does not include first day of the month
    [InlineData("2022-11-30T23:00Z", "2022-01-31T23:00Z", false)] // Two months
    [InlineData("2021-12-31T23:00Z", "2022-01-31T23:00Z", true)] // January
    [InlineData("2022-01-31T23:00Z", "2022-02-28T23:00Z", true)] // February
    [InlineData("2022-02-28T23:00Z", "2022-03-31T22:00Z", true)] // March
    [InlineData("2022-03-31T22:00Z", "2022-04-30T22:00Z", true)] // April
    [InlineData("2022-04-30T22:00Z", "2022-05-31T22:00Z", true)] // May
    [InlineData("2022-05-31T22:00Z", "2022-06-30T22:00Z", true)] // June
    [InlineData("2022-06-30T22:00Z", "2022-07-31T22:00Z", true)] // July
    [InlineData("2022-07-31T22:00Z", "2022-08-31T22:00Z", true)] // August
    [InlineData("2022-08-31T22:00Z", "2022-09-30T22:00Z", true)] // September
    [InlineData("2022-09-30T22:00Z", "2022-10-31T23:00Z", true)] // October
    [InlineData("2022-10-31T23:00Z", "2022-11-30T23:00Z", true)] // November
    [InlineData("2022-11-30T23:00Z", "2022-12-31T23:00Z", true)] // December
    public void Ctor_WhenWholesaleFixingPeriodIsNotEntireMonth_ThrowsBusinessValidationException(DateTimeOffset startDate, DateTimeOffset endDate, bool isEntireMonth)
    {
        // Arrange
        var someGridAreas = new List<GridAreaCode> { new("004"), new("805") };

        // Act
        var createCalculation = () => new Calculation(
            SystemClock.Instance.GetCurrentInstant(),
            CalculationType.WholesaleFixing,
            someGridAreas,
            Instant.FromDateTimeOffset(startDate),
            Instant.FromDateTimeOffset(endDate),
            SystemClock.Instance.GetCurrentInstant(),
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!,
            Guid.NewGuid(),
            SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().Ticks);

        // Assert
        if (isEntireMonth)
        {
            createCalculation();
        }
        else
        {
            Assert.Throws<BusinessValidationException>(createCalculation);
        }
    }

    [Fact]
    public void Ctor_SetsExecutionTimeEndToNull()
    {
        var sut = new CalculationBuilder().WithStatePending().Build();
        sut.ExecutionTimeEnd.Should().BeNull();
    }

    [Fact]
    public void Ctor_ExecutionTimeStartNotNull()
    {
        var sut = new CalculationBuilder().WithStatePending().Build();
        sut.ExecutionTimeStart.Should().NotBeNull();
    }

    [Fact]
    public void GetResolution_DoesNotThrowExceptionForAllCalculationTypes()
    {
        // Arrange
        foreach (var calculationType in Enum.GetValues(typeof(CalculationType)))
        {
            var sut = new CalculationBuilder().WithCalculationType((CalculationType)calculationType).Build();

            // Act & Assert
            sut.GetResolution();
        }
    }

    [Fact]
    public void GetQuantityUnit_DoesNotThrowExceptionForAllCalculationTypes()
    {
        // Arrange
        foreach (var calculationType in Enum.GetValues(typeof(CalculationType)))
        {
            var sut = new CalculationBuilder().WithCalculationType((CalculationType)calculationType).Build();

            // Act & Assert - Remember to add new [InlineAutoMoqData (...,...)] for new calculation types in other tests
            sut.GetQuantityUnit();
        }
    }

    [Theory]
    [InlineAutoMoqData(CalculationType.BalanceFixing, "PT15M")]
    [InlineAutoMoqData(CalculationType.Aggregation, "PT15M")]
    [InlineAutoMoqData(CalculationType.WholesaleFixing, "PT15M")]
    [InlineAutoMoqData(CalculationType.FirstCorrectionSettlement, "PT15M")]
    [InlineAutoMoqData(CalculationType.SecondCorrectionSettlement, "PT15M")]
    [InlineAutoMoqData(CalculationType.ThirdCorrectionSettlement, "PT15M")]
    public void GetResolution_ReturnsExpectedIso8601Duration(CalculationType calculationType, string expectedIso8601Duration)
    {
        // Arrange
        var sut = new CalculationBuilder().WithCalculationType(calculationType).Build();

        // Act
        var actual = sut.GetResolution();

        // Assert
        actual.Should().Be(expectedIso8601Duration);
    }

    [Theory]
    [InlineAutoMoqData(CalculationType.BalanceFixing, QuantityUnit.Kwh)]
    [InlineAutoMoqData(CalculationType.Aggregation, QuantityUnit.Kwh)]
    [InlineAutoMoqData(CalculationType.WholesaleFixing, QuantityUnit.Kwh)]
    [InlineAutoMoqData(CalculationType.FirstCorrectionSettlement, QuantityUnit.Kwh)]
    [InlineAutoMoqData(CalculationType.SecondCorrectionSettlement, QuantityUnit.Kwh)]
    [InlineAutoMoqData(CalculationType.ThirdCorrectionSettlement, QuantityUnit.Kwh)]
    public void GetQuantityUnit_ReturnsExpectedIso8601Duration(CalculationType calculationType, QuantityUnit expectedQuantityUnit)
    {
        // Arrange
        var sut = new CalculationBuilder().WithCalculationType(calculationType).Build();

        // Act
        var actual = sut.GetQuantityUnit();

        // Assert
        actual.Should().Be(expectedQuantityUnit);
    }

    [Fact]
    public void MarkAsCalculated_WhenComplete_ThrowsBusinessValidationException()
    {
        var sut = new CalculationBuilder().WithStateCompleted().Build();
        Assert.Throws<BusinessValidationException>(() => sut.MarkAsCalculated(It.IsAny<Instant>()));
    }

    [Theory]
    [InlineData("2023-01-31T23:00:00.0001Z", "Europe/Copenhagen")]
    [InlineData("2023-01-31T22:59:59Z", "Europe/Copenhagen")]
    [InlineData("2023-01-31T22:59:59.9999999Z", "Europe/Copenhagen")]
    [InlineData("2023-01-31T23:00:00Z", "Asia/Tokyo")]
    public void Ctor_WhenPeriodEndIsNotMidnight_ThrowsBusinessValidationException(string periodEndString, string timeZoneId)
    {
        // Arrange
        var periodEnd = DateTimeOffset.Parse(periodEndString).ToInstant();
        var gridAreaCode = new GridAreaCode("113");

        // Act
        var actual = Assert.Throws<BusinessValidationException>(() => new Calculation(
            SystemClock.Instance.GetCurrentInstant(),
            CalculationType.BalanceFixing,
            new List<GridAreaCode> { gridAreaCode },
            Instant.MinValue,
            periodEnd,
            SystemClock.Instance.GetCurrentInstant(),
            DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId)!,
            Guid.NewGuid(),
            SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().Ticks));

        // Assert
        actual.Message.Should().ContainAll("period", "end");
    }

    [Theory]
    [InlineData("2023-01-31T22:59:00.9999999Z", "Europe/Copenhagen")]
    [InlineData("2023-01-31T23:00:00.9999999Z", "Europe/Copenhagen")]
    [InlineData("2023-01-31T23:00:00Z", "America/Cayman")]
    public void Ctor_WhenPeriodStartIsNotMidnight_ThrowsBusinessValidationException(string periodStartString, string timeZoneId)
    {
        // Arrange
        var startPeriod = DateTimeOffset.Parse(periodStartString).ToInstant();
        var gridAreaCode = new GridAreaCode("113");

        // Act
        var actual = Assert.Throws<BusinessValidationException>(() => new Calculation(
            SystemClock.Instance.GetCurrentInstant(),
            CalculationType.BalanceFixing,
            new List<GridAreaCode> { gridAreaCode },
            startPeriod,
            Instant.FromDateTimeOffset(new DateTimeOffset(2023, 02, 01, 23, 0, 0, new TimeSpan(0))),
            SystemClock.Instance.GetCurrentInstant(),
            DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId)!,
            Guid.NewGuid(),
            SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().Ticks));

        // Assert
        actual.Message.Should().Contain($"The period start '{startPeriod.ToString()}' must be midnight.");
    }

    [Fact]
    public void MarkAsCalculated_WhenExecuting_CompletesCalculation()
    {
        // Arrange
        var sut = new CalculationBuilder().WithStateExecuting().Build();
        var executionTimeEndGreaterThanStart = sut.ExecutionTimeStart!.Value.Plus(Duration.FromDays(2));

        // Act
        sut.MarkAsCalculated(executionTimeEndGreaterThanStart);

        // Assert
        sut.ExecutionState.Should().Be(CalculationExecutionState.Completed);
    }

    [Fact]
    public void MarkAsCalculated_SetsExecutionTimeEnd()
    {
        // Arrange
        var sut = new CalculationBuilder().WithStateExecuting().Build();
        var executionTimeEndGreaterThanStart = sut.ExecutionTimeStart!.Value.Plus(Duration.FromDays(2));

        // Act
        sut.MarkAsCalculated(executionTimeEndGreaterThanStart);

        // Assert
        sut.ExecutionTimeEnd.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsCalculated_WhenExecutionTimeEndLessThanStart_ThrowsBusinessValidationException()
    {
        // Arrange
        var sut = new CalculationBuilder().WithStateExecuting().Build();
        var executionTimeEndLessThanStart = sut.ExecutionTimeStart!.Value.Minus(Duration.FromDays(2));

        // Act
        var actual = Assert.Throws<BusinessValidationException>(() => sut.MarkAsCalculated(executionTimeEndLessThanStart));

        // Assert
        sut.ExecutionTimeEnd.Should().BeNull();
        actual.Message.Should().Contain("cannot be before execution time start");
    }

    [Fact]
    public void MarkAsCalculating_WhenExecuting_ThrowsBusinessValidationException()
    {
        var sut = new CalculationBuilder().WithStateExecuting().Build();
        Assert.Throws<BusinessValidationException>(() => sut.MarkAsCalculating());
    }

    [Fact]
    public void MarkAsCalculating_WhenComplete_ThrowsBusinessValidationException()
    {
        var sut = new CalculationBuilder().WithStateCompleted().Build();
        Assert.Throws<BusinessValidationException>(() => sut.MarkAsCalculating());
    }

    [Fact]
    public void MarkAsCalculating_WhenPending_ExecutesCalculation()
    {
        var sut = new CalculationBuilder().WithStatePending().Build();
        sut.MarkAsCalculating();
        sut.ExecutionState.Should().Be(CalculationExecutionState.Executing);
    }

    [Fact]
    public void MarkAsCalculating_ExecutionTimeIsSetToNull()
    {
        var sut = new CalculationBuilder().WithStatePending().Build();
        sut.MarkAsCalculating();
        sut.ExecutionTimeEnd.Should().BeNull();
    }

    [Fact]
    public void Reset_WhenSubmitted_SetsStateCreated()
    {
        var sut = new CalculationBuilder().WithStateSubmitted().Build();
        sut.Reset();
        sut.ExecutionState.Should().Be(CalculationExecutionState.Created);
    }

    [Fact]
    public void Reset_WhenPending_SetsStateCreated()
    {
        var sut = new CalculationBuilder().WithStatePending().Build();
        sut.Reset();
        sut.ExecutionState.Should().Be(CalculationExecutionState.Created);
    }

    [Fact]
    public void Reset_WhenExecuting_SetsStateCreated()
    {
        var sut = new CalculationBuilder().WithStateExecuting().Build();
        sut.Reset();
        sut.ExecutionState.Should().Be(CalculationExecutionState.Created);
    }

    [Fact]
    public void Reset_WhenCompleted_ThrowsBusinessValidationException()
    {
        var sut = new CalculationBuilder().WithStateCompleted().Build();
        Assert.Throws<BusinessValidationException>(() => sut.Reset());
    }

    [Theory]
    [InlineData(CalculationExecutionState.Created)]
    [InlineData(CalculationExecutionState.Submitted)]
    [InlineData(CalculationExecutionState.Pending)]
    [InlineData(CalculationExecutionState.Executing)]
    [InlineData(CalculationExecutionState.Failed)]
    public void Reset_WhenGivenExecutionState_SetsOrchestrationStateScheduled(CalculationExecutionState givenExecutionState)
    {
        var sut = new CalculationBuilder()
            .WithState(givenExecutionState)
            .Build();

        sut.Reset();

        sut.OrchestrationState.Should().Be(CalculationOrchestrationState.Scheduled);
    }

    [Theory]
    [InlineData(CalculationExecutionState.Created, CalculationOrchestrationState.Scheduled)]
    [InlineData(CalculationExecutionState.Submitted, CalculationOrchestrationState.Scheduled)]
    [InlineData(CalculationExecutionState.Pending, CalculationOrchestrationState.Scheduled)]
    [InlineData(CalculationExecutionState.Executing, CalculationOrchestrationState.Calculating)]
    [InlineData(CalculationExecutionState.Completed, CalculationOrchestrationState.Calculated)]
    [InlineData(CalculationExecutionState.Failed, CalculationOrchestrationState.CalculationFailed)]
    public void WhenUpdatingExecutionState_SetsExpectedOrchestrationState(CalculationExecutionState executionState, CalculationOrchestrationState expectedOrchestrationState)
    {
        var sut = new CalculationBuilder().Build();

        var calculationJobId = new CalculationJobId(1);
        switch (executionState)
        {
            case CalculationExecutionState.Created:
                break;
            case CalculationExecutionState.Submitted:
                sut.MarkAsSubmitted(calculationJobId);
                break;
            case CalculationExecutionState.Pending:
                sut.MarkAsSubmitted(calculationJobId);
                sut.MarkAsScheduled();
                break;
            case CalculationExecutionState.Executing:
                sut.MarkAsSubmitted(calculationJobId);
                sut.MarkAsScheduled();
                sut.MarkAsCalculating();
                break;
            case CalculationExecutionState.Completed:
                sut.MarkAsSubmitted(calculationJobId);
                sut.MarkAsScheduled();
                sut.MarkAsCalculating();
                sut.MarkAsCalculated(sut.ExecutionTimeStart!.Value.Plus(Duration.FromMinutes(15)));
                break;
            case CalculationExecutionState.Failed:
                sut.MarkAsSubmitted(calculationJobId);
                sut.MarkAsScheduled();
                sut.MarkAsCalculating();
                sut.MarkAsCalculationFailed();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(executionState), executionState, "The given execution state is not supported in the test");
        }

        sut.OrchestrationState.Should().Be(expectedOrchestrationState);
    }
}
