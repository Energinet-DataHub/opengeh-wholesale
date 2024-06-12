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

public class SettlementReportChargeLinkPeriodsViewColumns
{
    public const string CalculationId = "calculation_id";
    public const string CalculationType = "calculation_type";
    public const string MeteringPointId = "metering_point_id";
    public const string MeteringPointType = "metering_point_type";
    public const string ChargeType = "charge_type";
    public const string ChargeCode = "charge_code";
    public const string ChargeOwnerId = "charge_owner_id";
    public const string Quantity = "quantity";
    public const string FromDate = "from_date";
    public const string ToDate = "to_date";
    public const string GridArea = "grid_area_code";
    public const string EnergySupplierId = "energy_supplier_id";

    public static Dictionary<string, (string Type, bool Nullable)> SchemaDefinition { get; } = new()
    {
        { CalculationId, ("string", false) },
        { CalculationType, ("string", false) },
        { MeteringPointId, ("string", false) },
        { MeteringPointType, ("string", false) },
        { ChargeType, ("string", false) },
        { ChargeCode, ("string", true) },
        { ChargeOwnerId, ("string", false) },
        { Quantity, ("decimal(18,3)", true) },
        { FromDate, ("timestamp", false) },
        { ToDate, ("timestamp", false) },
        { GridArea, ("string", false) },
        { EnergySupplierId, ("string", false) },
    };
}
