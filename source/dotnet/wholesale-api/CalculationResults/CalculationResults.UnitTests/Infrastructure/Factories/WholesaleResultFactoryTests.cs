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
using FluentAssertions;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.Factories;

public class WholesaleResultFactoryTests
{
    private const decimal DefaultPrice = 1.123456m;
    private const decimal DefaultAmount = 2.345678m;
    private readonly Instant _defaultPeriodStart = Instant.FromUtc(2022, 5, 1, 0, 0);
    private readonly Instant _defaultPeriodEnd = Instant.FromUtc(2022, 5, 2, 0, 0);
    private readonly Instant _defaultTime = Instant.FromUtc(2022, 5, 1, 1, 0);
    private readonly IEnumerable<QuantityQuality> _quantityQualities = new List<QuantityQuality> { QuantityQuality.Measured,  QuantityQuality.Missing };

    [Fact]
    public void ToInstant_WhenValueIsNull_ReturnsNull()
    {
         // Arrange
         var wholesaleTimeSeriesPoints = new List<WholesaleTimeSeriesPoint>
         {
             new(_defaultTime.ToDateTimeOffset(), 1.0m, _quantityQualities, DefaultPrice, DefaultAmount),
         };
         var row = CreateDefaultSqlResultRow();

         // Act
         var actual = WholesaleResultFactory.CreateWholesaleResult(row, wholesaleTimeSeriesPoints, _defaultPeriodStart, _defaultPeriodEnd);

         // Assert
         actual.Should().BeNull();
    }

    private static TestSqlResultRow CreateDefaultSqlResultRow()
    {
        var list = new List<KeyValuePair<string, string>>
        {
            new(WholesaleResultColumnNames.BatchId, "bbbbbbbb-aaaa-bbbb-cccc-0123456789ab"),
            new(WholesaleResultColumnNames.CalculationResultId, "aaaaaaaa-bbbb-cccc-dddd-0123456789ab"),
            new(WholesaleResultColumnNames.EnergySupplierId, "energySupplierId"),
            new(WholesaleResultColumnNames.GridArea, "504"),
            new(WholesaleResultColumnNames.BatchProcessType, "WholesaleFixing"),
            new(WholesaleResultColumnNames.ChargeCode, "chargeCode"),
            new(WholesaleResultColumnNames.ChargeType, "tariff"),
            new(WholesaleResultColumnNames.ChargeOwnerId, "chargeOwnerId"),
            new(WholesaleResultColumnNames.QuantityUnit, "kWh"),
            new(WholesaleResultColumnNames.MeteringPointType, "consumption"),
            new(WholesaleResultColumnNames.SettlementMethod, "flex"),
            new(WholesaleResultColumnNames.IsTax, "True"),
        };
        return new TestSqlResultRow(list);
    }
}
