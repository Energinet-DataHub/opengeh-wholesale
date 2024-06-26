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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public static class SettlementReportChargeLinkPeriodsRowFactory
{
    public static SettlementReportChargeLinkPeriodsRow Create(DatabricksSqlRow databricksSqlRow)
    {
        var periodStart = databricksSqlRow[SettlementReportChargeLinkPeriodsViewColumns.FromDate];
        var periodEnd = databricksSqlRow[SettlementReportChargeLinkPeriodsViewColumns.ToDate];
        var quantity = databricksSqlRow[SettlementReportChargeLinkPeriodsViewColumns.Quantity];
        var chargeType = databricksSqlRow[SettlementReportChargeLinkPeriodsViewColumns.ChargeType];
        var chargeCode = databricksSqlRow[SettlementReportChargeLinkPeriodsViewColumns.ChargeCode];
        var chargeOwnerId = databricksSqlRow[SettlementReportChargeLinkPeriodsViewColumns.ChargeOwnerId];
        var meteringPointId = databricksSqlRow[SettlementReportChargeLinkPeriodsViewColumns.MeteringPointId];
        var meteringPointType = databricksSqlRow[SettlementReportChargeLinkPeriodsViewColumns.MeteringPointType];

        return new SettlementReportChargeLinkPeriodsRow(
            meteringPointId!,
            MeteringPointTypeMapper.FromDeltaTableValueNonNull(meteringPointType!), // The model definition says this can't be null, so this should be ok
            ChargeTypeMapper.FromDeltaTableValue(chargeType!),
            chargeOwnerId!,
            chargeCode!,
            SqlResultValueConverters.ToInt(quantity)!.Value,
            SqlResultValueConverters.ToInstant(periodStart)!.Value,
            periodEnd is null ? null : SqlResultValueConverters.ToInstant(periodEnd)!.Value);
    }
}
