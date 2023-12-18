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
using Energinet.DataHub.Wholesale.Batches.Application;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Batches;
using Energinet.DataHub.Wholesale.Batches.Application.UseCases;
using Energinet.DataHub.Wholesale.Batches.UnitTests.Infrastructure.BatchAggregate;
using Energinet.DataHub.Wholesale.Shared.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Batches.UnitTests.Application.Batches;

public class StartCalculationHandlerTests
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
        await sut.StartAsync();

        // Assert
        unitOfWorkMock.Verify(x => x.CommitAsync());
        foreach (var batch in batches)
        {
            calculationDomainServiceMock.Verify(x => x.StartAsync(batch.Id));
        }
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task StartCalculationAsync_(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        [Frozen] Mock<ILogger<StartCalculationHandler>> loggerMock,
        StartCalculationHandler sut)
    {
        // Arrange
        const string expectedLogMessage = $"Calculation for calculation {LoggingConstants.CalculationId} started";
        var batches = new List<Batch> { new BatchBuilder().Build(), new BatchBuilder().Build() };
        batchRepositoryMock
            .Setup(repository => repository.GetCreatedAsync())
            .ReturnsAsync(batches);

        // Act
        await sut.StartAsync();

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(
                    (state, t) =>
                    CheckValue(state, expectedLogMessage, "{OriginalFormat}")),
                It.IsAny<Exception>(),
                ((Func<It.IsAnyType, Exception, string>)It.IsAny<object>())!));
    }

    private static bool CheckValue(object state, object expectedValue, string key)
    {
        var keyValuePairList = (IReadOnlyList<KeyValuePair<string, object>>)state;

        var actualValue = keyValuePairList.First(kvp => string.Compare(kvp.Key, key, StringComparison.Ordinal) == 0).Value;

        return expectedValue.Equals(actualValue);
    }
}
