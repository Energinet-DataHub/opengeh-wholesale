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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public class EnergyPerEsBrpGaAggregatedTimeSeriesDatabricksContract : IAggregatedTimeSeriesDatabricksContract
{
    public string GetAggregationLevel()
    {
        return DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea;
    }

    public string GetSource(DeltaTableOptions tableOptions)
    {
        return
            $"{tableOptions.WholesaleCalculationResultsSchemaName}.{tableOptions.ENERGY_PER_ES_BRP_GA_V1_VIEW_NAME}";
    }

    public string[] GetColumnsToProject()
    {
        return
        [
            EnergyPerEsBrpGaViewColumnNames.CalculationId,
            EnergyPerEsBrpGaViewColumnNames.CalculationType,
            //EnergyPerEsBrpGaViewColumnNames.CalculationPeriodStart,
            //EnergyPerEsBrpGaViewColumnNames.CalculationPeriodEnd,
            EnergyPerEsBrpGaViewColumnNames.CalculationVersion,
            EnergyPerEsBrpGaViewColumnNames.ResultId,
            EnergyPerEsBrpGaViewColumnNames.GridAreaCode,
            EnergyPerEsBrpGaViewColumnNames.EnergySupplierId,
            EnergyPerEsBrpGaViewColumnNames.BalanceResponsiblePartyId,
            EnergyPerEsBrpGaViewColumnNames.MeteringPointType,
            EnergyPerEsBrpGaViewColumnNames.SettlementMethod,
            EnergyPerEsBrpGaViewColumnNames.Resolution,
            EnergyPerEsBrpGaViewColumnNames.Time,
            EnergyPerEsBrpGaViewColumnNames.Quantity,
            //EnergyPerEsBrpGaViewColumnNames.QuantityUnit,
            EnergyPerEsBrpGaViewColumnNames.QuantityQualities,
        ];
    }

    public string[] GetColumnsToAggregateBy()
    {
        return
        [
            EnergyPerEsBrpGaViewColumnNames.GridAreaCode,
            EnergyPerEsBrpGaViewColumnNames.MeteringPointType,
            EnergyPerEsBrpGaViewColumnNames.SettlementMethod,
        ];
    }
}
