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

public sealed class EnergyPerBrpGaAggregatedTimeSeriesDatabricksContract : IAggregatedTimeSeriesDatabricksContract
{
    public string GetAggregationLevel()
    {
        return DeltaTableAggregationLevel.BalanceResponsibleAndGridArea;
    }

    public string GetSource(DeltaTableOptions tableOptions)
    {
        return
            $"{tableOptions.CalculationResultViewsSource}.{tableOptions.ENERGY_PER_BRP_V1_VIEW_NAME}";
    }

    public string GetTimeColumnName()
    {
        return EnergyPerBrpGaViewColumnNames.Time;
    }

    public string GetCalculationVersionColumnName()
    {
        return EnergyPerBrpGaViewColumnNames.CalculationVersion;
    }

    public string GetCalculationTypeColumnName()
    {
        return EnergyPerBrpGaViewColumnNames.CalculationType;
    }

    public string GetGridAreaCodeColumnName()
    {
        return EnergyPerBrpGaViewColumnNames.GridAreaCode;
    }

    public string GetMeteringPointTypeColumnName()
    {
        return EnergyPerBrpGaViewColumnNames.MeteringPointType;
    }

    public string GetSettlementMethodColumnName()
    {
        return EnergyPerBrpGaViewColumnNames.SettlementMethod;
    }

    public string GetEnergySupplierIdColumnName()
    {
        throw new InvalidOperationException("Oh dear, there is no energy supplier for brp_ga aggregation");
    }

    public string GetBalanceResponsiblePartyIdColumnName()
    {
        return EnergyPerBrpGaViewColumnNames.BalanceResponsiblePartyId;
    }

    public string GetResolutionColumnName()
    {
        return EnergyPerBrpGaViewColumnNames.Resolution;
    }

    public string GetCalculationIdColumnName()
    {
        return EnergyPerBrpGaViewColumnNames.CalculationId;
    }

    public string[] GetColumnsToProject()
    {
        return
        [
            EnergyPerBrpGaViewColumnNames.CalculationId,
            EnergyPerBrpGaViewColumnNames.CalculationType,
            EnergyPerBrpGaViewColumnNames.CalculationVersion,
            EnergyPerBrpGaViewColumnNames.ResultId,
            EnergyPerBrpGaViewColumnNames.GridAreaCode,
            EnergyPerBrpGaViewColumnNames.BalanceResponsiblePartyId,
            EnergyPerBrpGaViewColumnNames.MeteringPointType,
            EnergyPerBrpGaViewColumnNames.SettlementMethod,
            EnergyPerBrpGaViewColumnNames.Resolution,
            EnergyPerBrpGaViewColumnNames.Time,
            EnergyPerBrpGaViewColumnNames.Quantity,
            EnergyPerBrpGaViewColumnNames.QuantityQualities,
        ];
    }

    public string[] GetColumnsToAggregateBy()
    {
        return
        [
            EnergyPerBrpGaViewColumnNames.GridAreaCode,
            EnergyPerBrpGaViewColumnNames.MeteringPointType,
            EnergyPerBrpGaViewColumnNames.SettlementMethod,
        ];
    }
}
