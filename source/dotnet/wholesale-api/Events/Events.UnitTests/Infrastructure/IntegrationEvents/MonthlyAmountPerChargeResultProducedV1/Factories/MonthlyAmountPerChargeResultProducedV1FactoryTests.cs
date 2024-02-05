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
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Common;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.UnitTests.Fixtures;
using FluentAssertions;
using Xunit;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Factories;

public class MonthlyAmountPerChargeResultProducedV1FactoryTests
{
    private readonly WholesaleTimeSeriesPoint _someTimeSeriesPoint =
        new(new DateTime(2021, 1, 1), 1, new[] { QuantityQuality.Measured }, 2, 3);

    [Theory]
    [InlineData(AmountType.AmountPerCharge, false)]
    [InlineData(AmountType.MonthlyAmountPerCharge, true)]
    [InlineData(AmountType.TotalMonthlyAmount, false)]
    public void CanCreate_WhenAmountType_ReturnsExpectedValue(
        AmountType amountType, bool expected)
    {
        // Arrange
        var wholesaleResult = new WholesaleResultBuilder()
            .WithResolution(Resolution.Month)
            .WithAmountType(amountType)
            .Build();
        var sut = new MonthlyAmountPerChargeResultProducedV1Factory();

        // Act
        var actual = sut.CanCreate(wholesaleResult);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(Resolution.Hour, false)]
    [InlineData(Resolution.Day, false)]
    [InlineData(Resolution.Month, true)]
    public void CanCreate_WhenResolution_ReturnsExpectedValue(
        Resolution resolution, bool expected)
    {
        // Arrange
        var wholesaleResult = new WholesaleResultBuilder()
            .WithResolution(resolution)
            .WithAmountType(AmountType.MonthlyAmountPerCharge)
            .Build();
        var sut = new MonthlyAmountPerChargeResultProducedV1Factory();

        // Act
        var actual = sut.CanCreate(wholesaleResult);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void CanCreate_WhenTimeSeriesLength_ReturnsExpectedValue(
        int numberOfTimeSeriesPoints, bool expected)
    {
        // Arrange
        var timeSeriesPoints = new List<WholesaleTimeSeriesPoint>();
        for (var p = 0; p < numberOfTimeSeriesPoints; p++)
            timeSeriesPoints.Add(_someTimeSeriesPoint);

        var wholesaleResult = new WholesaleResultBuilder()
            .WithResolution(Resolution.Month)
            .WithAmountType(AmountType.MonthlyAmountPerCharge)
            .WithTimeSeriesPoints(timeSeriesPoints).Build();
        var sut = new MonthlyAmountPerChargeResultProducedV1Factory();

        // Act
        var actual = sut.CanCreate(wholesaleResult);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Create_ReturnsExpectedObject(
        MonthlyAmountPerChargeResultProducedV1Factory sut)
    {
        // Arrange
        var wholesaleResult = new WholesaleResultBuilder()
            .WithResolution(Resolution.Month)
            .WithAmountType(AmountType.MonthlyAmountPerCharge)
            .Build();
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
        var sut = new MonthlyAmountPerChargeResultProducedV1Factory();
        var wholesaleResult = new WholesaleResultBuilder()
            .WithCalculationType(calculationType)
            .WithResolution(Resolution.Month)
            .WithAmountType(AmountType.MonthlyAmountPerCharge).Build();

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
        var wholesaleResult = new WholesaleResultBuilder()
            .WithTimeSeriesPoints(timeSeriesPoints)
            .WithResolution(Resolution.Month)
            .WithAmountType(AmountType.MonthlyAmountPerCharge).Build();

        // Act
        var act = () => sut.Create(wholesaleResult);

        // Act and Assert
        act.Should().Throw<ArgumentException>();
    }

    private static Contracts.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1 CreateExpected(WholesaleResult wholesaleResult)
    {
        var monthlyAmountPerChargeResultProducedV1 = new Contracts.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1
        {
            CalculationId = wholesaleResult.CalculationId.ToString(),
            CalculationType = Contracts.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Types.CalculationType.WholesaleFixing,
            QuantityUnit = Contracts.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Types.QuantityUnit.Kwh,
            PeriodStartUtc = wholesaleResult.PeriodStart.ToTimestamp(),
            PeriodEndUtc = wholesaleResult.PeriodEnd.ToTimestamp(),
            GridAreaCode = wholesaleResult.GridArea,
            EnergySupplierId = wholesaleResult.EnergySupplierId,
            ChargeCode = wholesaleResult.ChargeCode,
            ChargeType = Contracts.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Types.ChargeType.Tariff,
            ChargeOwnerId = wholesaleResult.ChargeOwnerId,
            IsTax = wholesaleResult.IsTax,
            Currency = Contracts.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Types.Currency.Dkk,
            Amount = wholesaleResult.TimeSeriesPoints.Single().Amount,
        };

        return monthlyAmountPerChargeResultProducedV1;
    }
}
