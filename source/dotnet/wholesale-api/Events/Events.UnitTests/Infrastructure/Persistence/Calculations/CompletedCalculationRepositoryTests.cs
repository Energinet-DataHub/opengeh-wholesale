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

using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence.CompletedCalculations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using Moq;
using Moq.EntityFrameworkCore;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.Persistence.Calculations;

public class CompletedCalculationRepositoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task GetLastCompletedOrNullAsync_WhenNone_ReturnsNull(
        Mock<EventsDatabaseContext> databaseContextMock)
    {
        // Arrange
        databaseContextMock
            .Setup<DbSet<CompletedCalculation>>(context => context.CompletedCalculations)
            .ReturnsDbSet(new List<CompletedCalculation>());
        var sut = new CompletedCalculationRepository(databaseContextMock.Object);

        // Act
        var actual = await sut.GetLastCompletedOrNullAsync();

        // Assert
        actual.Should().BeNull();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetLastCompletedOrNullAsync_WhenMore_ReturnsLastByCompletedTime(
        CompletedCalculation calculationCompletedFirst,
        CompletedCalculation calculationCompletedLast,
        [Frozen] Mock<EventsDatabaseContext> databaseContextMock)
    {
        // Arrange
        calculationCompletedLast.SetPrivateProperty(b => b.CompletedTime, calculationCompletedFirst.CompletedTime.PlusSeconds(1));

        databaseContextMock
            .Setup<DbSet<CompletedCalculation>>(context => context.CompletedCalculations)
            .ReturnsDbSet(new List<CompletedCalculation> { calculationCompletedFirst, calculationCompletedLast });
        var sut = new CompletedCalculationRepository(databaseContextMock.Object);

        // Act
        var actual = await sut.GetLastCompletedOrNullAsync();

        // Assert
        actual.Should().Be(calculationCompletedLast);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetNextUnpublishedOrNullAsync_WhenNone_ReturnsNull(
        Mock<EventsDatabaseContext> databaseContextMock)
    {
        // Arrange
        databaseContextMock
            .Setup<DbSet<CompletedCalculation>>(context => context.CompletedCalculations)
            .ReturnsDbSet(new List<CompletedCalculation>());
        var sut = new CompletedCalculationRepository(databaseContextMock.Object);

        // Act
        var actual = await sut.GetNextUnpublishedOrNullAsync();

        // Assert
        actual.Should().BeNull();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetNextUnpublishedOrNullAsync_WhenMore_ReturnsFirstByCompletedTime(
        CompletedCalculation calculationCompletedFirst,
        CompletedCalculation calculationCompletedLast,
        [Frozen] Mock<EventsDatabaseContext> databaseContextMock)
    {
        // Arrange
        calculationCompletedFirst.PublishedTime = null;
        calculationCompletedLast.PublishedTime = null;
        calculationCompletedLast.SetPrivateProperty(b => b.CompletedTime, calculationCompletedFirst.CompletedTime.PlusSeconds(1));

        databaseContextMock
            .Setup<DbSet<CompletedCalculation>>(context => context.CompletedCalculations)
            .ReturnsDbSet(new List<CompletedCalculation> { calculationCompletedFirst, calculationCompletedLast });
        var sut = new CompletedCalculationRepository(databaseContextMock.Object);

        // Act
        var actual = await sut.GetNextUnpublishedOrNullAsync();

        // Assert
        actual.Should().Be(calculationCompletedFirst);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetNextUnpublishedOrNullAsync_WhenAllAlreadyPublished_ReturnsNull(
        CompletedCalculation publishedCalculation,
        [Frozen] Mock<EventsDatabaseContext> databaseContextMock)
    {
        // Arrange
        databaseContextMock
            .Setup<DbSet<CompletedCalculation>>(context => context.CompletedCalculations)
            .ReturnsDbSet(new List<CompletedCalculation> { publishedCalculation });
        var sut = new CompletedCalculationRepository(databaseContextMock.Object);

        // Act
        var actual = await sut.GetNextUnpublishedOrNullAsync();

        // Assert
        actual.Should().BeNull();
    }
}
