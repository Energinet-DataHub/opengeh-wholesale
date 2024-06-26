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

public static class SettlementReportMonthlyAmountRowFactory
{
    public static SettlementReportMonthlyAmountRow Create(DatabricksSqlRow databricksSqlRow)
    {
        var calculationType = databricksSqlRow[SettlementReportMonthlyAmountViewColumns.CalculationType];
        var gridArea = databricksSqlRow[SettlementReportMonthlyAmountViewColumns.GridAreaCode];
        var energySupplierId = databricksSqlRow[SettlementReportMonthlyAmountViewColumns.EnergySupplierId];
        var startTime = databricksSqlRow[SettlementReportMonthlyAmountViewColumns.Time];
        var resolution = databricksSqlRow[SettlementReportMonthlyAmountViewColumns.Resolution];
        var quantityUnit = databricksSqlRow[SettlementReportMonthlyAmountViewColumns.QuantityUnit];
        var amount = databricksSqlRow[SettlementReportMonthlyAmountViewColumns.Amount];
        var chargeType = databricksSqlRow[SettlementReportMonthlyAmountViewColumns.ChargeType];
        var chargeCode = databricksSqlRow[SettlementReportMonthlyAmountViewColumns.ChargeCode];
        var chargeOwnerId = databricksSqlRow[SettlementReportMonthlyAmountViewColumns.ChargeOwnerId];

        return new SettlementReportMonthlyAmountRow(
            CalculationTypeMapper.FromDeltaTableValue(calculationType!),
            gridArea!,
            energySupplierId!,
            SqlResultValueConverters.ToInstant(startTime)!.Value,
            ResolutionMapper.FromDeltaTableValue(resolution!),
            QuantityUnitMapper.FromDeltaTableValue(quantityUnit!),
            Currency.DKK,
            SqlResultValueConverters.ToDecimal(amount),
            chargeType is null ? null : ChargeTypeMapper.FromDeltaTableValue(chargeType),
            chargeCode,
            chargeOwnerId);
    }
}
