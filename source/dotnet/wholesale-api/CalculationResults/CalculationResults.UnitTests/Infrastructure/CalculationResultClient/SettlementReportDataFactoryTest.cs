﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Wholesale.CalculationResults.Application;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Common.Models;
using FluentAssertions;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResultClient;

[UnitTest]
public class SettlementReportDataFactoryTests
{
    private readonly Table _table;
    private readonly SettlementReportResultRow _firstRow;
    private readonly SettlementReportResultRow _lastRow;

    public SettlementReportDataFactoryTests()
    {
        var columnNames = new List<string>()
        {
            ResultColumnNames.GridArea,
            ResultColumnNames.BatchProcessType,
            ResultColumnNames.Time,
            ResultColumnNames.TimeSeriesType,
            ResultColumnNames.Quantity,
        };
        var rows = new List<string[]>()
        {
            new[] { "123", "BalanceFixing", "2022-05-16T01:00:00.000Z", "non_profiled_consumption", "1.1" },
            new[] { "234", "BalanceFixing", "2022-05-16T01:15:00.000Z", "production", "2.2" },
            new[] { "234", "BalanceFixing", "2022-05-16T01:30:00.000Z", "production", "3.3" },
        };
        _table = new Table(columnNames, rows);
        _firstRow = new SettlementReportResultRow("123", ProcessType.BalanceFixing, Instant.FromUtc(2022, 5, 16, 1, 0, 0), "PT15M", MeteringPointType.Consumption, SettlementMethod.NonProfiled, new decimal(1.1));
        _lastRow = new SettlementReportResultRow("234", ProcessType.BalanceFixing, Instant.FromUtc(2022, 5, 16, 1, 30, 0), "PT15M", MeteringPointType.Production, null, new decimal(3.3));
    }

    [Fact]
    public void Create_ReturnExpectedNumberOfRows()
    {
        // Act
        var actual = SettlementReportDataFactory.Create(_table);

        // Assert
        actual.Count().Should().Be(_table.RowCount);
    }

    [Fact]
    public void Create_ReturnExpectedContent()
    {
        // Act
        var actual = SettlementReportDataFactory.Create(_table);

        // Assert
        var actualRows = actual.ToList();
        actualRows.First().Should().BeEquivalentTo(_firstRow);
        actualRows.ToList().Last().Should().BeEquivalentTo(_lastRow);
    }
}
