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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SettlementReport;

public class SettlementReportDataFactoryTests
{
    private readonly SettlementReportResultRow _firstRow;
    private readonly SettlementReportResultRow _lastRow;
    private readonly List<DatabricksSqlRow> _rows;

    public SettlementReportDataFactoryTests()
    {
        _rows = new List<DatabricksSqlRow>();
        var row1 = CreateRow("123", "BalanceFixing", "2022-05-16T01:00:00.000Z", "non_profiled_consumption", "1.1");
        var row2 = CreateRow("234", "BalanceFixing", "2022-05-16T01:15:00.000Z", "production", "2.2");
        var row3 = CreateRow("234", "BalanceFixing", "2022-05-16T01:30:00.000Z", "production", "3.3");
        _rows.Add(row1);
        _rows.Add(row2);
        _rows.Add(row3);

        _firstRow = new SettlementReportResultRow("123", CalculationType.BalanceFixing, Instant.FromUtc(2022, 5, 16, 1, 0, 0), "PT15M", MeteringPointType.Consumption, SettlementMethod.NonProfiled, new decimal(1.1));
        _lastRow = new SettlementReportResultRow("234", CalculationType.BalanceFixing, Instant.FromUtc(2022, 5, 16, 1, 30, 0), "PT15M", MeteringPointType.Production, null, new decimal(3.3));
    }

    [Fact]
    public void Create_ReturnExpectedNumberOfRows()
    {
        // Act
        var actual = SettlementReportDataFactory.Create(_rows);

        // Assert
        actual.Count().Should().Be(_rows.Count);
    }

    [Fact]
    public void Create_ReturnExpectedContent()
    {
        // Act
        var actual = SettlementReportDataFactory.Create(_rows);

        // Assert
        var actualRows = actual.ToList();
        actualRows.First().Should().BeEquivalentTo(_firstRow);
        actualRows.ToList().Last().Should().BeEquivalentTo(_lastRow);
    }

    private static DatabricksSqlRow CreateRow(string gridArea, string balanceFixing, string time, string timeSeriesType, string quantity)
    {
        var row = new Dictionary<string, object?>
        {
            { EnergyResultColumnNames.GridArea, gridArea },
            { EnergyResultColumnNames.CalculationType, balanceFixing },
            { EnergyResultColumnNames.Time, time },
            { EnergyResultColumnNames.TimeSeriesType, timeSeriesType },
            { EnergyResultColumnNames.Quantity, quantity },
        };
        return new DatabricksSqlRow(row);
    }
}
