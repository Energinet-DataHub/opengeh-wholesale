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

public class EnergyPerGaAggregatedTimeSeriesDatabricksContract : IAggregatedTimeSeriesDatabricksContract
{
    public string GetAggregationLevel()
    {
        return DeltaTableAggregationLevel.GridArea;
    }

    public string GetSource(DeltaTableOptions tableOptions)
    {
        return
            $"{tableOptions.WholesaleCalculationResultsSchemaName}.{tableOptions.ENERGY_PER_GA_V1_VIEW_NAME}";
    }

    public string GetTimeColumnName()
    {
        return EnergyPerGaViewColumnNames.Time;
    }

    public string GetCalculationVersionColumnName()
    {
        return EnergyPerGaViewColumnNames.CalculationVersion;
    }

    public string[] GetColumnsToProject()
    {
        return
        [
            EnergyPerGaViewColumnNames.CalculationId,
            EnergyPerGaViewColumnNames.CalculationType,
            //EnergyPerGaViewColumnNames.CalculationPeriodStart,
            //EnergyPerGaViewColumnNames.CalculationPeriodEnd,
            EnergyPerGaViewColumnNames.CalculationVersion,
            EnergyPerGaViewColumnNames.ResultId,
            EnergyPerGaViewColumnNames.GridAreaCode,
            EnergyPerGaViewColumnNames.MeteringPointType,
            EnergyPerGaViewColumnNames.SettlementMethod,
            EnergyPerGaViewColumnNames.Resolution,
            EnergyPerGaViewColumnNames.Time,
            EnergyPerGaViewColumnNames.Quantity,
            //EnergyPerGaViewColumnNames.QuantityUnit,
            EnergyPerGaViewColumnNames.QuantityQualities,
        ];
    }

    public string[] GetColumnsToAggregateBy()
    {
        return
        [
            EnergyPerGaViewColumnNames.GridAreaCode,
            EnergyPerGaViewColumnNames.MeteringPointType,
            EnergyPerGaViewColumnNames.SettlementMethod,
        ];
    }
}
