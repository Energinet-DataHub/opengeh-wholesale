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

public class WholesaleServicesRelationalAlgebraHelper
{
    public string GetProjection(string prefix, AmountType amountType)
    {
        return $"{string.Join(", ", Enumerable.Select<string, string>(GetColumnsToProject(amountType), cts => $"`{prefix}`.`{cts}`"))}";
    }

    public string GetCalculationTypeColumnName(AmountType amountType)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => AmountsPerChargeViewColumnNames.CalculationType,
            AmountType.MonthlyAmountPerCharge => MonthlyAmountsPerChargeViewColumnNames.CalculationType,
            AmountType.TotalMonthlyAmount => TotalMonthlyAmountsViewColumnNames.CalculationType,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public string GetGridAreaCodeColumnName(AmountType amountType)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => AmountsPerChargeViewColumnNames.GridAreaCode,
            AmountType.MonthlyAmountPerCharge => MonthlyAmountsPerChargeViewColumnNames.GridAreaCode,
            AmountType.TotalMonthlyAmount => TotalMonthlyAmountsViewColumnNames.GridAreaCode,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public string GetSource(AmountType amountType, DeltaTableOptions tableOptions)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => $"{tableOptions.WholesaleCalculationResultsSchemaName}.{tableOptions.AMOUNTS_PER_CHARGE_V1_VIEW_NAME}",
            AmountType.MonthlyAmountPerCharge => $"{tableOptions.WholesaleCalculationResultsSchemaName}.{tableOptions.MONTHLY_AMOUNTS_V1_VIEW_NAME}",
            AmountType.TotalMonthlyAmount => $"{tableOptions.WholesaleCalculationResultsSchemaName}.{tableOptions.TOTAL_MONTHLY_AMOUNTS_V1_VIEW_NAME}",
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public string[] GetColumnsToProject(AmountType amountType)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => ColumnsToProjectForAmountsPerCharge,
            AmountType.MonthlyAmountPerCharge => ColumnsToProjectForMonthlyAmountsPerCharge,
            AmountType.TotalMonthlyAmount => ColumnsToProjectForTotalMonthlyAmounts,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public string[] GetColumnsToAggregateBy(AmountType amountType)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => ColumnsToAggregateByForAmountsPerCharge,
            AmountType.MonthlyAmountPerCharge => ColumnsToAggregateByForMonthlyAmountsPerCharge,
            AmountType.TotalMonthlyAmount => ColumnsToAggregateByForTotalMonthlyAmounts,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private static string[] ColumnsToAggregateByForAmountsPerCharge =>
    [
        AmountsPerChargeViewColumnNames.GridAreaCode,
        AmountsPerChargeViewColumnNames.EnergySupplierId,
        AmountsPerChargeViewColumnNames.ChargeOwnerId,
        AmountsPerChargeViewColumnNames.ChargeType,
        AmountsPerChargeViewColumnNames.ChargeCode,
        AmountsPerChargeViewColumnNames.Resolution,
        AmountsPerChargeViewColumnNames.MeteringPointType,
        AmountsPerChargeViewColumnNames.SettlementMethod,
    ];

    private static string[] ColumnsToAggregateByForMonthlyAmountsPerCharge =>
    [
        MonthlyAmountsPerChargeViewColumnNames.GridAreaCode,
        MonthlyAmountsPerChargeViewColumnNames.EnergySupplierId,
        MonthlyAmountsPerChargeViewColumnNames.ChargeOwnerId,
        MonthlyAmountsPerChargeViewColumnNames.ChargeType,
        MonthlyAmountsPerChargeViewColumnNames.ChargeCode,
    ];

    private static string[] ColumnsToAggregateByForTotalMonthlyAmounts =>
    [
        TotalMonthlyAmountsViewColumnNames.GridAreaCode,
        TotalMonthlyAmountsViewColumnNames.EnergySupplierId,
        TotalMonthlyAmountsViewColumnNames.ChargeOwnerId,
    ];

    private static string[] ColumnsToProjectForAmountsPerCharge =>
    [
        AmountsPerChargeViewColumnNames.CalculationId,
        AmountsPerChargeViewColumnNames.CalculationType,
        AmountsPerChargeViewColumnNames.CalculationVersion,
        AmountsPerChargeViewColumnNames.CalculationResultId,
        AmountsPerChargeViewColumnNames.GridAreaCode,
        AmountsPerChargeViewColumnNames.EnergySupplierId,
        AmountsPerChargeViewColumnNames.ChargeCode,
        AmountsPerChargeViewColumnNames.ChargeType,
        AmountsPerChargeViewColumnNames.ChargeOwnerId,
        AmountsPerChargeViewColumnNames.Resolution,
        AmountsPerChargeViewColumnNames.QuantityUnit,
        AmountsPerChargeViewColumnNames.MeteringPointType,
        AmountsPerChargeViewColumnNames.SettlementMethod,
        AmountsPerChargeViewColumnNames.IsTax,
        AmountsPerChargeViewColumnNames.Currency,
        AmountsPerChargeViewColumnNames.Time,
        AmountsPerChargeViewColumnNames.Quantity,
        AmountsPerChargeViewColumnNames.QuantityQualities,
        AmountsPerChargeViewColumnNames.Price,
        AmountsPerChargeViewColumnNames.Amount,
    ];

    private static string[] ColumnsToProjectForMonthlyAmountsPerCharge =>
    [
        MonthlyAmountsPerChargeViewColumnNames.CalculationId,
        MonthlyAmountsPerChargeViewColumnNames.CalculationType,
        MonthlyAmountsPerChargeViewColumnNames.CalculationVersion,
        MonthlyAmountsPerChargeViewColumnNames.CalculationResultId,
        MonthlyAmountsPerChargeViewColumnNames.GridAreaCode,
        MonthlyAmountsPerChargeViewColumnNames.EnergySupplierId,
        MonthlyAmountsPerChargeViewColumnNames.ChargeCode,
        MonthlyAmountsPerChargeViewColumnNames.ChargeType,
        MonthlyAmountsPerChargeViewColumnNames.ChargeOwnerId,
        MonthlyAmountsPerChargeViewColumnNames.QuantityUnit,
        MonthlyAmountsPerChargeViewColumnNames.IsTax,
        MonthlyAmountsPerChargeViewColumnNames.Currency,
        MonthlyAmountsPerChargeViewColumnNames.Time,
        MonthlyAmountsPerChargeViewColumnNames.Amount,
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
