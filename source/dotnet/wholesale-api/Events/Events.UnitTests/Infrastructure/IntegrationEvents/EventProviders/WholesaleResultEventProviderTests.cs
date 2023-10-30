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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Models;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;
using FluentAssertions;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.EventProviders
{
    public class WholesaleResultEventProviderTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenMultipleResults_ReturnsOneEventPerResult(
            [Frozen] Mock<IAmountPerChargeResultProducedV1Factory> amountPerChargeResultProducedV1FactoryMock,
            [Frozen] Mock<IMonthlyAmountPerChargeResultProducedV1Factory> monthlyAmountPerChargeResultProducedV1FactoryMock,
            [Frozen] Mock<IWholesaleResultQueries> wholesaleResultQueriesMock,
            WholesaleResultEventProvider sut)
        {
            // Arrange
            var expectedEventsPerResult = 1;
            var wholesaleResults = new[] { CreateWholesaleResult(AmountType.AmountPerCharge), CreateWholesaleResult(AmountType.MonthlyAmountPerCharge) };
            var expectedEventsCount = wholesaleResults.Length * expectedEventsPerResult;

            var fixture = new Fixture();
            var wholesaleFixingBatch = fixture
                .Build<CompletedBatch>()
                .With(p => p.ProcessType, ProcessType.WholesaleFixing)
                .Create();

            wholesaleResultQueriesMock
                .Setup(mock => mock.GetAsync(wholesaleFixingBatch.Id))
                .Returns(wholesaleResults.ToAsyncEnumerable());

            amountPerChargeResultProducedV1FactoryMock
                .Setup(mock => mock.Create(It.IsAny<WholesaleResult>()))
                .Returns(new AmountPerChargeResultProducedV1());
            monthlyAmountPerChargeResultProducedV1FactoryMock
                .Setup(mock => mock.Create(It.IsAny<WholesaleResult>()))
                .Returns(new MonthlyAmountPerChargeResultProducedV1());

            // Act
            var actualIntegrationEvents = await sut.GetAsync(wholesaleFixingBatch).ToListAsync();

            // Assert
            actualIntegrationEvents.Should().HaveCount(expectedEventsCount);
        }

        [Theory]
        [InlineData(ProcessType.Aggregation, false)]
        [InlineData(ProcessType.BalanceFixing, false)]
        [InlineData(ProcessType.WholesaleFixing, true)]
        [InlineData(ProcessType.FirstCorrectionSettlement, true)]
        [InlineData(ProcessType.SecondCorrectionSettlement, true)]
        [InlineData(ProcessType.ThirdCorrectionSettlement, true)]
        public void CanContainWholesaleResults_WhenProcessTypeCanContainWholesaleResults_ReturnsTrue(
            ProcessType processType,
            bool canContainWholesaleResults)
        {
            // Arrange
            var fixture = new Fixture();
            var batch = fixture
                .Build<CompletedBatch>()
                .With(p => p.ProcessType, processType)
                .Create();

            var wholesaleResultQueriesStub = Mock.Of<IWholesaleResultQueries>();

            var sut = new WholesaleResultEventProvider(
                wholesaleResultQueriesStub,
                new AmountPerChargeResultProducedV1Factory(),
                new MonthlyAmountPerChargeResultProducedV1Factory());

            // Act
            var actualResult = sut.CanContainWholesaleResults(batch);

            // Assert
            actualResult.Should().Be(canContainWholesaleResults);
        }

        private WholesaleResult CreateWholesaleResult(AmountType amountType)
        {
            var qualities = new List<QuantityQuality>
            {
                QuantityQuality.Measured,
            };

            return new WholesaleResult(
                Guid.NewGuid(),
                Guid.NewGuid(),
                ProcessType.FirstCorrectionSettlement,
                Instant.FromUtc(2022, 5, 1, 0, 0),
                Instant.FromUtc(2022, 5, 1, 1, 0),
                "gridArea",
                "energySupplierId",
                amountType,
                "chargeCode",
                ChargeType.Tariff,
                "chargeOwnerId",
                false,
                QuantityUnit.Kwh,
                amountType == AmountType.AmountPerCharge ? ChargeResolution.Hour : ChargeResolution.Month,
                MeteringPointType.Production,
                null,
                new WholesaleTimeSeriesPoint[]
                {
                    new(new DateTime(2021, 1, 1), 1, qualities, 2, 3),
                });
        }
    }
}
