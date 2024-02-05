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
using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Application.UseCases;
using Energinet.DataHub.Wholesale.Calculations.UnitTests.Infrastructure.CalculationAggregate;
using Microsoft.Extensions.Logging;
using Moq;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.Calculations.UnitTests.Application.Calculations;

public class StartCalculationHandlerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task StartCalculationAsync_ActivatesInfrastructureServiceAndCommits(
        [Frozen] Mock<ICalculationRepository> calculationRepositoryMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<ICalculationInfrastructureService> calculationInfrastructureServiceMock,
        StartCalculationHandler sut)
    {
        // Arrange
        var calculations = new List<Calculation> { new CalculationBuilder().Build(), new CalculationBuilder().Build() };
        calculationRepositoryMock
            .Setup(repository => repository.GetCreatedAsync())
            .ReturnsAsync(calculations);

        // Arrange & Act
        await sut.StartAsync();

        // Assert
        unitOfWorkMock.Verify(x => x.CommitAsync());
        foreach (var calculation in calculations)
        {
            calculationInfrastructureServiceMock.Verify(x => x.StartAsync(calculation.Id));
        }
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task StartCalculationAsync_LogsExpectedMessage(
        [Frozen] Mock<ICalculationRepository> calculationRepositoryMock,
        [Frozen] Mock<ILogger<StartCalculationHandler>> loggerMock,
        StartCalculationHandler sut)
    {
        // Arrange
        const string expectedLogMessage = $"Calculation with id {LoggingConstants.CalculationId} started.";
        var calculations = new List<Calculation> { new CalculationBuilder().Build(), new CalculationBuilder().Build() };
        calculationRepositoryMock
            .Setup(repository => repository.GetCreatedAsync())
            .ReturnsAsync(calculations);

        // Act
        await sut.StartAsync();

        // Assert
        loggerMock.ShouldBeCalledWith(LogLevel.Information, expectedLogMessage);
    }
}
