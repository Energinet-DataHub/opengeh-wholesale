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

using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.Calculations;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Test.Core;
using Energinet.DataHub.Wholesale.Test.Core.Fixture.Database;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Calculations.IntegrationTests.Infrastructure.Persistence.Calculation;

public class CalculationRepositoryTests
    : IClassFixture<WholesaleDatabaseFixture<DatabaseContext>>, IAsyncLifetime
{
    private readonly WholesaleDatabaseManager<DatabaseContext> _databaseManager;

    public CalculationRepositoryTests(WholesaleDatabaseFixture<DatabaseContext> fixture)
    {
        _databaseManager = fixture.DatabaseManager;
    }

    public async Task InitializeAsync()
    {
        // Clear existing calculations from db
        await using var dbContext = _databaseManager.CreateDbContext();
        await dbContext.Calculations.ExecuteDeleteAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AddAsync_AddsCalculation()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var someGridAreasIds = new List<GridAreaCode> { new("004"), new("805") };
        var expectedCalculation = CreateCalculation(CalculationType.Aggregation, someGridAreasIds);
        expectedCalculation.MarkAsStarted();
        var sut = new CalculationRepository(writeContext);

        // Act
        await sut.AddAsync(expectedCalculation);
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.Calculations.SingleAsync(b => b.Id == expectedCalculation.Id);

        actual.Should().BeEquivalentTo(expectedCalculation);
        actual.GridAreaCodes.Should().BeEquivalentTo(someGridAreasIds);
        actual.CalculationType.Should().Be(CalculationType.Aggregation);
        actual.IsInternalCalculation.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_CalculationContainsExecutionTime()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var someGridAreasIds = new List<GridAreaCode> { new("004"), new("805") };
        var calculation = CreateCalculation(someGridAreasIds);
        var sut = new CalculationRepository(writeContext);
        calculation.MarkAsStarted();
        calculation.MarkAsCalculationJobSubmitted(new CalculationJobId(1), SystemClock.Instance.GetCurrentInstant());
        calculation.MarkAsCalculationJobPending();
        calculation.MarkAsCalculating();
        calculation.MarkAsCalculated(calculation.ExecutionTimeStart!.Value.Plus(Duration.FromDays(2))); // This call will ensure ExecutionTimeEnd is set
        calculation.ExecutionTimeEnd.Should().NotBeNull(); // Additional check
        calculation.ExecutionTimeStart.Should().NotBeNull(); // Additional check

        // Act
        await sut.AddAsync(calculation);
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.Calculations.SingleAsync(b => b.Id == calculation.Id);

        Assert.Equal(actual.ExecutionTimeStart, calculation.ExecutionTimeStart);
        Assert.Equal(actual.ExecutionTimeEnd, calculation.ExecutionTimeEnd);
    }

    [Fact]
    public async Task AddAsync_WhenExecutionTimeEndIsNull_CalculationExecutionTimeIsNull()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var someGridAreasIds = new List<GridAreaCode> { new("004"), new("805") };
        var calculation = CreateCalculation(someGridAreasIds);
        var sut = new CalculationRepository(writeContext);

        // Act
        await sut.AddAsync(calculation);
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.Calculations.SingleAsync(b => b.Id == calculation.Id);

        actual.Should().BeEquivalentTo(calculation);
        actual.GridAreaCodes.Should().BeEquivalentTo(someGridAreasIds);
        actual.ExecutionTimeEnd.Should().BeNull();
        actual.ExecutionTimeStart.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_HasGridAreaFilter_FiltersAsExpected()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var someGridAreasIds = new List<GridAreaCode> { new("004"), new("805") };
        var calculation = CreateCalculation(someGridAreasIds);
        var sut = new CalculationRepository(writeContext);
        await sut.AddAsync(calculation);
        await writeContext.SaveChangesAsync();

        // Act
        var actual = await sut.SearchAsync(
            new[] { new GridAreaCode("004") },
            Array.Empty<CalculationExecutionState>(),
            null,
            null,
            null,
            null,
            null);

        // Assert
        actual.Should().Contain(calculation);
    }

    [Fact]
    public async Task SearchAsync_HasCalculationExecutionStateFilter_FiltersAsExpected()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var someGridAreasIds = new List<GridAreaCode> { new("004"), new("805") };
        var calculation = CreateCalculation(someGridAreasIds);
        var sut = new CalculationRepository(writeContext);
        await sut.AddAsync(calculation);
        await writeContext.SaveChangesAsync();

        // Act
        var actual = await sut.SearchAsync(
            Array.Empty<GridAreaCode>(),
            new[] { CalculationExecutionState.Created },
            null,
            null,
            null,
            null,
            null);

        // Assert
        actual.Should().Contain(calculation);
    }

    [Theory]
    [InlineData("2021-12-31T23:00Z", "2022-01-31T23:00Z", true)] // Period overlaps calculation interval
    [InlineData("2022-01-01T23:00Z", "2022-01-30T23:00Z", true)] // Period inside of calculation interval
    [InlineData("2022-01-30T23:00Z", "2022-02-02T23:00Z", true)] // Period overlaps end of calculation interval
    [InlineData("2021-12-30T23:00Z", "2022-02-01T23:00Z", true)] // Period overlaps entire calculation interval
    [InlineData("2021-12-25T23:00Z", "2022-01-01T23:00Z", true)] // Period overlaps start of calculation interval
    [InlineData("2021-12-25T23:00Z", "2021-12-31T23:00Z", false)] // Period before start of calculation interval, complete miss
    [InlineData("2021-12-21T23:00Z", "2021-12-29T23:00Z", false)] // Period before start of calculation interval, complete miss
    [InlineData("2022-01-31T23:00Z", "2022-02-01T23:00Z", false)] // Period past end of calculation interval, complete miss
    public async Task SearchAsync_HasPeriodFilter_FiltersAsExpected(DateTimeOffset start, DateTimeOffset end, bool expected)
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var someGridAreasIds = new List<GridAreaCode> { new("004"), new("805") };
        var calculation = CreateCalculation(someGridAreasIds);
        var sut = new CalculationRepository(writeContext);
        await sut.AddAsync(calculation);
        await writeContext.SaveChangesAsync();

        // Act
        var actual = await sut.SearchAsync(
            Array.Empty<GridAreaCode>(),
            Array.Empty<CalculationExecutionState>(),
            null,
            null,
            Instant.FromDateTimeOffset(start),
            Instant.FromDateTimeOffset(end),
            null);

        // Assert
        if (expected)
            actual.Should().Contain(calculation);
        else
            actual.Should().NotContain(calculation);
    }

    [Theory]
    [InlineData("2022-04-01T23:00Z", "2022-06-01T23:00Z", true)] // Execution time inside period
    [InlineData("2022-05-02T23:00Z", "2022-06-01T23:00Z", false)] // Execution time before period
    [InlineData("2022-01-01T23:00Z", "2022-04-30T23:00Z", false)] // Execution time after period
    public async Task SearchAsync_HasExecutionTimeFilter_FiltersAsExpected(DateTimeOffset start, DateTimeOffset end, bool expected)
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();

        var period = Periods.January_EuropeCopenhagen_Instant;
        var executionTimeStart = Instant.FromUtc(2022, 5, 1, 0, 0);
        var calculation = new Application.Model.Calculations.Calculation(
            SystemClock.Instance.GetCurrentInstant(),
            CalculationType.BalanceFixing,
            new List<GridAreaCode> { new("004") },
            period.PeriodStart,
            period.PeriodEnd,
            Instant.FromUtc(2022, 4, 4, 0, 0),
            period.DateTimeZone,
            Guid.NewGuid(),
            SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().Ticks,
            false);
        calculation.MarkAsStarted();
        calculation.MarkAsCalculationJobSubmitted(new CalculationJobId(1), executionTimeStart); // Sets execution time start

        var sut = new CalculationRepository(writeContext);
        await sut.AddAsync(calculation);
        await writeContext.SaveChangesAsync();

        // Act
        var actual = await sut.SearchAsync(
            Array.Empty<GridAreaCode>(),
            Array.Empty<CalculationExecutionState>(),
            Instant.FromDateTimeOffset(start),
            Instant.FromDateTimeOffset(end),
            null,
            null,
            null);

        // Assert
        if (expected)
            actual.Should().Contain(calculation);
        else
            actual.Should().NotContain(calculation);
    }

    [Fact]
    public async Task SearchAsync_HasCalculationTypeFilter_FiltersAsExpected()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var someGridAreasIds = new List<GridAreaCode> { new("004"), new("805") };
        var calculation = CreateCalculation(someGridAreasIds);
        var sut = new CalculationRepository(writeContext);
        await sut.AddAsync(calculation);
        await writeContext.SaveChangesAsync();

        // Act
        var actual = await sut.SearchAsync(
            Array.Empty<GridAreaCode>(),
            Array.Empty<CalculationExecutionState>(),
            null,
            null,
            null,
            null,
            CalculationType.BalanceFixing);

        // Assert
        actual.Should().Contain(calculation);
    }

    /// <summary>
    /// When a calculation is scheduled to run later, the execution time start is not set before the calculation
    /// is started. In this case the filter looks at when the calculation is scheduled to run instead.
    /// </summary>
    [Fact]
    public async Task SearchAsync_WhenNoExecutionTimeStartButScheduledAtIsInFilterRange_CalculationIsReturned()
    {
        // Arrange
        var now = Instant.FromUtc(2024, 08, 21, 13, 37);
        var tomorrow = now.Plus(Duration.FromDays(1));
        var nextMonth = now.Plus(Duration.FromDays(31));
        var scheduledCalculation = CreateCalculation(scheduledAt: tomorrow);
        await using (var writeContext = _databaseManager.CreateDbContext())
        {
            var calculationRepository = new CalculationRepository(writeContext);
            await calculationRepository.AddAsync(scheduledCalculation);
            await writeContext.SaveChangesAsync();
        }

        var dbContext = _databaseManager.CreateDbContext();
        var sut = new CalculationRepository(dbContext);

        // Act
        var actual = await sut.SearchAsync(
            Array.Empty<GridAreaCode>(),
            Array.Empty<CalculationExecutionState>(),
            minExecutionTimeStart: now,
            maxExecutionTimeStart: nextMonth,
            null,
            null,
            null);

        // Assert
        // => Since the calculation is scheduled to run tomorrow, we expect the calculation to be returned
        actual.Should().BeEquivalentTo([scheduledCalculation]);
    }

    /// <summary>
    /// When a calculation is scheduled to run later, the execution time start is not set before the calculation
    /// is started. In this case the filter looks at when the calculation is scheduled to run instead.
    /// </summary>
    [Fact]
    public async Task SearchAsync_WhenNoExecutionTimeStartButScheduledAtIsOutsideFilterRange_NoCalculationsReturned()
    {
        // Arrange
        var today = Instant.FromUtc(2024, 08, 21, 13, 37);
        var yesterday = today.Minus(Duration.FromDays(1));
        var tomorrow = today.Plus(Duration.FromDays(1));
        var nextMonth = today.Plus(Duration.FromDays(31));
        var calculationScheduledYesterday = CreateCalculation(scheduledAt: yesterday);
        var calculationScheduledNextMonth = CreateCalculation(scheduledAt: nextMonth);
        await using (var writeContext = _databaseManager.CreateDbContext())
        {
            var calculationRepository = new CalculationRepository(writeContext);
            await calculationRepository.AddAsync(calculationScheduledYesterday);
            await calculationRepository.AddAsync(calculationScheduledNextMonth);
            await writeContext.SaveChangesAsync();
        }

        var dbContext = _databaseManager.CreateDbContext();
        var sut = new CalculationRepository(dbContext);

        // Act
        var actual = await sut.SearchAsync(
            Array.Empty<GridAreaCode>(),
            Array.Empty<CalculationExecutionState>(),
            minExecutionTimeStart: today,
            maxExecutionTimeStart: tomorrow,
            null,
            null,
            null);

        // Assert
        // => Since the filter is from today to tomorrow, we don't expect
        // the calculations scheduled yesterday or next month to be returned
        actual.Should().BeEmpty();
    }

    [Fact]
    public async Task GetScheduledCalculationsAsync_ReturnsScheduledCalculations()
    {
        // Arrange
        var now = Instant.FromUtc(2024, 08, 16, 13, 37);
        var inThePast = now.Minus(Duration.FromMilliseconds(1));
        var inTheFuture = now.Plus(Duration.FromMilliseconds(1));

        var expectedCalculation1 = CreateCalculation(scheduledAt: now);
        var expectedCalculation2 = CreateCalculation(scheduledAt: inThePast);

        var futureCalculation = CreateCalculation(scheduledAt: inTheFuture);

        var startedCalculation1 = CreateCalculation(scheduledAt: inThePast);
        startedCalculation1.MarkAsStarted();

        var startedCalculation2 = CreateCalculation(scheduledAt: inThePast);
        startedCalculation2.MarkAsStarted();
        startedCalculation2.MarkAsCalculationJobSubmitted(new CalculationJobId(1), executionTimeStart: inThePast);
        startedCalculation2.MarkAsCalculating();

        var finishedCalculation = CreateCalculation(scheduledAt: inThePast);
        finishedCalculation.MarkAsStarted();
        finishedCalculation.MarkAsCalculationJobSubmitted(new CalculationJobId(2), executionTimeStart: inThePast);
        finishedCalculation.MarkAsCalculated(executionTimeEnd: inThePast);
        finishedCalculation.MarkAsActorMessagesEnqueuing(enqueuingTimeStart: inThePast);
        finishedCalculation.MarkAsActorMessagesEnqueued(enqueuedTimeEnd: inThePast);
        finishedCalculation.MarkAsCompleted(completedAt: inThePast);

        await using var writeContext = _databaseManager.CreateDbContext();
        {
            var repository = new CalculationRepository(writeContext);

            await repository.AddAsync(expectedCalculation1);
            await repository.AddAsync(expectedCalculation2);
            await repository.AddAsync(futureCalculation);
            await repository.AddAsync(startedCalculation1);
            await repository.AddAsync(startedCalculation2);
            await repository.AddAsync(finishedCalculation);

            await writeContext.SaveChangesAsync();
        }

        await using var readContext = _databaseManager.CreateDbContext();
        var sut = new CalculationRepository(readContext);

        // Act
        var scheduledCalculations = await sut.GetScheduledCalculationsAsync(scheduledToRunBefore: now);

        // Assert
        using var assertionScope = new AssertionScope();
        scheduledCalculations.Should().Satisfy(
            sc => sc.CalculationId.Id == expectedCalculation1.Id &&
                  sc.OrchestrationInstanceId == expectedCalculation1.OrchestrationInstanceId,
            sc => sc.CalculationId.Id == expectedCalculation2.Id &&
                  sc.OrchestrationInstanceId == expectedCalculation2.OrchestrationInstanceId);
    }

    [Fact]
    public async Task GetScheduledCalculationsAsync_WhenCalculationIsCanceled_ReturnsNoScheduledCalculations()
    {
        // Arrange
        var now = Instant.FromUtc(2024, 08, 16, 13, 37);
        var inThePast = now.Minus(Duration.FromMilliseconds(1));

        var canceledCalculation = CreateCalculation(scheduledAt: inThePast);
        canceledCalculation.MarkAsCanceled(Guid.NewGuid());

        await using var writeContext = _databaseManager.CreateDbContext();
        {
            var repository = new CalculationRepository(writeContext);
            await repository.AddAsync(canceledCalculation);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = _databaseManager.CreateDbContext();
        var sut = new CalculationRepository(readContext);

        // Act
        var scheduledCalculations = await sut.GetScheduledCalculationsAsync(scheduledToRunBefore: now);

        // Assert
        using var assertionScope = new AssertionScope();
        scheduledCalculations.Should().BeEmpty();
    }

    private static Application.Model.Calculations.Calculation CreateCalculation(List<GridAreaCode> someGridAreasIds)
    {
        return CreateCalculation(CalculationType.BalanceFixing, someGridAreasIds);
    }

    private static Application.Model.Calculations.Calculation CreateCalculation(
        CalculationType calculationType,
        List<GridAreaCode> someGridAreasIds,
        Instant? scheduledAt = null)
    {
        var period = Periods.January_EuropeCopenhagen_Instant;
        return new Application.Model.Calculations.Calculation(
            SystemClock.Instance.GetCurrentInstant(),
            calculationType,
            someGridAreasIds,
            period.PeriodStart,
            period.PeriodEnd,
            scheduledAt ?? SystemClock.Instance.GetCurrentInstant(),
            period.DateTimeZone,
            Guid.NewGuid(),
            SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().Ticks,
            false);
    }

    private static Application.Model.Calculations.Calculation CreateCalculation(Instant scheduledAt)
    {
        return CreateCalculation(
            calculationType: CalculationType.BalanceFixing,
            someGridAreasIds: [new GridAreaCode("001")],
            scheduledAt: scheduledAt);
    }
}
