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
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Application.UseCases;

public class RegisterCompletedBatchesHandlerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task RegisterCompletedBatchesAsync_WhenTwoNewBatchHasCompleted_RegistersThem(
        BatchDto newBatch1,
        BatchDto newBatch2,
        CompletedBatch lastKnownCompletedBatch,
        CompletedBatch newCompletedBatch1,
        CompletedBatch newCompletedBatch2,
        [Frozen] Mock<ICalculationsClient> batchesClientMock,
        [Frozen] Mock<ICompletedBatchRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<ICompletedBatchFactory> completedBatchFactoryMock,
        RegisterCompletedBatchesHandler sut)
    {
        // Arrange
        completedBatchRepositoryMock
            .Setup(repository => repository.GetLastCompletedOrNullAsync())
            .ReturnsAsync(lastKnownCompletedBatch);
        batchesClientMock
            .Setup(client => client.GetBatchesCompletedAfterAsync(It.IsAny<Instant>()))
            .ReturnsAsync(new[] { newBatch1, newBatch2 });
        completedBatchFactoryMock
            .Setup(x => x.CreateFromBatches(It.IsAny<IEnumerable<BatchDto>>()))
            .Returns(new[] { newCompletedBatch1, newCompletedBatch2 });

        // Act
        await sut.RegisterCompletedBatchesAsync();

        // Assert

        // The two batches has been registered
        completedBatchRepositoryMock
            .Verify(
                x => x.AddAsync(It.Is<IEnumerable<CompletedBatch>>(
                    batches => batches.First().Id == newCompletedBatch1.Id && batches.Last().Id == newCompletedBatch2.Id)),
                Times.Once);

        // And the unit of work has been committed
        unitOfWorkMock.Verify(work => work.CommitAsync());
    }
}
