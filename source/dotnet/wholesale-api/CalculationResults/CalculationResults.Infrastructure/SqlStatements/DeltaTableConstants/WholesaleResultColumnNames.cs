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

public class WholesaleResultColumnNames : ResultColumnNames
{
    public const string QuantityUnit = "quantity_unit";
    public const string QuantityQualities = "quantity_qualities";
    public const string Resolution = "resolution";
    public const string MeteringPointType = "metering_point_type";
    public const string SettlementMethod = "settlement_method";
    public const string Price = "price";
    public const string Amount = "amount";
    public const string IsTax = "is_tax";
    public const string ChargeCode = "charge_id";
    public const string ChargeType = "charge_type";
    public const string ChargeOwnerId = "charge_owner_id";

    // public static IEnumerable<string> GetAllNames()
    // {
    //     var fieldInfos = typeof(EnergyResultColumnNames).GetFields(BindingFlags.Public | BindingFlags.Static);
    //     return fieldInfos.Select(x => x.GetValue(null)).Cast<string>().ToList();
    // }
    //
    // public static string GetType(string columnName) =>
    //      columnName switch
    //      {
    //          GridArea => "string",
    //          BalanceResponsibleId => "string",
    //          CalculationResultId => "string",
    //          EnergySupplierId => "string",
    //          Time => "timestamp",
    //          QuantityQuality => "string",
    //          AggregationLevel => "string",
    //          TimeSeriesType => "string",
    //          BatchId => "string",
    //          BatchProcessType => "string",
    //          BatchExecutionTimeStart => "timestamp",
    //          FromGridArea => "string",
    //          Quantity => "decimal(18,3)",
    //          _ => throw new ArgumentException($"Unexpected column name '{columnName}'."),
    //      };
}
