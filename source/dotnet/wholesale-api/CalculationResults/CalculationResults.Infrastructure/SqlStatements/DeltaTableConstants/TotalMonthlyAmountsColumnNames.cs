﻿// Copyright 2020 Energinet DataHub A/S
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

public class TotalMonthlyAmountsColumnNames
{
    public const string CalculationId = "calculation_id";
    public const string CalculationExecutionTimeStart = "calculation_execution_time_start";
    public const string CalculationType = "calculation_type";
    public const string CalculationResultId = "calculation_result_id";
    public const string GridArea = "grid_area";
    public const string EnergySupplierId = "energy_supplier_id";
    public const string Time = "time";
    public const string Amount = "amount";
    public const string ChargeOwnerId = "charge_owner_id";

    public static IReadOnlyCollection<string> GetAllNames()
    {
        var fieldInfos = typeof(TotalMonthlyAmountsColumnNames).GetFields(BindingFlags.Public | BindingFlags.Static);
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
            ChargeOwnerId => "string",
            Time => "timestamp",
            Amount => "decimal(18,6)",
            _ => throw new ArgumentOutOfRangeException(nameof(columnName), actualValue: columnName, "Unexpected column name."),
        };
}
