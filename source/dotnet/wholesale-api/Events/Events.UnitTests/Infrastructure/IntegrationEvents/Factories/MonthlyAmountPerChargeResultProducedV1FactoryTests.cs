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
using NodaTime;
using Test.Core;
using Xunit;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Factories;

public class MonthlyAmountPerChargeResultProducedV1FactoryTests
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
        MonthlyAmountPerChargeResultProducedV1Factory sut)
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
        var sut = new MonthlyAmountPerChargeResultProducedV1Factory();
        var wholesaleResult = CreateWholesaleResult(calculationType);

        // Act
        var act = () => sut.Create(wholesaleResult);

        // Act and Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineAutoMoqData]
    public void Create_WhenWholesaleResultHasMoreThanOneTimeSeriesPoints_ThrowsException(MonthlyAmountPerChargeResultProducedV1Factory sut)
    {
        // Arrange
        var timeSeriesPoints = new WholesaleTimeSeriesPoint[]
        {
            new(new DateTime(2021, 1, 1), 1, new List<QuantityQuality> { QuantityQuality.Measured }, 2, 3),
            new(new DateTime(2021, 1, 2), 1, new List<QuantityQuality> { QuantityQuality.Measured }, 2, 3),
        };
        var wholesaleResult = CreateWholesaleResult(timeSeriesPoints: timeSeriesPoints);

        // Act
        var act = () => sut.Create(wholesaleResult);

        // Act and Assert
        act.Should().Throw<ArgumentException>();
    }

    private WholesaleResult CreateWholesaleResult(
        Common.Models.ProcessType calculationType = Common.Models.ProcessType.FirstCorrectionSettlement,
        IReadOnlyCollection<WholesaleTimeSeriesPoint>? timeSeriesPoints = default)
    {
        var qualities = new List<QuantityQuality>
        {
            QuantityQuality.Estimated,
            QuantityQuality.Measured,
        };

        timeSeriesPoints ??= new WholesaleTimeSeriesPoint[]
        {
            new(new DateTime(2021, 1, 1), 1, qualities, 2, 3),
        };

        return new WholesaleResult(
            _resultId,
            _calculationId,
            calculationType,
            _periodStart,
            _periodEnd,
            _gridArea,
            _energySupplierId,
            AmountType.MonthlyAmountPerCharge,
            _chargeCode,
            _chargeType,
            _chargeOwnerId,
            _isTax,
            QuantityUnit.Kwh,
            Resolution.Month,
            CalculationResults.Interfaces.CalculationResults.Model.MeteringPointType.Production,
            null,
            timeSeriesPoints);
    }

    private static MonthlyAmountPerChargeResultProducedV1 CreateExpected(WholesaleResult wholesaleResult)
    {
        var monthlyAmountPerChargeResultProducedV1 = new MonthlyAmountPerChargeResultProducedV1
        {
            CalculationId = wholesaleResult.CalculationId.ToString(),
            CalculationType = MonthlyAmountPerChargeResultProducedV1.Types.CalculationType.FirstCorrectionSettlement,
            QuantityUnit = MonthlyAmountPerChargeResultProducedV1.Types.QuantityUnit.Kwh,
            PeriodStartUtc = wholesaleResult.PeriodStart.ToTimestamp(),
            PeriodEndUtc = wholesaleResult.PeriodEnd.ToTimestamp(),
            GridAreaCode = wholesaleResult.GridArea,
            EnergySupplierId = wholesaleResult.EnergySupplierId,
            ChargeCode = wholesaleResult.ChargeCode,
            ChargeType = MonthlyAmountPerChargeResultProducedV1.Types.ChargeType.Tariff,
            ChargeOwnerId = wholesaleResult.ChargeOwnerId,
            IsTax = wholesaleResult.IsTax,
            Amount = wholesaleResult.TimeSeriesPoints.Single().Amount,
        };

        return monthlyAmountPerChargeResultProducedV1;
    }
}
