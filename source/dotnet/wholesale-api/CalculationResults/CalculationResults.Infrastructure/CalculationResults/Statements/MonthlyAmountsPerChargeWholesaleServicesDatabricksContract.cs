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

public sealed class
    MonthlyAmountsPerChargeWholesaleServicesDatabricksContract : IWholesaleServicesDatabricksContract
{
    public AmountType GetAmountType()
    {
        return AmountType.MonthlyAmountPerCharge;
    }

    public string GetSource(DeltaTableOptions tableOptions)
    {
        return
            $"{tableOptions.CalculationResultViewsSource}.{tableOptions.MONTHLY_AMOUNTS_PER_CHARGE_V1_VIEW_NAME}";
    }

    public string GetCalculationTypeColumnName()
    {
        return MonthlyAmountsPerChargeViewColumnNames.CalculationType;
    }

    public string GetGridAreaCodeColumnName()
    {
        return MonthlyAmountsPerChargeViewColumnNames.GridAreaCode;
    }

    public string GetTimeColumnName()
    {
        return MonthlyAmountsPerChargeViewColumnNames.Time;
    }

    public string GetEnergySupplierIdColumnName()
    {
        return MonthlyAmountsPerChargeViewColumnNames.EnergySupplierId;
    }

    public string GetChargeOwnerIdColumnName()
    {
        return MonthlyAmountsPerChargeViewColumnNames.ChargeOwnerId;
    }

    public string GetChargeCodeColumnName()
    {
        return MonthlyAmountsPerChargeViewColumnNames.ChargeCode;
    }

    public string GetChargeTypeColumnName()
    {
        return MonthlyAmountsPerChargeViewColumnNames.ChargeType;
    }

    public string GetCalculationVersionColumnName()
    {
        return MonthlyAmountsPerChargeViewColumnNames.CalculationVersion;
    }

    public string GetCalculationIdColumnName()
    {
        return MonthlyAmountsPerChargeViewColumnNames.CalculationId;
    }

    public string GetResolutionColumnName()
    {
        throw new InvalidOperationException("There is no resolution for monthly amounts");
    }

    public string GetIsTaxColumnName()
    {
        return MonthlyAmountsPerChargeViewColumnNames.IsTax;
    }

    public string[] GetColumnsToProject()
    {
        return ColumnsToProjectForMonthlyAmountsPerCharge;
    }

    public string[] GetColumnsToAggregateBy()
    {
        return ColumnsToAggregateByForMonthlyAmountsPerCharge;
    }

    private static string[] ColumnsToAggregateByForMonthlyAmountsPerCharge =>
    [
        MonthlyAmountsPerChargeViewColumnNames.GridAreaCode,
        MonthlyAmountsPerChargeViewColumnNames.EnergySupplierId,
        MonthlyAmountsPerChargeViewColumnNames.ChargeOwnerId,
        MonthlyAmountsPerChargeViewColumnNames.ChargeType,
        MonthlyAmountsPerChargeViewColumnNames.ChargeCode,
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
}
