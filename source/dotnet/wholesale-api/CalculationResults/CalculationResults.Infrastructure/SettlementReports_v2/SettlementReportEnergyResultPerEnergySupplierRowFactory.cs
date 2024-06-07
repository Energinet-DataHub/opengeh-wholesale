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

public static class SettlementReportEnergyResultPerEnergySupplierRowFactory
{
    public static SettlementReportEnergyResultRow Create(DatabricksSqlRow databricksSqlRow, long version)
    {
        var calculationType = databricksSqlRow[SettlementReportEnergyResultPerEnergySupplierViewColumns.CalculationType];
        var gridArea = databricksSqlRow[SettlementReportEnergyResultPerEnergySupplierViewColumns.GridArea];
        var startTime = databricksSqlRow[SettlementReportEnergyResultPerEnergySupplierViewColumns.Time];
        var resolution = databricksSqlRow[SettlementReportEnergyResultPerEnergySupplierViewColumns.Resolution];
        var quantity = databricksSqlRow[SettlementReportEnergyResultPerEnergySupplierViewColumns.Quantity];
        var meteringPointType = databricksSqlRow[SettlementReportEnergyResultPerEnergySupplierViewColumns.MeteringPointType];
        var settlementMethod = databricksSqlRow[SettlementReportEnergyResultPerEnergySupplierViewColumns.SettlementMethod];
        var energySupplierId = databricksSqlRow[SettlementReportEnergyResultPerEnergySupplierViewColumns.EnergySupplier];

        return new SettlementReportEnergyResultRow(
            CalculationTypeMapper.FromDeltaTableValue(calculationType!),
            SqlResultValueConverters.ToInstant(startTime!)!.Value,
            gridArea!,
            energySupplierId,
            ResolutionMapper.FromDeltaTableValue(resolution!),
            MeteringPointTypeMapper.FromDeltaTableValue(meteringPointType!)!,
            SettlementMethodMapper.FromDeltaTableValue(settlementMethod!)!,
            SqlResultValueConverters.ToDecimal(quantity)!.Value);
    }
}
