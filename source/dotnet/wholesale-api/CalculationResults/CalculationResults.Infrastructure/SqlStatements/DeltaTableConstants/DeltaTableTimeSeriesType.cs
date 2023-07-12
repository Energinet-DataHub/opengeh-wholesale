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

public static class DeltaTableTimeSeriesType
{
    // These strings represents how write our results from spark to the delta table.
    // They should only be changed with changing how we write to the delta table.
    public const string NonProfiledConsumption = "non_profiled_consumption";
    public const string FlexConsumption = "flex_consumption";
    public const string Production = "production";
    public const string NetExchangePerGridArea = "net_exchange_per_ga";
    public const string NetExchangePerNeighboringGridArea = "net_exchange_per_neighboring_ga";
    public const string GridLoss = "grid_loss";
    public const string NegativeGridLoss = "negative_grid_loss";
    public const string PositiveGridLoss = "positive_grid_loss";
    public const string TotalConsumption = "total_consumption";
    public const string TempFlexConsumption = "temp_flex_consumption";
    public const string TempProduction = "temp_production";
}
