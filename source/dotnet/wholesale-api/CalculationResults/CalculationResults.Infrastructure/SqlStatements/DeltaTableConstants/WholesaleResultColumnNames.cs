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

using System.Reflection;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;

public class WholesaleResultColumnNames
{
    public const string CalculationId = "calculation_id";
    public const string CalculationExecutionTimeStart = "calculation_execution_time_start";
    public const string CalculationType = "calculation_type";
    public const string CalculationResultId = "calculation_result_id";
    public const string GridArea = "grid_area_code";
    public const string EnergySupplierId = "energy_supplier_id";
    public const string Time = "time";
    public const string Quantity = "quantity";

    public const string QuantityUnit = "quantity_unit";
    public const string QuantityQualities = "quantity_qualities";
    public const string Resolution = "resolution";
    public const string MeteringPointType = "metering_point_type";
    public const string SettlementMethod = "settlement_method";
    public const string Price = "price";
    public const string Amount = "amount";
    public const string IsTax = "is_tax";
    public const string ChargeCode = "charge_code";
    public const string ChargeType = "charge_type";
    public const string ChargeOwnerId = "charge_owner_id";
    public const string AmountType = "amount_type";

    public static IReadOnlyCollection<string> GetAllNames()
    {
        var fieldInfos = typeof(WholesaleResultColumnNames).GetFields(BindingFlags.Public | BindingFlags.Static);
        return fieldInfos.Select(x => x.GetValue(null)).Cast<string>().ToList();
    }

    public static string GetType(string columnName) =>
        columnName switch
        {
            CalculationId => "string",
            CalculationExecutionTimeStart => "timestamp",
            CalculationType => "string",
            CalculationResultId => "string",
            GridArea => "string",
            EnergySupplierId => "string",
            Resolution => "string",
            MeteringPointType => "string",
            SettlementMethod => "string",
            ChargeCode => "string",
            ChargeType => "string",
            ChargeOwnerId => "string",
            IsTax => "boolean",
            Time => "timestamp",
            Quantity => "decimal(18,3)",
            QuantityUnit => "string",
            QuantityQualities => "array<string>",
            Price => "decimal(18,6)",
            Amount => "decimal(18,6)",
            AmountType => "string",
            _ => throw new ArgumentOutOfRangeException(nameof(columnName), actualValue: columnName, "Unexpected column name."),
        };
}
