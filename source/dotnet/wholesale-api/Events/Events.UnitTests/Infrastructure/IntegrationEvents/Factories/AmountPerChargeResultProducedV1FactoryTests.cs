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
using Energinet.DataHub.Wholesale.Common.Models;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Types;
using Energinet.DataHub.Wholesale.Events.UnitTests.Fixtures;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Factories;

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
            .WithResolution(ChargeResolution.Day)
            .Build();
        var sut = new AmountPerChargeResultProducedV1Factory();

        // Act
        var actual = sut.CanCreate(wholesaleResult);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(ChargeResolution.Hour, true)]
    [InlineData(ChargeResolution.Day, true)]
    [InlineData(ChargeResolution.Month, false)]
    public void CanCreate_WhenResolution_ReturnsExpectedValue(
        ChargeResolution resolution, bool expected)
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
    [InlineData(ProcessType.Aggregation)]
    [InlineData(ProcessType.BalanceFixing)]
    public void Create_WhenUnexpectedCalculationType_ThrowsException(ProcessType calculationType)
    {
        // Arrange
        var sut = new AmountPerChargeResultProducedV1Factory();
        var wholesaleResult = new WholesaleResultBuilder().WithCalculationType(calculationType).Build();

        // Act
        var act = () => sut.Create(wholesaleResult);

        // Act and Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static AmountPerChargeResultProducedV1 CreateExpected(WholesaleResult wholesaleResult)
    {
        var amountPerChargeResultProducedV1 = new AmountPerChargeResultProducedV1()
        {
            CalculationId = wholesaleResult.CalculationId.ToString(),
            CalculationType = AmountPerChargeResultProducedV1.Types.CalculationType.WholesaleFixing,
            QuantityUnit = AmountPerChargeResultProducedV1.Types.QuantityUnit.Kwh,
            PeriodStartUtc = wholesaleResult.PeriodStart.ToTimestamp(),
            PeriodEndUtc = wholesaleResult.PeriodEnd.ToTimestamp(),
            GridAreaCode = wholesaleResult.GridArea,
            EnergySupplierId = wholesaleResult.EnergySupplierId,
            ChargeCode = wholesaleResult.ChargeCode,
            ChargeType = AmountPerChargeResultProducedV1.Types.ChargeType.Tariff,
            ChargeOwnerId = wholesaleResult.ChargeOwnerId,
            Resolution = AmountPerChargeResultProducedV1.Types.Resolution.Hour,
            MeteringPointType = AmountPerChargeResultProducedV1.Types.MeteringPointType.Production,
            SettlementMethod = AmountPerChargeResultProducedV1.Types.SettlementMethod.Unspecified,
            IsTax = wholesaleResult.IsTax,
            Currency = AmountPerChargeResultProducedV1.Types.Currency.Dkk,
        };

        var qualities = new List<AmountPerChargeResultProducedV1.Types.QuantityQuality>
        {
            AmountPerChargeResultProducedV1.Types.QuantityQuality.Measured,
        };

        amountPerChargeResultProducedV1.TimeSeriesPoints.AddRange(
            wholesaleResult.TimeSeriesPoints.Select(timeSeriesPoint =>
            {
                var p = new AmountPerChargeResultProducedV1.Types.TimeSeriesPoint
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
