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

public class EnergyResultColumnNames
{
    public const string CalculationId = "calculation_id";
    public const string CalculationExecutionTimeStart = "calculation_execution_time_start";
    public const string CalculationType = "calculation_type";
    public const string CalculationResultId = "calculation_result_id";
    public const string GridArea = "grid_area_code";
    public const string EnergySupplierId = "energy_supplier_id";
    public const string Time = "time";
    public const string Quantity = "quantity";

    public const string BalanceResponsibleId = "balance_responsible_id";
    public const string QuantityQualities = "quantity_qualities";
    public const string AggregationLevel = "aggregation_level";
    public const string TimeSeriesType = "time_series_type";
    public const string NeighborGridArea = "neighbor_grid_area_code";
    public const string MeteringPointId = "metering_point_id";
    public const string Resolution = "resolution";

    public static IReadOnlyCollection<string> GetAllNames()
    {
        var fieldInfos = typeof(EnergyResultColumnNames).GetFields(BindingFlags.Public | BindingFlags.Static);
        return fieldInfos.Select(x => x.GetValue(null)).Cast<string>().ToList();
    }

    public static string GetType(string columnName) =>
         columnName switch
         {
             GridArea => "string",
             BalanceResponsibleId => "string",
             CalculationResultId => "string",
             EnergySupplierId => "string",
             Time => "timestamp",
             QuantityQualities => "array<string>",
             AggregationLevel => "string",
             TimeSeriesType => "string",
             CalculationId => "string",
             CalculationType => "string",
             CalculationExecutionTimeStart => "timestamp",
             NeighborGridArea => "string",
             Quantity => "decimal(18,3)",
             MeteringPointId => "string",
             Resolution => "string",
             _ => throw new ArgumentOutOfRangeException(nameof(columnName), actualValue: columnName, "Unexpected column name."),
         };
}
