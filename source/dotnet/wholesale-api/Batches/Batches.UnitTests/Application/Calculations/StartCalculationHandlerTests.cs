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
using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Batches.Application.UseCases;
using Energinet.DataHub.Wholesale.Batches.UnitTests.Infrastructure.CalculationAggregate;
using Microsoft.Extensions.Logging;
using Moq;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.Batches.UnitTests.Application.Calculations;

public class StartCalculationHandlerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task StartCalculationAsync_ActivatesInfrastructureServiceAndCommits(
        [Frozen] Mock<ICalculationRepository> batchRepositoryMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<ICalculationInfrastructureService> calculationInfrastructureServiceMock,
        StartCalculationHandler sut)
    {
        // Arrange
        var batches = new List<Calculation> { new CalculationBuilder().Build(), new CalculationBuilder().Build() };
        batchRepositoryMock
            .Setup(repository => repository.GetCreatedAsync())
            .ReturnsAsync(batches);

        // Arrange & Act
        await sut.StartAsync();

        // Assert
        unitOfWorkMock.Verify(x => x.CommitAsync());
        foreach (var batch in batches)
        {
            calculationInfrastructureServiceMock.Verify(x => x.StartAsync(batch.Id));
        }
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task StartCalculationAsync_LogsExpectedMessage(
        [Frozen] Mock<ICalculationRepository> batchRepositoryMock,
        [Frozen] Mock<ILogger<StartCalculationHandler>> loggerMock,
        StartCalculationHandler sut)
    {
        // Arrange
        const string expectedLogMessage = $"Calculation with id {LoggingConstants.CalculationId} started.";
        var batches = new List<Calculation> { new CalculationBuilder().Build(), new CalculationBuilder().Build() };
        batchRepositoryMock
            .Setup(repository => repository.GetCreatedAsync())
            .ReturnsAsync(batches);

        // Act
        await sut.StartAsync();

        // Assert
        loggerMock.ShouldBeCalledWith(LogLevel.Information, expectedLogMessage);
    }
}
