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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.Fixtures;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.Factories;

public class WholesaleResultFactoryTests
{
    private const decimal DefaultPrice = 1.123456m;
    private const decimal DefaultAmount = 2.345678m;
    private readonly Instant _defaultPeriodStart = Instant.FromUtc(2022, 5, 1, 0, 0);
    private readonly Instant _defaultPeriodEnd = Instant.FromUtc(2022, 5, 2, 0, 0);
    private static readonly Instant _defaultTime = Instant.FromUtc(2022, 5, 1, 1, 0);
    private readonly IEnumerable<QuantityQuality> _quantityQualities = new List<QuantityQuality> { QuantityQuality.Measured, QuantityQuality.Missing };
    private readonly List<WholesaleTimeSeriesPoint> _defaultWholesaleTimeSeriesPoints = new()
    {
        new WholesaleTimeSeriesPoint(_defaultTime.ToDateTimeOffset(), 1.0m, new List<QuantityQuality> { QuantityQuality.Measured,  QuantityQuality.Missing }, DefaultPrice, DefaultAmount),
    };

    [Fact]
    public void CreateWholesaleResult_ReturnExpectedWholesaleResult()
    {
        // Arrange
        var row = CreateDefaultSqlResultRow();

        // Act
        var actual = WholesaleResultFactory.CreateWholesaleResult(row, _defaultWholesaleTimeSeriesPoints, _defaultPeriodStart, _defaultPeriodEnd);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.ChargeType.Should().Be(ChargeType.Tariff);
        actual.ChargeCode.Should().Be("chargeCode");
        actual.ChargeOwnerId.Should().Be("chargeOwnerId");
        actual.EnergySupplierId.Should().Be("energySupplierId");
        actual.GridArea.Should().Be("504");
        actual.Id.Should().Be(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-0123456789ab"));
        actual.AmountType.Should().Be(AmountType.AmountPerCharge);
        actual.IsTax.Should().BeTrue();
        actual.Resolution.Should().Be(Resolution.Hour);
        actual.MeteringPointType.Should().Be(MeteringPointType.Consumption);
        actual.SettlementMethod.Should().Be(SettlementMethod.Flex);
        actual.PeriodEnd.Should().Be(_defaultPeriodEnd);
        actual.PeriodStart.Should().Be(_defaultPeriodStart);
        actual.CalculationType.Should().Be(ProcessType.WholesaleFixing);
        actual.QuantityUnit.Should().Be(QuantityUnit.Kwh);
        actual.TimeSeriesPoints.Should().HaveCount(1);
    }

    private static TestSqlResultRow CreateDefaultSqlResultRow()
    {
        var list = new List<KeyValuePair<string, string>>
        {
            new(WholesaleResultColumnNames.CalculationId, "bbbbbbbb-aaaa-bbbb-cccc-0123456789ab"),
            new(WholesaleResultColumnNames.CalculationResultId, "aaaaaaaa-bbbb-cccc-dddd-0123456789ab"),
            new(WholesaleResultColumnNames.EnergySupplierId, "energySupplierId"),
            new(WholesaleResultColumnNames.GridArea, "504"),
            new(WholesaleResultColumnNames.AmountType, "amount_per_charge"),
            new(WholesaleResultColumnNames.CalculationType, "WholesaleFixing"),
            new(WholesaleResultColumnNames.ChargeCode, "chargeCode"),
            new(WholesaleResultColumnNames.ChargeType, "tariff"),
            new(WholesaleResultColumnNames.ChargeOwnerId, "chargeOwnerId"),
            new(WholesaleResultColumnNames.QuantityUnit, "kWh"),
            new(WholesaleResultColumnNames.Resolution, "PT1H"),
            new(WholesaleResultColumnNames.MeteringPointType, "consumption"),
            new(WholesaleResultColumnNames.SettlementMethod, "flex"),
            new(WholesaleResultColumnNames.IsTax, "true"),
        };
        return new TestSqlResultRow(list);
    }
}
