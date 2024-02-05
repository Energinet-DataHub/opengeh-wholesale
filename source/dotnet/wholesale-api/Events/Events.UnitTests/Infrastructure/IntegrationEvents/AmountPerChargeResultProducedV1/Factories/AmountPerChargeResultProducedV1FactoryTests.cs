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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.AmountPerChargeResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Common;
using Energinet.DataHub.Wholesale.Events.UnitTests.Fixtures;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.AmountPerChargeResultProducedV1.Factories;

public class AmountPerChargeResultProducedV1FactoryTests
{
    [Theory]
    [InlineData(AmountType.AmountPerCharge, true)]
    [InlineData(AmountType.MonthlyAmountPerCharge, false)]
    [InlineData(AmountType.TotalMonthlyAmount, false)]
    public void CanCreate_WhenAmountType_ReturnsExpectedValue(
        AmountType amountType, bool expected)
    {
        // Arrange
        var wholesaleResult = new WholesaleResultBuilder()
            .WithAmountType(amountType)
            .WithResolution(Resolution.Day)
            .Build();
        var sut = new AmountPerChargeResultProducedV1Factory();

        // Act
        var actual = sut.CanCreate(wholesaleResult);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(Resolution.Hour, true)]
    [InlineData(Resolution.Day, true)]
    [InlineData(Resolution.Month, false)]
    public void CanCreate_WhenResolution_ReturnsExpectedValue(
        Resolution resolution, bool expected)
    {
        // Arrange
        var wholesaleResult = new WholesaleResultBuilder()
            .WithAmountType(AmountType.AmountPerCharge)
            .WithResolution(resolution)
            .Build();
        var sut = new AmountPerChargeResultProducedV1Factory();

        // Act
        var actual = sut.CanCreate(wholesaleResult);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Create_ReturnsExpectedObject(
        AmountPerChargeResultProducedV1Factory sut)
    {
        // Arrange
        var wholesaleResult = new WholesaleResultBuilder().Build();
        var expected = CreateExpected(wholesaleResult);

        // Act
        var actual = sut.Create(wholesaleResult);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(CalculationType.Aggregation)]
    [InlineData(CalculationType.BalanceFixing)]
    public void Create_WhenUnexpectedCalculationType_ThrowsException(CalculationType calculationType)
    {
        // Arrange
        var sut = new AmountPerChargeResultProducedV1Factory();
        var wholesaleResult = new WholesaleResultBuilder()
            .WithCalculationType(calculationType)
            .Build();

        // Act
        var act = () => sut.Create(wholesaleResult);

        // Act and Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static Contracts.IntegrationEvents.AmountPerChargeResultProducedV1 CreateExpected(WholesaleResult wholesaleResult)
    {
        var amountPerChargeResultProducedV1 = new Contracts.IntegrationEvents.AmountPerChargeResultProducedV1()
        {
            CalculationId = wholesaleResult.CalculationId.ToString(),
            CalculationType = Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.CalculationType.WholesaleFixing,
            QuantityUnit = Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.QuantityUnit.Kwh,
            PeriodStartUtc = wholesaleResult.PeriodStart.ToTimestamp(),
            PeriodEndUtc = wholesaleResult.PeriodEnd.ToTimestamp(),
            GridAreaCode = wholesaleResult.GridArea,
            EnergySupplierId = wholesaleResult.EnergySupplierId,
            ChargeCode = wholesaleResult.ChargeCode,
            ChargeType = Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.ChargeType.Tariff,
            ChargeOwnerId = wholesaleResult.ChargeOwnerId,
            Resolution = Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.Resolution.Hour,
            MeteringPointType = Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.MeteringPointType.Production,
            SettlementMethod = Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.SettlementMethod.Unspecified,
            IsTax = wholesaleResult.IsTax,
            Currency = Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.Currency.Dkk,
        };

        var qualities = new List<Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.QuantityQuality>
        {
            Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.QuantityQuality.Measured,
        };

        amountPerChargeResultProducedV1.TimeSeriesPoints.AddRange(
            wholesaleResult.TimeSeriesPoints.Select(timeSeriesPoint =>
            {
                var p = new Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.TimeSeriesPoint
                {
                    Time = timeSeriesPoint.Time.ToTimestamp(),
                    Quantity = timeSeriesPoint.Quantity,
                    Price = timeSeriesPoint.Price,
                    Amount = timeSeriesPoint.Amount,
                };
                p.QuantityQualities.AddRange(qualities);
                return p;
            }));

        return amountPerChargeResultProducedV1;
    }
}
