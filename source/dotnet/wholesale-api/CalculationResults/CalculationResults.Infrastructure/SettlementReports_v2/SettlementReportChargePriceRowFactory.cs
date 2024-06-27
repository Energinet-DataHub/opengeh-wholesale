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
    public static SettlementReportChargePriceRow Create(DatabricksSqlRow databricksSqlRow)
    {
        var chargeType = databricksSqlRow[SettlementReportChargePriceViewColumns.ChargeType];
        var chargeCode = databricksSqlRow[SettlementReportChargePriceViewColumns.ChargeCode];
        var chargeOwnerId = databricksSqlRow[SettlementReportChargePriceViewColumns.ChargeOwnerId];
        var resolution = databricksSqlRow[SettlementReportChargePriceViewColumns.Resolution];
        var taxIndicator = databricksSqlRow[SettlementReportChargePriceViewColumns.Taxation];
        var startTime = databricksSqlRow[SettlementReportChargePriceViewColumns.StartTime];
        var energyPrices = databricksSqlRow[SettlementReportChargePriceViewColumns.EnergyPrices];

        return new SettlementReportChargePriceRow(
            ChargeTypeMapper.FromDeltaTableValue(chargeType!),
            chargeCode!,
            chargeOwnerId!,
            ResolutionMapper.FromDeltaTableValue(resolution!),
            SqlResultValueConverters.ToBool(taxIndicator!),
            SqlResultValueConverters.ToInstant(startTime)!.Value,
            EnergyPriceTypeMapper.FromDeltaTableValue(energyPrices)!);
    }
}
