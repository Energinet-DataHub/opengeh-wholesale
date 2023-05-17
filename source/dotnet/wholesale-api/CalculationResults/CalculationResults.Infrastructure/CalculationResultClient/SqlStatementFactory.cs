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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient.Mappers;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;

public static class SqlStatementFactory
{
    public static string CreateForSettlementReport(
        string[] gridAreaCodes,
        Interfaces.CalculationResultClient.ProcessType processType,
        Instant periodStart,
        Instant periodEnd,
        string? energySupplier)
    {
        // TODO: Handle energy supplier
        var selectColumns = string.Join(", ", ResultColumnNames.GridArea, ResultColumnNames.BatchProcessType, ResultColumnNames.Time, ResultColumnNames.TimeSeriesType, ResultColumnNames.Quantity);
        var processTypeString = ProcessTypeMapper.ToDeltaTableValue(processType);
        var gridAreas = string.Join(",", gridAreaCodes);
        var startTimeString = periodStart.ToString();
        var endTimeString = periodEnd.ToString();

        return
            $@"SELECT {selectColumns} FROM wholesale_output.result WHERE {ResultColumnNames.GridArea} IN ({gridAreas}) WHERE {ResultColumnNames.BatchProcessType} = {processTypeString} WHERE {ResultColumnNames.Time} BETWEEN '{startTimeString}' AND '{endTimeString}' order by time";
    }
}
