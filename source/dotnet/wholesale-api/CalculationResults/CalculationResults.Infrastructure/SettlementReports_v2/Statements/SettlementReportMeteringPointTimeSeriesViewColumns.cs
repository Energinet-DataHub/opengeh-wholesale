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

public class SettlementReportMeteringPointTimeSeriesViewColumns
{
    public const string CalculationId = "calculation_id";
    public const string CalculationType = "calculation_type";
    public const string MeteringPointId = "metering_point_id";
    public const string MeteringPointType = "metering_point_type";
    public const string Resolution = "resolution";
    public const string GridArea = "grid_area_code";
    public const string EnergySupplier = "energy_supplier_id";
    public const string ObservationTime = "observation_time";
    public const string Quantity = "quantity";

    public static Dictionary<string, (string Type, bool Nullable)> SchemaDefinition { get; } = new()
    {
        { CalculationId, ("string", false) },
        { CalculationType, ("string", false) },
        { MeteringPointId, ("string", false) },
        { MeteringPointType, ("string", false) },
        { Resolution, ("string", false) },
        { GridArea, ("string", false) },
        { EnergySupplier, ("string", false) },
        { ObservationTime, ("timestamp", false) },
        { Quantity, ("decimal(18,3)", false) },
    };

    public static string[] AllNames { get; } = [.. SchemaDefinition.Keys];
}
