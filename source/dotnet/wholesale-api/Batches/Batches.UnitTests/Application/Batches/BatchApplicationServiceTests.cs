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

using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Batches.Application;
using Energinet.DataHub.Wholesale.Batches.Application.Model;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Batches;
using Energinet.DataHub.Wholesale.Batches.Application.UseCases;
using Energinet.DataHub.Wholesale.Batches.UnitTests.Infrastructure.BatchAggregate;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Batches.UnitTests.Application.Batches;

[UnitTest]
public class BatchApplicationServiceTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task StartCalculationAsync_ActivatesDomainServiceAndCommits(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<ICalculationInfrastructureService> calculationDomainServiceMock,
        StartCalculationHandler sut)
    {
        // Arrange
        var batches = new List<Batch> { new BatchBuilder().Build(), new BatchBuilder().Build() };
        batchRepositoryMock
            .Setup(repository => repository.GetCreatedAsync())
            .ReturnsAsync(batches);

        // Arrange & Act
        await sut.StartCalculationsAsync();

        // Assert
        unitOfWorkMock.Verify(x => x.CommitAsync());
        foreach (var batch in batches)
        {
            calculationDomainServiceMock.Verify(x => x.StartAsync(batch.Id));
        }
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionStateAsync_ActivatesDomainServiceAndCommits(
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IBatchExecutionStateInfrastructureService> calculationDomainServiceMock,
        BatchApplicationService sut)
    {
        // Arrange & Act
        await sut.UpdateExecutionStateAsync();

        // Assert
        unitOfWorkMock.Verify(x => x.CommitAsync());
        calculationDomainServiceMock.Verify(x => x.UpdateExecutionStateAsync());
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task SearchAsync_NoMatchingBatches_ReturnsZeroBatches(
       [Frozen] Mock<IBatchRepository> batchRepositoryMock,
       BatchApplicationService sut)
    {
        // Arrange
        var noBatches = new List<Batch>();
        batchRepositoryMock
            .Setup(x => x.SearchAsync(
                Array.Empty<GridAreaCode>(),
                Array.Empty<BatchExecutionState>(),
                null,
                null,
                null,
                null))
            .ReturnsAsync(noBatches);

        // Act
        var searchResult = await sut.SearchAsync(
            Array.Empty<string>(),
            null,
            null,
            null,
            null,
            null);

        // Assert
        searchResult.Count().Should().Be(0);
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

        batchRepositoryMock
            .Setup(x => x.SearchAsync(
                It.IsAny<List<GridAreaCode>>(),
                Array.Empty<BatchExecutionState>(),
                null,
                null,
                null,
                null))
            .ReturnsAsync(batches);

        // Act
        var searchResult = await sut.SearchAsync(
            Array.Empty<string>(),
            null,
            null,
            null,
            null,
            null);

        // Assert
        searchResult.Count().Should().Be(numberOfBatches);
    }
}
