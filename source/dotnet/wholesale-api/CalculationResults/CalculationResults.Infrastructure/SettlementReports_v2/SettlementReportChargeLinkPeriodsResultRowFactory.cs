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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public static class SettlementReportChargeLinkPeriodsResultRowFactory
{
    public static SettlementReportChargeLinkPeriodsResultRow Create(DatabricksSqlRow databricksSqlRow, long version)
    {
        var periodStart = databricksSqlRow[SettlementReportChargeLinkPeriodsResultQueryStatement.ColumnNames.PeriodStart];
        var periodEnd = databricksSqlRow[SettlementReportChargeLinkPeriodsResultQueryStatement.ColumnNames.PeriodEnd];
        var quantity = databricksSqlRow[SettlementReportChargeLinkPeriodsResultQueryStatement.ColumnNames.Quantity];
        var chargeType = databricksSqlRow[SettlementReportChargeLinkPeriodsResultQueryStatement.ColumnNames.ChargeType];
        var chargeCode = databricksSqlRow[SettlementReportChargeLinkPeriodsResultQueryStatement.ColumnNames.ChargeCode];
        var chargeOwnerId = databricksSqlRow[SettlementReportChargeLinkPeriodsResultQueryStatement.ColumnNames.ChargeOwnerId];
        var meteringPointId = databricksSqlRow[SettlementReportChargeLinkPeriodsResultQueryStatement.ColumnNames.MeteringPointId];
        var meteringPointType = databricksSqlRow[SettlementReportChargeLinkPeriodsResultQueryStatement.ColumnNames.MeteringPointType];

        return new SettlementReportChargeLinkPeriodsResultRow(
            meteringPointId!,
            MeteringPointTypeMapper.FromDeltaTableValue(meteringPointType),
            ChargeTypeMapper.FromDeltaTableValue(chargeType!),
            chargeOwnerId!,
            chargeCode!,
            SqlResultValueConverters.ToInt(quantity)!.Value,
            SqlResultValueConverters.ToInstant(periodStart)!.Value,
            SqlResultValueConverters.ToInstant(periodEnd)!.Value);
    }
}
