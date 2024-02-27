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

using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.Calculations;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Test.Core;
using Test.Core.Fixture.Database;
using Xunit;

namespace Energinet.DataHub.Wholesale.Calculations.IntegrationTests.Infrastructure.Persistence.Calculation;

public class CalculationRepositoryTests : IClassFixture<WholesaleDatabaseFixture<DatabaseContext>>
{
    private readonly WholesaleDatabaseManager<DatabaseContext> _databaseManager;

    public CalculationRepositoryTests(WholesaleDatabaseFixture<DatabaseContext> fixture)
    {
        _databaseManager = fixture.DatabaseManager;
    }

    [Fact]
    public async Task AddAsync_AddsCalculation()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var someGridAreasIds = new List<GridAreaCode> { new("004"), new("805") };
        var expectedCalculation = CreateCalculation(CalculationType.Aggregation, someGridAreasIds);
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
    }

    [Fact]
    public async Task AddAsync_CalculationContainsExecutionTime()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var someGridAreasIds = new List<GridAreaCode> { new("004"), new("805") };
        var calculation = CreateCalculation(someGridAreasIds);
        var sut = new CalculationRepository(writeContext);
        calculation.MarkAsExecuting(); // This call will ensure ExecutionTimeStart is set
        calculation.MarkAsCompleted(
            calculation.ExecutionTimeStart!.Value.Plus(Duration.FromDays(2))); // This call will ensure ExecutionTimeEnd is set
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
        actual.ExecutionTimeStart.Should().NotBeNull();
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
        var calculation = new Application.Model.Calculations.Calculation(
            SystemClock.Instance.GetCurrentInstant(),
            CalculationType.BalanceFixing,
            new List<GridAreaCode> { new("004") },
            period.PeriodStart,
            period.PeriodEnd,
            Instant.FromUtc(2022, 5, 1, 0, 0),
            period.DateTimeZone,
            Guid.NewGuid(),
            SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().Ticks);

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

    private static Application.Model.Calculations.Calculation CreateCalculation(List<GridAreaCode> someGridAreasIds)
    {
        return CreateCalculation(CalculationType.BalanceFixing, someGridAreasIds);
    }

    private static Application.Model.Calculations.Calculation CreateCalculation(CalculationType calculationType, List<GridAreaCode> someGridAreasIds)
    {
        var period = Periods.January_EuropeCopenhagen_Instant;
        return new Application.Model.Calculations.Calculation(
            SystemClock.Instance.GetCurrentInstant(),
            calculationType,
            someGridAreasIds,
            period.PeriodStart,
            period.PeriodEnd,
            SystemClock.Instance.GetCurrentInstant(),
            period.DateTimeZone,
            Guid.NewGuid(),
            SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().Ticks);
    }
}
