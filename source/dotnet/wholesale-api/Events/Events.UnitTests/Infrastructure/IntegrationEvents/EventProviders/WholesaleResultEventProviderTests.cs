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

using AutoFixture;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.AmountPerChargeResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Factories;
using FluentAssertions;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.EventProviders
{
    public class WholesaleResultEventProviderTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenCanCreateAmountPerChargeResultProducedV1_ReturnsExpectedEvent(
            [Frozen] Mock<IAmountPerChargeResultProducedV1Factory> amountPerChargeResultProducedV1FactoryMock,
            [Frozen] Mock<IWholesaleResultQueries> wholesaleResultQueriesMock,
            WholesaleResult wholesaleResult,
            WholesaleResultEventProvider sut)
        {
            // Arrange
            var expectedIntegrationEvent = new Contracts.IntegrationEvents.AmountPerChargeResultProducedV1();
            var amountPerChargeResult = new[] { wholesaleResult };
            var wholesaleFixingCalculation = CreateWholesaleFixingCalculation();

            wholesaleResultQueriesMock
                .Setup(mock => mock.GetAsync(wholesaleFixingCalculation.Id))
                .Returns(amountPerChargeResult.ToAsyncEnumerable());

            amountPerChargeResultProducedV1FactoryMock
                .Setup(mock => mock.CanCreate(It.IsAny<WholesaleResult>()))
                .Returns(true);
            amountPerChargeResultProducedV1FactoryMock
                .Setup(mock => mock.Create(It.IsAny<WholesaleResult>()))
                .Returns(expectedIntegrationEvent);

            // Act
            var actualIntegrationEvents = await sut.GetAsync(wholesaleFixingCalculation).ToListAsync();

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
            var wholesaleFixingCalculation = CreateWholesaleFixingCalculation();
            var wholesaleResults = new[] { wholesaleResult };

            wholesaleResultQueriesMock
                .Setup(mock => mock.GetAsync(wholesaleFixingCalculation.Id))
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
            var actualIntegrationEvents = await sut.GetAsync(wholesaleFixingCalculation).ToListAsync();

            // Assert
            actualIntegrationEvents.Single().EventName.Should().Be(((IEventMessage)expectedIntegrationEvent).EventName);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenCannotCreateAnyEvents_ThrowsException(
            [Frozen] Mock<IAmountPerChargeResultProducedV1Factory> amountPerChargeResultProducedV1FactoryMock,
            [Frozen] Mock<IMonthlyAmountPerChargeResultProducedV1Factory> monthlyAmountPerChargeResultProducedV1FactoryMock,
            [Frozen] Mock<IWholesaleResultQueries> wholesaleResultQueriesMock,
            WholesaleResult wholesaleResult,
            WholesaleResultEventProvider sut)
        {
            // Arrange
            var wholesaleFixingCalculation = CreateWholesaleFixingCalculation();
            var wholesaleResults = new[] { wholesaleResult };

            wholesaleResultQueriesMock
                .Setup(mock => mock.GetAsync(wholesaleFixingCalculation.Id))
                .Returns(wholesaleResults.ToAsyncEnumerable());

            amountPerChargeResultProducedV1FactoryMock
                .Setup(mock => mock.CanCreate(It.IsAny<WholesaleResult>()))
                .Returns(false);
            monthlyAmountPerChargeResultProducedV1FactoryMock
                .Setup(mock => mock.CanCreate(It.IsAny<WholesaleResult>()))
                .Returns(false);

            // Act
            var act = async () => await sut.GetAsync(wholesaleFixingCalculation).SingleAsync();

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenMultipleResults_ReturnsOneEventPerResult(
            [Frozen] Mock<IAmountPerChargeResultProducedV1Factory> amountPerChargeResultProducedV1FactoryMock,
            [Frozen] Mock<IWholesaleResultQueries> wholesaleResultQueriesMock,
            WholesaleResult[] wholesaleResults,
            WholesaleResultEventProvider sut)
        {
            // Arrange
            const int expectedEventsPerResult = 1;
            var expectedEventsCount = wholesaleResults.Length * expectedEventsPerResult;
            var wholesaleFixingCalculation = CreateWholesaleFixingCalculation();

            wholesaleResultQueriesMock
                .Setup(mock => mock.GetAsync(wholesaleFixingCalculation.Id))
                .Returns(wholesaleResults.ToAsyncEnumerable());

            amountPerChargeResultProducedV1FactoryMock
                .Setup(mock => mock.CanCreate(It.IsAny<WholesaleResult>()))
                .Returns(true);
            amountPerChargeResultProducedV1FactoryMock
                .Setup(mock => mock.Create(It.IsAny<WholesaleResult>()))
                .Returns(new Contracts.IntegrationEvents.AmountPerChargeResultProducedV1());

            // Act
            var actualIntegrationEvents = await sut.GetAsync(wholesaleFixingCalculation).ToListAsync();

            // Assert
            actualIntegrationEvents.Should().HaveCount(expectedEventsCount);
        }

        [Theory]
        [InlineData(CalculationType.Aggregation, false)]
        [InlineData(CalculationType.BalanceFixing, false)]
        [InlineData(CalculationType.WholesaleFixing, true)]
        [InlineData(CalculationType.FirstCorrectionSettlement, true)]
        [InlineData(CalculationType.SecondCorrectionSettlement, true)]
        [InlineData(CalculationType.ThirdCorrectionSettlement, true)]
        public void CanContainWholesaleResults_WhenCalculationTypeCanContainWholesaleResults_ReturnsTrue(
            CalculationType calculationType,
            bool canContainWholesaleResults)
        {
            // Arrange
            var fixture = new Fixture();
            var calculation = fixture
                .Build<CompletedCalculation>()
                .With(p => p.CalculationType, calculationType)
                .Create();

            var wholesaleResultQueriesStub = Mock.Of<IWholesaleResultQueries>();

            var sut = new WholesaleResultEventProvider(
                wholesaleResultQueriesStub,
                new AmountPerChargeResultProducedV1Factory(),
                new MonthlyAmountPerChargeResultProducedV1Factory());

            // Act
            var actualResult = sut.CanContainWholesaleResults(calculation);

            // Assert
            actualResult.Should().Be(canContainWholesaleResults);
        }

        private static CompletedCalculation CreateWholesaleFixingCalculation()
        {
            var fixture = new Fixture();
            var wholesaleFixingCalculation = fixture
                .Build<CompletedCalculation>()
                .With(p => p.CalculationType, CalculationType.WholesaleFixing)
                .Create();
            return wholesaleFixingCalculation;
        }
    }
}
