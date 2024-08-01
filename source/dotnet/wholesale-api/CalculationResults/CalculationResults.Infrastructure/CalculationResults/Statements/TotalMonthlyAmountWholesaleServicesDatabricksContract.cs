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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public class TotalMonthlyAmountWholesaleServicesDatabricksContract : IWholesaleServicesDatabricksContract
{
    public AmountType GetAmountType()
    {
        return AmountType.TotalMonthlyAmount;
    }

    public string GetSource(DeltaTableOptions tableOptions)
    {
        return
            $"{tableOptions.WholesaleCalculationResultsSchemaName}.{tableOptions.TOTAL_MONTHLY_AMOUNTS_V1_VIEW_NAME}";
    }

    public string GetCalculationTypeColumnName()
    {
        return TotalMonthlyAmountsViewColumnNames.CalculationType;
    }

    public string GetGridAreaCodeColumnName()
    {
        return TotalMonthlyAmountsViewColumnNames.GridAreaCode;
    }

    public string GetTimeColumnName()
    {
        return TotalMonthlyAmountsViewColumnNames.Time;
    }

    public string GetEnergySupplierIdColumnName()
    {
        return TotalMonthlyAmountsViewColumnNames.EnergySupplierId;
    }

    public string GetChargeOwnerIdColumnName()
    {
        return TotalMonthlyAmountsViewColumnNames.ChargeOwnerId;
    }

    public string GetChargeCodeColumnName()
    {
        throw new InvalidOperationException("Oh dear, there is no charge code for total monthly amounts");
    }

    public string GetChargeTypeColumnName()
    {
        throw new InvalidOperationException("Oh dear, there is no charge type for total monthly amounts");
    }

    public string GetCalculationVersionColumnName()
    {
        return TotalMonthlyAmountsViewColumnNames.CalculationVersion;
    }

    public string GetCalculationIdColumnName()
    {
        return TotalMonthlyAmountsViewColumnNames.CalculationId;
    }

    public string[] GetColumnsToProject()
    {
        return ColumnsToProjectForTotalMonthlyAmounts;
    }

    public string[] GetColumnsToAggregateBy()
    {
        return ColumnsToAggregateByForTotalMonthlyAmounts;
    }

    private static string[] ColumnsToAggregateByForTotalMonthlyAmounts =>
    [
        TotalMonthlyAmountsViewColumnNames.GridAreaCode,
        TotalMonthlyAmountsViewColumnNames.EnergySupplierId,
        TotalMonthlyAmountsViewColumnNames.ChargeOwnerId,
    ];

    private static string[] ColumnsToProjectForTotalMonthlyAmounts =>
    [
        TotalMonthlyAmountsViewColumnNames.CalculationId,
        TotalMonthlyAmountsViewColumnNames.CalculationType,
        TotalMonthlyAmountsViewColumnNames.CalculationVersion,
        TotalMonthlyAmountsViewColumnNames.CalculationResultId,
        TotalMonthlyAmountsViewColumnNames.GridAreaCode,
        TotalMonthlyAmountsViewColumnNames.EnergySupplierId,
        TotalMonthlyAmountsViewColumnNames.ChargeOwnerId,
        TotalMonthlyAmountsViewColumnNames.Currency,
        TotalMonthlyAmountsViewColumnNames.Time,
        TotalMonthlyAmountsViewColumnNames.Amount,
    ];
}
