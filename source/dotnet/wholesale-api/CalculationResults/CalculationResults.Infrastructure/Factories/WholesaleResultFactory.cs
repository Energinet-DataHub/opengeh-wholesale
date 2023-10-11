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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;

public class WholesaleResultFactory
{
    public static WholesaleResult CreateWholesaleResult(
        SqlResultRow sqlResultRow,
        IReadOnlyCollection<WholesaleTimeSeriesPoint> wholesaleTimeSeriesPoints,
        Instant periodStart,
        Instant periodEnd)
    {
        var id = sqlResultRow[WholesaleResultColumnNames.CalculationResultId];
        var energySupplierId = sqlResultRow[WholesaleResultColumnNames.EnergySupplierId];
        var gridArea = sqlResultRow[WholesaleResultColumnNames.GridArea];
        var calculationId = sqlResultRow[WholesaleResultColumnNames.CalculationId];
        var calculationType = sqlResultRow[WholesaleResultColumnNames.CalculationType];
        var chargeCode = sqlResultRow[WholesaleResultColumnNames.ChargeCode];
        var chargeType = sqlResultRow[WholesaleResultColumnNames.ChargeType];
        var chargeOwnerId = sqlResultRow[WholesaleResultColumnNames.ChargeOwnerId];
        var quantityUnit = sqlResultRow[WholesaleResultColumnNames.QuantityUnit];
        var chargeResolution = sqlResultRow[WholesaleResultColumnNames.ChargeResolution];
        var meteringPointType = sqlResultRow[WholesaleResultColumnNames.MeteringPointType];
        var settlementMethod = sqlResultRow[WholesaleResultColumnNames.SettlementMethod];
        var isTax = sqlResultRow[WholesaleResultColumnNames.IsTax];

        return new WholesaleResult(
            SqlResultValueConverters.ToGuid(id),
            SqlResultValueConverters.ToGuid(calculationId),
            ProcessTypeMapper.FromDeltaTableValue(calculationType),
            periodStart,
            periodEnd,
            gridArea,
            energySupplierId,
            chargeCode,
            ChargeTypeMapper.FromDeltaTableValue(chargeType),
            chargeOwnerId,
            SqlResultValueConverters.ToBool(isTax),
            QuantityUnitMapper.FromDeltaTableValue(quantityUnit),
            ChargeResolutionMapper.FromDeltaTableValue(chargeResolution),
            MeteringPointTypeMapper.FromDeltaTableValue(meteringPointType),
            SettlementMethodMapper.FromDeltaTableValue(settlementMethod),
            wholesaleTimeSeriesPoints);
    }
}
