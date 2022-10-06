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
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using Energinet.DataHub.Wholesale.Tests.Domain.BatchAggregate;
using FluentAssertions;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Application;

[UnitTest]
public class BatchApplicationServiceTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task SearchAsync_NoMatchingBatches_ReturnsZeroBatches(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        BatchApplicationService sut)
    {
        // Arrange
        var noBatches = new List<Batch>();
        batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Instant>(), It.IsAny<Instant>())).ReturnsAsync(noBatches);
        var batchSearchDto = new BatchSearchDto(DateTimeOffset.Now, DateTimeOffset.Now);

        // Act
        var searchResult = await sut.SearchAsync(batchSearchDto);

        // Assert
        searchResult.Count().Should().Be(0);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task SearchAsync_NoMatchingBatches_DoesNotThrowException(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        BatchApplicationService sut)
    {
        // Arrange
        var noBatches = new List<Batch>();
        batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Instant>(), It.IsAny<Instant>())).ReturnsAsync(noBatches);

        // Act
        var batchSearchDto = new BatchSearchDto(DateTimeOffset.Now, DateTimeOffset.Now);
        await sut.SearchAsync(batchSearchDto);

        // Assert
        // --- If we had an exception the test will fail - so no need for more assert
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task SearchAsync_ReturnsCorrectNumberOfBatches(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        BatchApplicationService sut)
    {
        // Arrange
        const int numberOfBatches = 3;
        var batches = new List<Batch>()
        {
            new BatchBuilder().Build(),
            new BatchBuilder().Build(),
            new BatchBuilder().Build(),
        };
        batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Instant>(), It.IsAny<Instant>())).ReturnsAsync(batches);

        // Act
        var batchSearchDto = new BatchSearchDto(DateTimeOffset.Now, DateTimeOffset.Now);
        var searchResult = await sut.SearchAsync(batchSearchDto);

        // Assert
        searchResult.Count().Should().Be(numberOfBatches);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task SearchAsync_BatchesAreMappedCorrectly(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        [Frozen] Mock<IClock> clockMock,
        BatchApplicationService sut)
    {
        // Arrange
        var batch1 = new Batch(
            ProcessType.BalanceFixing,
            new List<GridAreaCode> { new("805"), new("806") },
            Instant.FromUtc(2022, 5, 31, 22, 00),
            Instant.FromUtc(2022, 6, 1, 22, 00),
            clockMock.Object);
        batch1.MarkAsExecuting();

        var batch2 = new Batch(
            ProcessType.BalanceFixing,
            new List<GridAreaCode> { new("105"), new("106") },
            Instant.FromUtc(2019, 2, 22, 22, 00),
            Instant.FromUtc(2019, 7, 1, 22, 00),
            clockMock.Object);

        var batches = new List<Batch>()
        {
            batch1,
            batch2,
        };

        batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Instant>(), It.IsAny<Instant>())).ReturnsAsync(batches);
        var batchSearchDto = new BatchSearchDto(DateTimeOffset.Now, DateTimeOffset.Now);

        // Act
        var searchResult = await sut.SearchAsync(batchSearchDto);

        // Assert
        for (var i = 0; i < batches.Count; i++)
        {
            Assert.Equal(MapToBatchDto.Map(batches[i]), searchResult.ElementAt(i));
        }
    }
}
