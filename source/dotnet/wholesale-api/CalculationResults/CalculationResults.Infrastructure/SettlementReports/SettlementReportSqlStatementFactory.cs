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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.EnergyResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;

public static class SettlementReportSqlStatementFactory
{
    public static string Create(
        string schemaName,
        string tableName,
        string[] gridAreaCodes,
        ProcessType processType,
        Instant periodStart,
        Instant periodEnd,
        string? energySupplier)
    {
        var isTotalGridArea = energySupplier == null;
        var aggregationLevel = isTotalGridArea ? DeltaTableAggregationLevel.GridArea : DeltaTableAggregationLevel.EnergySupplierAndGridArea;
        var selectColumns = string.Join(
            ", ",
            @$"t1.{EnergyResultColumnNames.GridArea}",
            @$"t1.{EnergyResultColumnNames.BatchProcessType}",
            @$"t1.{EnergyResultColumnNames.Time}",
            @$"t1.{EnergyResultColumnNames.TimeSeriesType}",
            @$"t1.{EnergyResultColumnNames.Quantity}");
        var processTypeString = ProcessTypeMapper.ToDeltaTableValue(processType);
        var gridAreas = string.Join(",", gridAreaCodes);
        var startTimeString = periodStart.ToString();
        var endTimeString = periodEnd.ToString();
        var timeSeriesTypesString = CreateTimeSeriesString(isTotalGridArea: isTotalGridArea);

        var sql = $@"
SELECT {selectColumns}
FROM {schemaName}.{tableName} t1
LEFT JOIN {schemaName}.{tableName} t2
    ON t1.{EnergyResultColumnNames.Time} = t2.{EnergyResultColumnNames.Time}
        AND t1.{EnergyResultColumnNames.BatchExecutionTimeStart} < t2.{EnergyResultColumnNames.BatchExecutionTimeStart}
        AND t1.{EnergyResultColumnNames.GridArea} = t2.{EnergyResultColumnNames.GridArea}
        AND COALESCE(t1.{EnergyResultColumnNames.FromGridArea}, 'N/A') = COALESCE(t2.{EnergyResultColumnNames.FromGridArea}, 'N/A')
        AND t1.{EnergyResultColumnNames.TimeSeriesType} = t2.{EnergyResultColumnNames.TimeSeriesType}
        AND t1.{EnergyResultColumnNames.BatchProcessType} = t2.{EnergyResultColumnNames.BatchProcessType}
        AND t1.{EnergyResultColumnNames.AggregationLevel} = t2.{EnergyResultColumnNames.AggregationLevel}
WHERE t2.time IS NULL
    AND t1.{EnergyResultColumnNames.GridArea} IN ({gridAreas})
    AND t1.{EnergyResultColumnNames.TimeSeriesType} IN ({timeSeriesTypesString})
    AND t1.{EnergyResultColumnNames.BatchProcessType} = '{processTypeString}'
    AND t1.{EnergyResultColumnNames.Time} BETWEEN '{startTimeString}' AND '{endTimeString}'
    AND t1.{EnergyResultColumnNames.AggregationLevel} = '{aggregationLevel}'";
        if (energySupplier != null)
        {
            sql += $@"
    AND t1.{EnergyResultColumnNames.EnergySupplierId} = '{energySupplier}'";
        }

        sql += @"
ORDER BY t1.time
";

        return sql;
    }

    private static string CreateTimeSeriesString(bool isTotalGridArea)
    {
        var timeSeriesTypes = new List<TimeSeriesType> { TimeSeriesType.Production, TimeSeriesType.FlexConsumption, TimeSeriesType.NonProfiledConsumption };
        if (isTotalGridArea)
        {
            timeSeriesTypes.Add(TimeSeriesType.NetExchangePerGa);
            timeSeriesTypes.Add(TimeSeriesType.TotalConsumption);
        }

        var timeSeriesTypesString = string.Join(",", timeSeriesTypes.Select(x => $"\'{TimeSeriesTypeMapper.ToDeltaTableValue(x)}\'"));
        return timeSeriesTypesString;
    }
}
