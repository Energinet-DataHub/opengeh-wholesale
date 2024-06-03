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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public static class SettlementReportWholesaleResultRowFactory
{
    public static SettlementReportWholesaleResultRow Create(DatabricksSqlRow databricksSqlRow, long version)
    {
        var calculationId = databricksSqlRow[SettlementReportWholesaleViewColumns.CalculationId];
        var calculationType = databricksSqlRow[SettlementReportWholesaleViewColumns.CalculationType];
        var gridArea = databricksSqlRow[SettlementReportWholesaleViewColumns.GridArea];
        var energySupplierId = databricksSqlRow[SettlementReportWholesaleViewColumns.EnergySupplierId];
        var startTime = databricksSqlRow[SettlementReportWholesaleViewColumns.StartDateTime];
        var resolution = databricksSqlRow[SettlementReportWholesaleViewColumns.Resolution];
        var quantityUnit = databricksSqlRow[SettlementReportWholesaleViewColumns.QuantityUnit];
        var quantity = databricksSqlRow[SettlementReportWholesaleViewColumns.Quantity];
        var price = databricksSqlRow[SettlementReportWholesaleViewColumns.Price];
        var amount = databricksSqlRow[SettlementReportWholesaleViewColumns.Amount];
        var chargeType = databricksSqlRow[SettlementReportWholesaleViewColumns.ChargeType];
        var chargeCode = databricksSqlRow[SettlementReportWholesaleViewColumns.ChargeCode];
        var chargeOwnerId = databricksSqlRow[SettlementReportWholesaleViewColumns.ChargeOwnerId];
        var meteringPointType = databricksSqlRow[SettlementReportWholesaleViewColumns.MeteringPointType];
        var settlementMethod = databricksSqlRow[SettlementReportWholesaleViewColumns.SettlementMethod];

        return new SettlementReportWholesaleResultRow(
            SqlResultValueConverters.ToGuid(calculationId!),
            CalculationTypeMapper.FromDeltaTableValue(calculationType!),
            gridArea!,
            energySupplierId!,
            SqlResultValueConverters.ToInstant(startTime!)!.Value,
            ResolutionMapper.FromDeltaTableValue(resolution!),
            MeteringPointTypeMapper.FromDeltaTableValue(meteringPointType!)!,
            SettlementMethodMapper.FromDeltaTableValue(settlementMethod!)!,
            QuantityUnitMapper.FromDeltaTableValue(quantityUnit!),
            Currency.DKK,
            SqlResultValueConverters.ToDecimal(quantity),
            SqlResultValueConverters.ToDecimal(price),
            SqlResultValueConverters.ToDecimal(amount),
            ChargeTypeMapper.FromDeltaTableValue(chargeType!),
            chargeCode,
            chargeOwnerId!,
            version);
    }
}
