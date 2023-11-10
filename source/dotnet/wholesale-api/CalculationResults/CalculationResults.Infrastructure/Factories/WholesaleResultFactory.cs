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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;

public class WholesaleResultFactory
{
    public static WholesaleResult CreateWholesaleResult(
        DatabricksSqlRow databricksSqlRow,
        IReadOnlyCollection<WholesaleTimeSeriesPoint> wholesaleTimeSeriesPoints,
        Instant periodStart,
        Instant periodEnd)
    {
        var id = databricksSqlRow[WholesaleResultColumnNames.CalculationResultId];
        var calculationId = databricksSqlRow[WholesaleResultColumnNames.CalculationId];
        var calculationType = databricksSqlRow[WholesaleResultColumnNames.CalculationType];
        var gridArea = databricksSqlRow[WholesaleResultColumnNames.GridArea];
        var energySupplierId = databricksSqlRow[WholesaleResultColumnNames.EnergySupplierId];
        var amountType = databricksSqlRow[WholesaleResultColumnNames.AmountType];
        var chargeCode = databricksSqlRow[WholesaleResultColumnNames.ChargeCode];
        var chargeType = databricksSqlRow[WholesaleResultColumnNames.ChargeType];
        var chargeOwnerId = databricksSqlRow[WholesaleResultColumnNames.ChargeOwnerId];
        var isTax = databricksSqlRow[WholesaleResultColumnNames.IsTax];
        var quantityUnit = databricksSqlRow[WholesaleResultColumnNames.QuantityUnit];
        var resolution = databricksSqlRow[WholesaleResultColumnNames.Resolution];
        var meteringPointType = databricksSqlRow[WholesaleResultColumnNames.MeteringPointType];
        var settlementMethod = databricksSqlRow[WholesaleResultColumnNames.SettlementMethod];

        return new WholesaleResult(
            SqlResultValueConverters.ToGuid(id!),
            SqlResultValueConverters.ToGuid(calculationId!),
            ProcessTypeMapper.FromDeltaTableValue(calculationType!),
            periodStart,
            periodEnd,
            gridArea!,
            energySupplierId!,
            AmountTypeMapper.FromDeltaTableValue(amountType!),
            chargeCode!,
            ChargeTypeMapper.FromDeltaTableValue(chargeType!),
            chargeOwnerId!,
            SqlResultValueConverters.ToBool(isTax!),
            QuantityUnitMapper.FromDeltaTableValue(quantityUnit!),
            ResolutionMapper.FromDeltaTableValue(resolution!),
            gridArea!,
            energySupplierId!,
            AmountTypeMapper.FromDeltaTableValue(amountType!),
            chargeCode!,
            ChargeTypeMapper.FromDeltaTableValue(chargeType!),
            chargeOwnerId!,
            SqlResultValueConverters.ToBool(isTax!),
            QuantityUnitMapper.FromDeltaTableValue(quantityUnit!),
            ResolutionMapper.FromDeltaTableValue(resolution!),
            MeteringPointTypeMapper.FromDeltaTableValue(meteringPointType),
            SettlementMethodMapper.FromDeltaTableValue(settlementMethod!),
            wholesaleTimeSeriesPoints);
    }
}
