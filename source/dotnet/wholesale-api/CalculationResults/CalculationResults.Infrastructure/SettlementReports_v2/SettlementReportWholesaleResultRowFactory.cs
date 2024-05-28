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

using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public static class SettlementReportWholesaleResultRowFactory
{
    public static SettlementReportWholesaleResultRow Create(DatabricksSqlRow databricksSqlRow, long version)
    {
        var id = databricksSqlRow[SettlementReportWholesaleResultQueryStatement.ColumnNames.CalculationId];
        var calculationId = databricksSqlRow[SettlementReportWholesaleResultQueryStatement.ColumnNames.CalculationId];
        var calculationType = databricksSqlRow[SettlementReportWholesaleResultQueryStatement.ColumnNames.CalculationType];
        var gridArea = databricksSqlRow[SettlementReportWholesaleResultQueryStatement.ColumnNames.GridArea];
        var energySupplierId = databricksSqlRow[SettlementReportWholesaleResultQueryStatement.ColumnNames.EnergySupplierId];
        var startTime = databricksSqlRow[SettlementReportWholesaleResultQueryStatement.ColumnNames.Time];
        var resolution = databricksSqlRow[SettlementReportWholesaleResultQueryStatement.ColumnNames.Resolution];
        var quantityUnit = databricksSqlRow[SettlementReportWholesaleResultQueryStatement.ColumnNames.QuantityUnit];
        var quantity = databricksSqlRow[SettlementReportWholesaleResultQueryStatement.ColumnNames.Quantity];
        var price = databricksSqlRow[SettlementReportWholesaleResultQueryStatement.ColumnNames.Price];
        var amount = databricksSqlRow[SettlementReportWholesaleResultQueryStatement.ColumnNames.Amount];
        var chargeType = databricksSqlRow[SettlementReportWholesaleResultQueryStatement.ColumnNames.ChargeType];
        var chargeCode = databricksSqlRow[SettlementReportWholesaleResultQueryStatement.ColumnNames.ChargeCode];
        var chargeOwnerId = databricksSqlRow[SettlementReportWholesaleResultQueryStatement.ColumnNames.ChargeOwnerId];
        var meteringPointType = databricksSqlRow[SettlementReportWholesaleResultQueryStatement.ColumnNames.MeteringPointType];
        var settlementMethod = databricksSqlRow[SettlementReportWholesaleResultQueryStatement.ColumnNames.SettlementMethod];

        return new SettlementReportWholesaleResultRow(
            SqlResultValueConverters.ToGuid(id!),
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
