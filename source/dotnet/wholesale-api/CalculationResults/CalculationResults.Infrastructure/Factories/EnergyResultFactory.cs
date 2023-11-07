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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;

public class EnergyResultFactory
{
    public static EnergyResult CreateEnergyResult(
        IDictionary<string, object> sqlResultRow,
        List<EnergyTimeSeriesPoint> timeSeriesPoints,
        Instant periodStart,
        Instant periodEnd)
    {
        var id = Convert.ToString(sqlResultRow[EnergyResultColumnNames.CalculationResultId]);
        var batchId = sqlResultRow[EnergyResultColumnNames.BatchId].ToString();
        var gridArea = sqlResultRow[EnergyResultColumnNames.GridArea].ToString();
        var timeSeriesType = sqlResultRow[EnergyResultColumnNames.TimeSeriesType].ToString();
        var energySupplierId = sqlResultRow[EnergyResultColumnNames.EnergySupplierId].ToString();
        var balanceResponsibleId = sqlResultRow[EnergyResultColumnNames.BalanceResponsibleId].ToString();
        var processType = Convert.ToString(sqlResultRow[EnergyResultColumnNames.BatchProcessType]);
        var fromGridArea = Convert.ToString(sqlResultRow[EnergyResultColumnNames.FromGridArea]);

        return new EnergyResult(
            SqlResultValueConverters.ToGuid(id!),
            Guid.Parse(batchId!),
            gridArea!,
            SqlResultValueConverters.ToTimeSeriesType(timeSeriesType!),
            energySupplierId,
            balanceResponsibleId,
            timeSeriesPoints.ToArray(),
            ProcessTypeMapper.FromDeltaTableValue(processType!),
            periodStart,
            periodEnd,
            fromGridArea);
    }
}
