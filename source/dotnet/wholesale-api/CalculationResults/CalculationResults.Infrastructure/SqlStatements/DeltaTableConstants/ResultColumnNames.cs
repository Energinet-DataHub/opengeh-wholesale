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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;

public static class ResultColumnNames
{
    public const string BatchId = "batch_id";
    public const string BatchExecutionTimeStart = "batch_execution_time_start";
    public const string BatchProcessType = "batch_process_type";
    public const string TimeSeriesType = "time_series_type";
    public const string GridArea = "grid_area";
    public const string FromGridArea = "out_grid_area";
    public const string BalanceResponsibleId = "balance_responsible_id";
    public const string EnergySupplierId = "energy_supplier_id";
    public const string Time = "time";
    public const string Quantity = "quantity";
    public const string QuantityQuality = "quantity_quality";
    public const string AggregationLevel = "aggregation_level";
}
