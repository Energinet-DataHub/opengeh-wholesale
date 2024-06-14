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

public class SettlementReportChargePriceViewColumns
{
    public const string CalculationId = "calculation_id";
    public const string CalculationType = "calculation_type";
    public const string CalculationVersion = "calculation_version";
    public const string ChargeType = "charge_type";
    public const string ChargeCode = "charge_code";
    public const string ChargeOwnerId = "charge_owner_id";
    public const string Resolution = "resolution";
    public const string Taxation = "is_tax";
    public const string StartTime = "start_date_time";
    public const string EnergyPrices = "price_points";
    public const string GridArea = "grid_area_code";
    public const string EnergySupplierId = "energy_supplier_id";

    public static Dictionary<string, (string Type, bool Nullable)> SchemaDefinition { get; } = new()
    {
        { CalculationId, ("string", false) },
        { CalculationType, ("string", false) },
        { CalculationVersion, ("bigint", false) },
        { ChargeType, ("string", false) },
        { ChargeCode, ("string", false) },
        { ChargeOwnerId, ("string", false) },
        { Resolution, ("string", false) },
        { Taxation, ("boolean", false) },
        { StartTime, ("timestamp", false) },
        { EnergyPrices, ("array<struct<time:timestamp,price:decimal(18,6)>>", false) },
        { GridArea, ("string", false) },
        { EnergySupplierId, ("string", false) },
    };
}
