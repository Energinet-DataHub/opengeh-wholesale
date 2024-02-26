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
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Application.UseCases;

public class RegisterCompletedCalculationsHandlerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task RegisterCompletedCalculationsAsync_WhenTwoNewCalculationsHasCompleted_RegistersThem(
        CalculationDto newCalculation1,
        CalculationDto newCalculation2,
        CompletedCalculation lastKnownCompletedCalculation,
        CompletedCalculation newCompletedCalculation1,
        CompletedCalculation newCompletedCalculation2,
        [Frozen] Mock<ICalculationsClient> calculationsClientMock,
        [Frozen] Mock<ICompletedCalculationRepository> completedCalculationRepositoryMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<ICompletedCalculationFactory> completedCalculationFactoryMock,
        RegisterCompletedCalculationsHandler sut)
    {
        // Arrange
        completedCalculationRepositoryMock
            .Setup(repository => repository.GetLastCompletedOrNullAsync())
            .ReturnsAsync(lastKnownCompletedCalculation);
        calculationsClientMock
            .Setup(client => client.GetCompletedAfterAsync(It.IsAny<Instant>()))
            .ReturnsAsync(new[] { newCalculation1, newCalculation2 });
        completedCalculationFactoryMock
            .Setup(x => x.CreateFromCalculations(It.IsAny<IEnumerable<CalculationDto>>()))
            .Returns(new[] { newCompletedCalculation1, newCompletedCalculation2 });

        // Act
        await sut.RegisterCompletedCalculationsAsync();

        // Assert

        // The two calculations has been registered
        completedCalculationRepositoryMock
            .Verify(
                x => x.AddAsync(It.Is<IEnumerable<CompletedCalculation>>(
                    calculations => calculations.First().Id == newCompletedCalculation1.Id && calculations.Last().Id == newCompletedCalculation2.Id)),
                Times.Once);

        // And the unit of work has been committed
        unitOfWorkMock.Verify(work => work.CommitAsync());
    }
}
