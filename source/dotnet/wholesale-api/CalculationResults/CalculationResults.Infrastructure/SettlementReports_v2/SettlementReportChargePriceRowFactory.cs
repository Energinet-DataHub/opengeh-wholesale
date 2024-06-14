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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public static class SettlementReportChargePriceRowFactory
{
    public static SettlementReportChargePriceRow Create(DatabricksSqlRow databricksSqlRow, long version)
    {
        var periodStart = databricksSqlRow[SettlementReportChargePriceViewColumns.PeriodStart];
        var periodEnd = databricksSqlRow[SettlementReportChargePriceViewColumns.PeriodEnd];
        var quantity = databricksSqlRow[SettlementReportChargePriceViewColumns.Quantity];
        var chargeType = databricksSqlRow[SettlementReportChargePriceViewColumns.ChargeType];
        var chargeCode = databricksSqlRow[SettlementReportChargePriceViewColumns.ChargeCode];
        var chargeOwnerId = databricksSqlRow[SettlementReportChargePriceViewColumns.ChargeOwnerId];
        var startTime = databricksSqlRow[SettlementReportChargePriceViewColumns.PeriodStart];
        var meteringPointType = databricksSqlRow[SettlementReportChargePriceViewColumns.MeteringPointType];
        var resolution = string.Empty;
        var taxIndicator = string.Empty;
        var energyPrice1 = string.Empty; //TODO: find the correct values

        return new SettlementReportChargePriceRow(
            ChargeTypeMapper.FromDeltaTableValue(chargeType!),
            chargeCode!,
            chargeOwnerId!,
            ResolutionMapper.FromDeltaTableValue(resolution!),
            taxIndicator,
            SqlResultValueConverters.ToInstant(startTime)!.Value,
            energyPrice1);
    }
}
