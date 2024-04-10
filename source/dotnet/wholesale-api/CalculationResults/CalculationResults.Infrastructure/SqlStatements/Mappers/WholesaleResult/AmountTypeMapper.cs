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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.WholesaleResult;

public static class AmountTypeMapper
{
    private const string AmountPerCharge = "amount_per_charge";
    private const string MonthlyAmountPerCharge = "monthly_amount_per_charge";
    private const string TotalMonthlyAmount = "total_monthly_amount";

    public static AmountType FromDeltaTableValue(string amountType) =>
        amountType switch
        {
            AmountPerCharge => AmountType.AmountPerCharge,
            MonthlyAmountPerCharge => AmountType.MonthlyAmountPerCharge,
            TotalMonthlyAmount => AmountType.TotalMonthlyAmount,
            _ => throw new ArgumentOutOfRangeException(
                nameof(amountType),
                actualValue: amountType,
                "Value does not contain a valid string representation of a amount type."),
        };

    public static string ToDeltaTableValue(AmountType amountType) =>
        amountType switch
        {
            AmountType.AmountPerCharge => AmountPerCharge,
            AmountType.MonthlyAmountPerCharge => MonthlyAmountPerCharge,
            AmountType.TotalMonthlyAmount => TotalMonthlyAmount,
            _ => throw new ArgumentOutOfRangeException(
                nameof(amountType),
                actualValue: amountType,
                $"Cannot map ${nameof(AmountType)} to delta table value"),
        };
}
