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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.EnergyResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public static class SettlementReportEnergyResultRowFactory
{
    public static SettlementReportEnergyResultRow Create(DatabricksSqlRow databricksSqlRow, bool isEnergySupplierIncluded)
    {
        var calculationType = databricksSqlRow[SettlementReportEnergyResultViewColumns.CalculationType];
        var gridArea = databricksSqlRow[SettlementReportEnergyResultViewColumns.GridArea];
        var startTime = databricksSqlRow[SettlementReportEnergyResultViewColumns.Time];
        var resolution = databricksSqlRow[SettlementReportEnergyResultViewColumns.Resolution];
        var quantity = databricksSqlRow[SettlementReportEnergyResultViewColumns.Quantity];
        var meteringPointType = databricksSqlRow[SettlementReportEnergyResultViewColumns.MeteringPointType];
        var settlementMethod = databricksSqlRow[SettlementReportEnergyResultViewColumns.SettlementMethod];
        var energySupplierId = isEnergySupplierIncluded
            ? databricksSqlRow[SettlementReportEnergyResultPerEnergySupplierViewColumns.EnergySupplier]
            : null;

        return new SettlementReportEnergyResultRow(
            CalculationTypeMapper.FromDeltaTableValue(calculationType!),
            SqlResultValueConverters.ToInstant(startTime)!.Value,
            gridArea!,
            energySupplierId,
            ResolutionMapper.FromDeltaTableValue(resolution!),
            MeteringPointTypeMapper.FromDeltaTableValue(meteringPointType)!,
            SettlementMethodMapper.FromDeltaTableValue(settlementMethod)!,
            SqlResultValueConverters.ToDecimal(quantity)!.Value);
    }
}
