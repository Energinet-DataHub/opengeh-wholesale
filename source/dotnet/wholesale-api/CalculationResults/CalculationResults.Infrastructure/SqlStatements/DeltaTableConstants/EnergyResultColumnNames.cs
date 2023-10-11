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
    public const string BatchId = "calculation_id";
    public const string BatchExecutionTimeStart = "calculation_execution_time_start";
    public const string BatchProcessType = "calculation_type";
    public const string CalculationResultId = "calculation_result_id";
    public const string GridArea = "grid_area";
    public const string EnergySupplierId = "energy_supplier_id";
    public const string Time = "time";
    public const string Quantity = "quantity";

    public const string BalanceResponsibleId = "balance_responsible_id";
    public const string QuantityQuality = "quantity_quality";
    public const string AggregationLevel = "aggregation_level";
    public const string TimeSeriesType = "time_series_type";
    public const string FromGridArea = "out_grid_area";

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
             QuantityQuality => "string",
             AggregationLevel => "string",
             TimeSeriesType => "string",
             BatchId => "string",
             BatchProcessType => "string",
             BatchExecutionTimeStart => "timestamp",
             FromGridArea => "string",
             Quantity => "decimal(18,3)",
             _ => throw new ArgumentException($"Unexpected column name '{columnName}'."),
         };
}
