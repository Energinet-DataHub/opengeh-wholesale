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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using NodaTime;
using NodaTime.Text;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;

public static class SqlForSettlementReport
{
    public static string CreateSqlStatement(
        string[] gridAreaCodes,
        ProcessType processType,
        Instant periodStart,
        Instant periodEnd,
        string? energySupplier)
    {
        // TODO: Handle energy supplier
        var selectColumns = string.Join(", ", ResultColumnNames.GridArea, ResultColumnNames.BatchProcessType, ResultColumnNames.Time, ResultColumnNames.TimeSeriesType, ResultColumnNames.Quantity);
        var processTypeString = MapFrom(processType);
        var gridAreas = string.Join(",", gridAreaCodes);
        var startTimeString = periodStart.ToString();
        var endTimeString = periodEnd.ToString();

        return
            $@"SELECT {selectColumns} FROM wholesale_output.result WHERE {ResultColumnNames.GridArea} IN ({gridAreas}) WHERE {ResultColumnNames.BatchProcessType} = {processTypeString} WHERE {ResultColumnNames.Time} BETWEEN '{startTimeString}' AND '{endTimeString}' order by time";
    }

    public static IEnumerable<SettlementReportResultRow> CreateSettlementReportRows(
        TableRows rows)
    {
        return Enumerable.Range(0, rows.Count)
            .Select(i => new SettlementReportResultRow(
                rows[i, ResultColumnNames.GridArea],
                MapToProcessType(rows[i, ResultColumnNames.BatchProcessType]),
                InstantPattern.ExtendedIso.Parse(rows[i, ResultColumnNames.Time]).Value,
                "PT15M", // TODO: store resolution in delta table?
                MapToMeteringPointType(rows[i, ResultColumnNames.TimeSeriesType]),
                MapToSettlementMethod(rows[i, ResultColumnNames.TimeSeriesType])));
    }

    // TODO: Move to mapper
    private static string MapFrom(ProcessType processType) =>
        processType switch
        {
            ProcessType.BalanceFixing => "BalanceFixing",
            ProcessType.Aggregation => "Aggregation",
            _ => throw new NotImplementedException($"Cannot map process type '{processType}"),
        };
}
