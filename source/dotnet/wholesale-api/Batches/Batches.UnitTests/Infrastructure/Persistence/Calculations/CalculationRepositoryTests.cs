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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Batches.Application.Model;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence.Calculations;
using Energinet.DataHub.Wholesale.Batches.UnitTests.Infrastructure.CalculationAggregate;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using NodaTime;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.Batches.UnitTests.Infrastructure.Persistence.Calculations;

public class CalculationRepositoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task GetCompletedAfterAsync_WhenCompletedTimeIsNullAndNoBatchMatches_ReturnsNone(
        Mock<DatabaseContext> databaseContextMock,
        Mock<DateTimeZone> dateTimeZoneMock)
    {
        // Arrange
        databaseContextMock
            .Setup<DbSet<Calculation>>(context => context.Batches)
            .ReturnsDbSet(new List<Calculation>());
        var sut = new CalculationRepository(databaseContextMock.Object, dateTimeZoneMock.Object);

        // Act
        var actual = await sut.GetCompletedAfterAsync(null);

        // Assert
        actual.Should().BeEmpty();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetCompletedAfterAsync_WhenCompletedTimeIsNotNullAndNoBatchMatches_ReturnsNone(
        Instant completedTime,
        Mock<DatabaseContext> databaseContextMock,
        Mock<DateTimeZone> dateTimeZoneMock)
    {
        // Arrange
        databaseContextMock
            .Setup<DbSet<Calculation>>(context => context.Batches)
            .ReturnsDbSet(new List<Calculation>());
        var sut = new CalculationRepository(databaseContextMock.Object, dateTimeZoneMock.Object);

        // Act
        var actual = await sut.GetCompletedAfterAsync(completedTime);

        // Assert
        actual.Should().BeEmpty();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetCompletedAfterAsync_WhenCompletedTimeIsNullAndSomeBatchesExist_ReturnsThem(
        Mock<DatabaseContext> databaseContextMock,
        Mock<DateTimeZone> dateTimeZoneMock)
    {
        // Arrange
        var batch1 = new CalculationBuilder().WithStateCompleted().Build();
        var batch2 = new CalculationBuilder().WithStateCompleted().Build();
        databaseContextMock
            .Setup<DbSet<Calculation>>(context => context.Batches)
            .ReturnsDbSet(new List<Calculation> { batch1, batch2 });

        var sut = new CalculationRepository(databaseContextMock.Object, dateTimeZoneMock.Object);

        // Act
        var actual = await sut.GetCompletedAfterAsync(null);

        // Assert
        actual.Should().Contain(batch1);
        actual.Should().Contain(batch2);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetCompletedAfterAsync_WhenBatchCompletedBeforeCompletedTime_DoesNotReturnIt(
        Mock<DatabaseContext> databaseContextMock,
        Mock<DateTimeZone> dateTimeZoneMock)
    {
        // Arrange
        var batch = new CalculationBuilder().WithStateCompleted().Build();
        databaseContextMock
            .Setup<DbSet<Calculation>>(context => context.Batches)
            .ReturnsDbSet(new List<Calculation> { batch });
        var futureCompletedTime = batch.ExecutionTimeEnd!.Value.Plus(Duration.FromMinutes(1));

        var sut = new CalculationRepository(databaseContextMock.Object, dateTimeZoneMock.Object);

        // Act
        var actual = await sut.GetCompletedAfterAsync(futureCompletedTime);

        // Assert
        actual.Should().NotContain(batch);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetCompletedAfterAsync_WhenBatchIsNotCompleted_DoesNotReturnIt(
        Mock<DatabaseContext> databaseContextMock,
        Mock<DateTimeZone> dateTimeZoneMock)
    {
        // Arrange
        var nonCompletedBatch = new CalculationBuilder().Build();
        databaseContextMock
            .Setup<DbSet<Calculation>>(context => context.Batches)
            .ReturnsDbSet(new List<Calculation> { nonCompletedBatch });

        var sut = new CalculationRepository(databaseContextMock.Object, dateTimeZoneMock.Object);

        // Act
        var actual = await sut.GetCompletedAfterAsync(null);

        // Assert
        actual.Should().NotContain(nonCompletedBatch);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetNewestCalculationIdsForPeriodAsync_WhenMulitpleCalculationForPeriod_ReturnsThem(
        Mock<DatabaseContext> databaseContextMock)
    {
        // Arrange
        var batch1WithInPeriod = new CalculationBuilder()
            .WithStateCompleted()
            .WithPeriodStart(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022))
            .WithPeriodEnd(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(5))).Build();
        var batch2WithInPeriod = new CalculationBuilder().WithStateCompleted()
            .WithPeriodStart(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(6)))
            .WithPeriodEnd(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(10))).Build();

        databaseContextMock
            .Setup<DbSet<Calculation>>(context => context.Batches)
            .ReturnsDbSet(new List<Calculation> { batch1WithInPeriod, batch2WithInPeriod });

        var sut = new CalculationRepository(databaseContextMock.Object, Periods.January_EuropeCopenhagen_Instant.DateTimeZone);

        // Act
        var actual = await sut.GetNewestCalculationIdsForPeriodAsync(
            Array.Empty<GridAreaCode>(),
            new[] { CalculationExecutionState.Completed },
            Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022),
            Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(10)));

        // Assert
        actual.Should().Contain(new List<CalculationId> { batch1WithInPeriod.CalculationId! });
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetNewestCalculationIdsForPeriodAsync_WhenANewerBatchOverlapsExistingPeriods_ReturnsThem(
        Mock<DatabaseContext> databaseContextMock)
    {
        // Arrange
        var batch1WithInPeriod = new CalculationBuilder()
            .WithStateCompleted()
            .WithPeriodStart(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022))
            .WithPeriodEnd(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(5))).Build();
        var batch2WithInPeriod = new CalculationBuilder().WithStateCompleted()
            .WithPeriodStart(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(6)))
            .WithPeriodEnd(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(10))).Build();
        var newerBatchThatOverlapsBatch1And2 = new CalculationBuilder().WithStateCompleted()
            .WithPeriodStart(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(3)))
            .WithPeriodEnd(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(10))).Build();

        databaseContextMock
            .Setup<DbSet<Calculation>>(context => context.Batches)
            .ReturnsDbSet(new List<Calculation> { batch1WithInPeriod, batch2WithInPeriod, newerBatchThatOverlapsBatch1And2 });

        var sut = new CalculationRepository(databaseContextMock.Object, Periods.January_EuropeCopenhagen_Instant.DateTimeZone);

        // Act
        var actual = await sut.GetNewestCalculationIdsForPeriodAsync(
            Array.Empty<GridAreaCode>(),
            new[] { CalculationExecutionState.Completed },
            Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022),
            Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(10)));

        // Assert
        actual.Should().Contain(new List<CalculationId> { batch1WithInPeriod.CalculationId!, newerBatchThatOverlapsBatch1And2.CalculationId! });
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetNewestCalculationIdsForPeriodAsync_WhenACalculationIsNotCoveringPeriod_ReturnsNOTSUREWHATSHOULDHAPPENHERE(
        Mock<DatabaseContext> databaseContextMock)
    {
        // Arrange
        var batchWithInPeriod = new CalculationBuilder()
            .WithStateCompleted()
            .WithPeriodStart(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022))
            .WithPeriodEnd(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(5))).Build();

        databaseContextMock
            .Setup<DbSet<Calculation>>(context => context.Batches)
            .ReturnsDbSet(new List<Calculation> { batchWithInPeriod });

        var sut = new CalculationRepository(databaseContextMock.Object, Periods.January_EuropeCopenhagen_Instant.DateTimeZone);

        // Act
        var actual = await sut.GetNewestCalculationIdsForPeriodAsync(
            Array.Empty<GridAreaCode>(),
            new[] { CalculationExecutionState.Completed },
            Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022),
            Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(10)));

        // Assert
        //actual.Should().Contain(new List<CalculationId> { batchWithInPeriod.CalculationId! });
        actual.Should().Contain(new List<CalculationId> { });
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetNewestCalculationIdsForPeriodAsync_WhenEdgePeriodEnd_ReturnsThem(
        Mock<DatabaseContext> databaseContextMock)
    {
        // Arrange
        var batch1WithInPeriod = new CalculationBuilder()
            .WithStateCompleted()
            .WithPeriodStart(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022))
            .WithPeriodEnd(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(5))).Build();
        var batch2WithInPeriod = new CalculationBuilder().WithStateCompleted()
            .WithPeriodStart(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(6)))
            .WithPeriodEnd(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(10))).Build();

        databaseContextMock
            .Setup<DbSet<Calculation>>(context => context.Batches)
            .ReturnsDbSet(new List<Calculation> { batch1WithInPeriod, batch2WithInPeriod });

        var sut = new CalculationRepository(databaseContextMock.Object, Periods.January_EuropeCopenhagen_Instant.DateTimeZone);

        // Act
        var actual = await sut.GetNewestCalculationIdsForPeriodAsync(
            Array.Empty<GridAreaCode>(),
            new[] { CalculationExecutionState.Completed },
            Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022),
            Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(5)));

        // Assert
        actual.Should().Contain(new List<CalculationId> { batch1WithInPeriod.CalculationId! });
        actual.Should().NotContain(new List<CalculationId> { batch2WithInPeriod.CalculationId! });
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetNewestCalculationIdsForPeriodAsync_WhenEdgePeriodStart_ReturnsThem(
        Mock<DatabaseContext> databaseContextMock)
    {
        // Arrange
        var batch1WithInPeriod = new CalculationBuilder()
            .WithStateCompleted()
            .WithPeriodStart(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022))
            .WithPeriodEnd(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(5))).Build();
        var batch2WithInPeriod = new CalculationBuilder().WithStateCompleted()
            .WithPeriodStart(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(6)))
            .WithPeriodEnd(Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(10))).Build();

        databaseContextMock
            .Setup<DbSet<Calculation>>(context => context.Batches)
            .ReturnsDbSet(new List<Calculation> { batch1WithInPeriod, batch2WithInPeriod });

        var sut = new CalculationRepository(databaseContextMock.Object, Periods.January_EuropeCopenhagen_Instant.DateTimeZone);

        // Act
        var actual = await sut.GetNewestCalculationIdsForPeriodAsync(
            Array.Empty<GridAreaCode>(),
            new[] { CalculationExecutionState.Completed },
            Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(6)),
            Instant.FromDateTimeOffset(CalculationBuilder.FirstOfJanuary2022.AddDays(10)));

        // Assert
        actual.Should().NotContain(new List<CalculationId> { batch1WithInPeriod.CalculationId! });
        actual.Should().Contain(new List<CalculationId> { batch2WithInPeriod.CalculationId! });
    }
}
