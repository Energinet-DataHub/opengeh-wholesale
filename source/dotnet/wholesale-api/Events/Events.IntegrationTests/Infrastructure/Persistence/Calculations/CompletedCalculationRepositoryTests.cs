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
using Energinet.DataHub.Wholesale.Common.Infrastructure.FeatureFlag;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence.CompletedCalculations;
using Energinet.DataHub.Wholesale.Test.Core.Fixture.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.IntegrationTests.Infrastructure.Persistence.Calculations;

public class CompletedCalculationRepositoryTests : IClassFixture<WholesaleDatabaseFixture<EventsDatabaseContext>>
{
    private readonly WholesaleDatabaseManager<EventsDatabaseContext> _databaseManager;

    public CompletedCalculationRepositoryTests(WholesaleDatabaseFixture<EventsDatabaseContext> fixture)
    {
        _databaseManager = fixture.DatabaseManager;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task AddAsync_AddsCompletedCalculationWithExpectedData(
        CompletedCalculation expectedCalculation,
        Mock<IFeatureFlagManager> featureFlagManagerMock,
        Mock<NodaTime.IClock> clockMock)
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var sut = new CompletedCalculationRepository(writeContext, featureFlagManagerMock.Object, clockMock.Object);
        expectedCalculation.PublishedTime = null;

        // Act
        await sut.AddAsync(new[] { expectedCalculation });
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.CompletedCalculations.SingleAsync(b => b.Id == expectedCalculation.Id);

        actual.Should().BeEquivalentTo(expectedCalculation);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task AddAsync_AddsCompletedCalculationAsPublished(
        CompletedCalculation expectedCalculation,
        Mock<IFeatureFlagManager> featureFlagManagerMock,
        Mock<NodaTime.IClock> clockMock)
    {
        // Arrange
        var currentInstant = NodaTime.SystemClock.Instance.GetCurrentInstant();
        clockMock.Setup(c => c.GetCurrentInstant()).Returns(currentInstant);
        featureFlagManagerMock.Setup(o => o.UsePublishCalculationResultsAsync())
            .Returns(Task.FromResult(false));
        await using var writeContext = _databaseManager.CreateDbContext();
        var sut = new CompletedCalculationRepository(writeContext, featureFlagManagerMock.Object, clockMock.Object);
        expectedCalculation.PublishedTime = null;

        // Act
        await sut.AddAsync(new[] { expectedCalculation });
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.CompletedCalculations.SingleAsync(b => b.Id == expectedCalculation.Id);

        actual.PublishedTime.Should().Be(currentInstant);
    }
}
