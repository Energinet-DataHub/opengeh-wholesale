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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;

public class SettlementReportMonthlyAmountViewColumns
{
    public const string CalculationId = "calculation_id";
    public const string CalculationType = "calculation_type";
    public const string ResultId = "result_id";
    public const string GridAreaCode = "grid_area_code";
    public const string EnergySupplierId = "energy_supplier_id";
    public const string Time = "time";
    public const string QuantityUnit = "quantity_unit";
    public const string Amount = "amount";
    public const string ChargeType = "charge_type";
    public const string ChargeCode = "charge_code";
    public const string ChargeOwnerId = "charge_owner_id";
    public const string IsTax = "is_tax";

    public static Dictionary<string, (string Type, bool Nullable)> SchemaDefinition { get; } = new()
    {
        { CalculationId, ("string", false) },
        { CalculationType, ("string", false) },
        { ResultId, ("string", false) },
        { GridAreaCode, ("string", false) },
        { EnergySupplierId, ("string", false) },
        { Time, ("timestamp", false) },
        { QuantityUnit, ("string", false) },
        { Amount, ("decimal(18,6)", true) },
        { ChargeType, ("string", true) },
        { ChargeCode, ("string", true) },
        { ChargeOwnerId, ("string", true) },
        { IsTax, ("boolean", true) },
    };
}
