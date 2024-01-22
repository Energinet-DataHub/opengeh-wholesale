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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.AmountPerChargeResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.CalculationResultCompleted.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EnergyResultProducedV2.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Factories;
using FluentAssertions;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.EventProviders
{
    public class EnergyResultEventProviderTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenCanCreateEnergyResultProducedV2_ReturnsExpectedEvent(
            [Frozen] Mock<IEnergyResultProducedV2Factory> energyResultProducedV2FactoryMock,
            [Frozen] Mock<IEnergyResultQueries> energyResultQueriesMock,
            EnergyResult energyResult,
            EnergyResultEventProvider sut)
        {
            // Arrange
            var expectedIntegrationEvent = new Contracts.IntegrationEvents.EnergyResultProducedV2();
            var amountPerChargeResult = new[] { wholesaleResult };
            var wholesaleFixingBatch = CreateWholesaleFixingBatch();

            energyResultQueriesMock
                .Setup(mock => mock.GetAsync(wholesaleFixingBatch.Id))
                .Returns(amountPerChargeResult.ToAsyncEnumerable());

            energyResultProducedV2FactoryMock
                .Setup(mock => mock.CanCreate(It.IsAny<EnergyResult>()))
                .Returns(true);
            energyResultProducedV2FactoryMock
                .Setup(mock => mock.Create(It.IsAny<EnergyResult>()))
                .Returns(expectedIntegrationEvent);

            // Act
            var actualIntegrationEvents = await sut.GetAsync(wholesaleFixingBatch).ToListAsync();

            // Assert
            actualIntegrationEvents.Single().EventName.Should().Be(((IEventMessage)expectedIntegrationEvent).EventName);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenCanCreateMonthlyAmountPerChargeResultProducedV1_ReturnsExpectedEvent(
            [Frozen] Mock<IAmountPerChargeResultProducedV1Factory> amountPerChargeResultProducedV1FactoryMock,
            [Frozen] Mock<IMonthlyAmountPerChargeResultProducedV1Factory> monthlyAmountPerChargeResultProducedV1FactoryMock,
            [Frozen] Mock<IWholesaleResultQueries> wholesaleResultQueriesMock,
            WholesaleResult wholesaleResult,
            WholesaleResultEventProvider sut)
        {
            // Arrange
            var expectedIntegrationEvent = new Contracts.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1();
            var wholesaleFixingBatch = CreateWholesaleFixingBatch();
            var wholesaleResults = new[] { wholesaleResult };

            wholesaleResultQueriesMock
                .Setup(mock => mock.GetAsync(wholesaleFixingBatch.Id))
                .Returns(wholesaleResults.ToAsyncEnumerable());

            amountPerChargeResultProducedV1FactoryMock
                .Setup(mock => mock.CanCreate(It.IsAny<WholesaleResult>()))
                .Returns(false);
            monthlyAmountPerChargeResultProducedV1FactoryMock
                .Setup(mock => mock.CanCreate(It.IsAny<WholesaleResult>()))
                .Returns(true);
            monthlyAmountPerChargeResultProducedV1FactoryMock
                .Setup(mock => mock.Create(It.IsAny<WholesaleResult>()))
                .Returns(expectedIntegrationEvent);

            // Act
            var actualIntegrationEvents = await sut.GetAsync(wholesaleFixingBatch).ToListAsync();

            // Assert
            actualIntegrationEvents.Single().EventName.Should().Be(((IEventMessage)expectedIntegrationEvent).EventName);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenNegativeOrPositiveGridLoss_ReturnsTwoEventsPerResult(
            CompletedCalculation completedCalculation,
            EnergyResult[] energyResults,
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
            [Frozen(Matching.ImplementedInterfaces)] CalculationResultCompletedFactory calculationResultCompletedFactory,
            [Frozen(Matching.ImplementedInterfaces)] EnergyResultProducedV2Factory energyResultProducedV2Factory,
            [Frozen(Matching.ImplementedInterfaces)] GridLossResultProducedV1Factory gridLossResultProducedV2Factory,
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
            [Frozen] Mock<IEnergyResultQueries> energyResultQueriesMock,
            EnergyResultEventProvider sut)
        {
            // Arrange
            var expectedEventsPerResult = 2;
            var expectedEventsCount = energyResults.Length * expectedEventsPerResult;

            energyResultQueriesMock
                .Setup(mock => mock.GetAsync(completedCalculation.Id))
                .Returns(energyResults.ToAsyncEnumerable());

            // Act
            var actualIntegrationEvents = await sut.GetAsync(completedCalculation).ToListAsync();

            // Assert
            actualIntegrationEvents.Should().HaveCount(expectedEventsCount);
        }
    }
}
