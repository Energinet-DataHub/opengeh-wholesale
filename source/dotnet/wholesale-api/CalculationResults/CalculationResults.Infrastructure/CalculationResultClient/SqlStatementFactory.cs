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

using System.Diagnostics.Eventing.Reader;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using Energinet.DataHub.Wholesale.Common.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;

public static class SqlStatementFactory
{
    public static string CreateForSettlementReport(
        string[] gridAreaCodes,
        ProcessType processType,
        Instant periodStart,
        Instant periodEnd,
        string? energySupplier)
    {
        return string.IsNullOrEmpty(energySupplier) ? GetSqlStatementForGridArea(gridAreaCodes, processType, periodStart, periodEnd) : GetSqlStatementForEnergySupplier(gridAreaCodes, processType, periodStart, periodEnd, energySupplier);
    }

    private static string GetSqlStatementForGridArea(
        string[] gridAreaCodes,
        ProcessType processType,
        Instant periodStart,
        Instant periodEnd)
    {
        var selectColumns = string.Join(", ", ResultColumnNames.GridArea, ResultColumnNames.BatchProcessType, ResultColumnNames.Time, ResultColumnNames.TimeSeriesType, ResultColumnNames.Quantity);
        var processTypeString = ProcessTypeMapper.ToDeltaTableValue(processType);
        var gridAreas = string.Join(",", gridAreaCodes);
        var startTimeString = periodStart.ToString();
        var endTimeString = periodEnd.ToString();
        var timeSeriesTypes = new List<TimeSeriesType> { TimeSeriesType.Production, TimeSeriesType.FlexConsumption, TimeSeriesType.NonProfiledConsumption, TimeSeriesType.NetExchangePerGa };
        var timeSeriesTypesString = CreateTimeSeriesString(timeSeriesTypes);

        return $@"
SELECT {selectColumns}
FROM wholesale_output.result
WHERE
    {ResultColumnNames.GridArea} IN ({gridAreas})
    AND {ResultColumnNames.TimeSeriesType} IN ({timeSeriesTypesString})
    AND {ResultColumnNames.BatchProcessType} = '{processTypeString}'
    AND {ResultColumnNames.Time} BETWEEN '{startTimeString}' AND '{endTimeString}'
    AND {ResultColumnNames.AggregationLevel} = 'total_ga'
ORDER by time
";
    }

    private static string GetSqlStatementForEnergySupplier(
        string[] gridAreaCodes,
        ProcessType processType,
        Instant periodStart,
        Instant periodEnd,
        string energySupplier)
    {
        var selectColumns = string.Join(", ", ResultColumnNames.GridArea, ResultColumnNames.BatchProcessType, ResultColumnNames.Time, ResultColumnNames.TimeSeriesType, ResultColumnNames.Quantity);
        var processTypeString = ProcessTypeMapper.ToDeltaTableValue(processType);
        var gridAreas = string.Join(",", gridAreaCodes);
        var startTimeString = periodStart.ToString();
        var endTimeString = periodEnd.ToString();
        var timeSeriesTypes = new List<TimeSeriesType> { TimeSeriesType.Production, TimeSeriesType.FlexConsumption, TimeSeriesType.NonProfiledConsumption };
        var timeSeriesTypesString = CreateTimeSeriesString(timeSeriesTypes);

        return $@"
SELECT {selectColumns}
FROM wholesale_output.result
WHERE
    {ResultColumnNames.GridArea} IN ({gridAreas})
    AND {ResultColumnNames.TimeSeriesType} IN ({timeSeriesTypesString})
    AND {ResultColumnNames.BatchProcessType} = '{processTypeString}'
    AND {ResultColumnNames.Time} BETWEEN '{startTimeString}' AND '{endTimeString}'
    AND {ResultColumnNames.AggregationLevel} = 'es_ga'
    AND {ResultColumnNames.EnergySupplierId} = '{energySupplier}'
ORDER by time
";
    }

    private static string CreateTimeSeriesString(IEnumerable<TimeSeriesType> timeSeriesTypes)
    {
        var timeSeriesTypesString = string.Join(",", timeSeriesTypes.Select(x => $"\'{TimeSeriesTypeMapper.ToDeltaTableValue(x)}\'"));
        return timeSeriesTypesString;
    }
}
