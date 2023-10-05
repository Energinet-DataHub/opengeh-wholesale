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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;

public class EnergyResultFactory
{
    public static EnergyResult CreateEnergyResult(
        SqlResultRow sqlResultRow,
        List<EnergyTimeSeriesPoint> timeSeriesPoints,
        Instant periodStart,
        Instant periodEnd)
    {
        var id = SqlResultValueConverters.ToGuid(sqlResultRow[EnergyResultColumnNames.CalculationResultId]);
        var timeSeriesType =
            SqlResultValueConverters.ToTimeSeriesType(sqlResultRow[EnergyResultColumnNames.TimeSeriesType]);
        var energySupplierId = sqlResultRow[EnergyResultColumnNames.EnergySupplierId];
        var balanceResponsibleId = sqlResultRow[EnergyResultColumnNames.BalanceResponsibleId];
        var gridArea = sqlResultRow[EnergyResultColumnNames.GridArea];
        var fromGridArea = sqlResultRow[EnergyResultColumnNames.FromGridArea];
        var batchId = sqlResultRow[EnergyResultColumnNames.BatchId];
        var processType = sqlResultRow[EnergyResultColumnNames.BatchProcessType];

        return new EnergyResult(
            id,
            Guid.Parse(batchId),
            gridArea,
            timeSeriesType,
            energySupplierId,
            balanceResponsibleId,
            timeSeriesPoints.ToArray(),
            ProcessTypeMapper.FromDeltaTableValue(processType),
            periodStart,
            periodEnd,
            fromGridArea);
    }
}
