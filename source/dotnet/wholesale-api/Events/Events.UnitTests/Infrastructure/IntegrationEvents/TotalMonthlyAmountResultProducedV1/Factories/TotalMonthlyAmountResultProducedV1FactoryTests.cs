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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TotalMonthlyAmountResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Common;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.TotalMonthlyAmountResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.UnitTests.Fixtures;
using FluentAssertions;
using Xunit;
using TotalMonthlyAmountResultProducedV1CalculationType = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.TotalMonthlyAmountResultProducedV1.Types.CalculationType;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.TotalMonthlyAmountResultProducedV1.Factories;

public class TotalMonthlyAmountResultProducedV1FactoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public void Create_ReturnsExpectedValue(TotalMonthlyAmountResultProducedV1Factory sut)
    {
        // Arrange
        var totalMonthlyAmountResult = new TotalMonthlyAmountResultBuilder()
            .Build();

        var expected = CreateExpected(totalMonthlyAmountResult);

        // Act
        var actual = sut.Create(totalMonthlyAmountResult);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(CalculationType.Aggregation)]
    [InlineData(CalculationType.BalanceFixing)]
    public void Create_WhenUnexpectedCalculationType_ThrowsException(CalculationType calculationType)
    {
        // Arrange
        var totalMonthlyAmountResult = new TotalMonthlyAmountResultBuilder()
            .WithCalculationType(calculationType)
            .Build();
        var sut = new TotalMonthlyAmountResultProducedV1Factory();

        // Act
        var act = () => sut.Create(totalMonthlyAmountResult);

        // Act and Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(CalculationType.WholesaleFixing)]
    [InlineData(CalculationType.FirstCorrectionSettlement)]
    [InlineData(CalculationType.SecondCorrectionSettlement)]
    [InlineData(CalculationType.ThirdCorrectionSettlement)]
    public void Create_WhenExpectedCalculationType_ReturnsExpectedValue(CalculationType calculationType)
    {
        // Arrange
        var totalMonthlyAmountResult = new TotalMonthlyAmountResultBuilder()
            .WithCalculationType(calculationType)
            .Build();
        var expected = CreateExpected(totalMonthlyAmountResult);
        var sut = new TotalMonthlyAmountResultProducedV1Factory();

        // Act
        var actual = sut.Create(totalMonthlyAmountResult);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Create_WithoutChargeOwner_ReturnsExpectedValue(TotalMonthlyAmountResultProducedV1Factory sut)
    {
        // Arrange
        var totalMonthlyAmountResult = new TotalMonthlyAmountResultBuilder()
            .WithChargeOwner(null!)
            .Build();
        var expected = CreateExpected(totalMonthlyAmountResult);

        // Act
        var actual = sut.Create(totalMonthlyAmountResult);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    private static Contracts.IntegrationEvents.TotalMonthlyAmountResultProducedV1 CreateExpected(TotalMonthlyAmountResult totalMonthlyAmountResult)
    {
        var totalMonthlyAmountResultProducedV1 = new Contracts.IntegrationEvents.TotalMonthlyAmountResultProducedV1
        {
            CalculationId = totalMonthlyAmountResult.CalculationId.ToString(),
            CalculationType = MapCalculationType(totalMonthlyAmountResult.CalculationType),
            PeriodStartUtc = totalMonthlyAmountResult.PeriodStart.ToTimestamp(),
            PeriodEndUtc = totalMonthlyAmountResult.PeriodEnd.ToTimestamp(),
            GridAreaCode = totalMonthlyAmountResult.GridAreaCode,
            EnergySupplierId = totalMonthlyAmountResult.EnergySupplierId,
            Currency = Contracts.IntegrationEvents.TotalMonthlyAmountResultProducedV1.Types.Currency.Dkk,
            Amount = totalMonthlyAmountResult.Amount,
            CalculationResultVersion = totalMonthlyAmountResult.Version,
        };

        if (totalMonthlyAmountResult.ChargeOwnerId is not null)
            totalMonthlyAmountResultProducedV1.ChargeOwnerId = totalMonthlyAmountResult.ChargeOwnerId;

        return totalMonthlyAmountResultProducedV1;
    }

    private static TotalMonthlyAmountResultProducedV1CalculationType MapCalculationType(CalculationType calculationType)
    {
        return calculationType switch
        {
            CalculationType.WholesaleFixing => TotalMonthlyAmountResultProducedV1CalculationType.WholesaleFixing,
            CalculationType.FirstCorrectionSettlement => TotalMonthlyAmountResultProducedV1CalculationType.FirstCorrectionSettlement,
            CalculationType.SecondCorrectionSettlement => TotalMonthlyAmountResultProducedV1CalculationType.SecondCorrectionSettlement,
            CalculationType.ThirdCorrectionSettlement => TotalMonthlyAmountResultProducedV1CalculationType.ThirdCorrectionSettlement,
            _ => throw new ArgumentOutOfRangeException(
                nameof(calculationType),
                actualValue: calculationType,
                "Value cannot be mapped to a calculation type."),
        };
    }
}
