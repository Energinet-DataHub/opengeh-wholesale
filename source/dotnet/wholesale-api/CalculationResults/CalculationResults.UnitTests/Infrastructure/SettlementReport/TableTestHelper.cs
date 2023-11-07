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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SettlementReport;

public static class TableTestHelper
{
    public static TableChunk CreateTableForSettlementReport(int rowCount)
    {
        var columnNames = new[]
        {
            EnergyResultColumnNames.GridArea,
            EnergyResultColumnNames.BatchProcessType,
            EnergyResultColumnNames.Time,
            EnergyResultColumnNames.TimeSeriesType,
            EnergyResultColumnNames.Quantity,
        };
        var rows = new List<string[]>();
        for (var row = 0; row < rowCount; row++)
        {
            var dummyQuantity = $@"{row}.1";
            rows.Add(new[] { "123", "BalanceFixing", "2022-05-16T01:00:00.000Z", "non_profiled_consumption", $@"{dummyQuantity}" });
        }

        return new TableChunk(columnNames, rows);
    }
}
