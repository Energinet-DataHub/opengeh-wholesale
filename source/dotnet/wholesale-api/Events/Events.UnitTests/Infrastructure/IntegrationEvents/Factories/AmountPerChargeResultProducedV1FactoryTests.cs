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
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using Test.Core;
using Xunit;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Factories;

public class AmountPerChargeResultProducedV1FactoryTests
{
    private readonly Guid _calculationId = Guid.NewGuid();
    private readonly Guid _resultId = Guid.NewGuid();
    private readonly Instant _periodStart = SystemClock.Instance.GetCurrentInstant();
    private readonly Instant _periodEnd = SystemClock.Instance.GetCurrentInstant();
    private readonly string _gridArea = "543";
    private readonly string _energySupplierId = "es_id";
    private readonly string _chargeCode = "charge_code";
    private readonly string _chargeOwnerId = "charge_owner_id";
    private readonly ChargeType _chargeType = ChargeType.Tariff;
    private readonly bool _isTax = true;

    [Theory]
    [InlineAutoMoqData]
    public void Create_ReturnsExpectedObject(
        AmountPerChargeResultProducedV1Factory sut)
    {
        // Arrange
        var wholesaleResult = CreateWholesaleResult();
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
        var wholesaleResult = CreateWholesaleResult();
        wholesaleResult.SetPrivateProperty(r => r.CalculationType, calculationType);

        // Act
        var act = () => sut.Create(wholesaleResult);

        // Act and Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private WholesaleResult CreateWholesaleResult()
    {
        var qualities = new List<QuantityQuality>
        {
            QuantityQuality.Estimated,
            QuantityQuality.Measured,
        };

        return new WholesaleResult(
            _resultId,
            _calculationId,
            Common.Models.ProcessType.FirstCorrectionSettlement,
            _periodStart,
            _periodEnd,
            _gridArea,
            _energySupplierId,
            _chargeCode,
            _chargeType,
            _chargeOwnerId,
            _isTax,
            QuantityUnit.Kwh,
            ChargeResolution.Hour,
            CalculationResults.Interfaces.CalculationResults.Model.MeteringPointType.Production,
            null,
            new WholesaleTimeSeriesPoint[]
            {
                new(new DateTime(2021, 1, 1), 1, qualities, 2, 3),
                new(new DateTime(2021, 1, 1), 2, qualities, 4, 5),
                new(new DateTime(2021, 1, 1), 3, qualities, 6, 7),
            });
    }

    private static AmountPerChargeResultProducedV1 CreateExpected(WholesaleResult wholesaleResult)
    {
        var amountPerChargeResultProducedV1 = new AmountPerChargeResultProducedV1()
        {
            CalculationId = wholesaleResult.CalculationId.ToString(),
            CalculationType = AmountPerChargeResultProducedV1.Types.CalculationType.FirstCorrectionSettlement,
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
        };

        var qualities = new List<AmountPerChargeResultProducedV1.Types.QuantityQuality>
        {
            AmountPerChargeResultProducedV1.Types.QuantityQuality.Estimated,
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
