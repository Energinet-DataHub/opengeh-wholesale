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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Processes;

public static class TimeSeriesTypeMapper
{
    // These strings represents how write our results from spark.
    // They should only be changed with changing how we write down the results.
    private const string NonProfiledConsumption = "non_profiled_consumption";
    private const string Consumption = "consumption";
    private const string Production = "production";
    private const string NetExchangePerGridArea = "net_exchange_per_ga";
    private const string NetExchangePerNeighboringGridArea = "net_exchange_per_neighboring_ga";
    private const string GridLoss = "grid_loss";
    private const string NegativeGridLoss = "negative_grid_loss";
    private const string PositiveGridLoss = "positive_grid_loss";

    public static string Map(TimeSeriesType timeSeriesType)
    {
        return timeSeriesType switch
        {
            TimeSeriesType.NonProfiledConsumption => NonProfiledConsumption,
            TimeSeriesType.FlexConsumption => Consumption,
            TimeSeriesType.Production => Production,
            TimeSeriesType.NetExchangePerGa => NetExchangePerGridArea,
            TimeSeriesType.NetExchangePerNeighboringGa => NetExchangePerNeighboringGridArea,
            TimeSeriesType.GridLoss => GridLoss,
            TimeSeriesType.NegativeGridLoss => NegativeGridLoss,
            TimeSeriesType.PositiveGridLoss => PositiveGridLoss,
            _ => throw new ArgumentOutOfRangeException(nameof(timeSeriesType), timeSeriesType, null),
        };
    }
}
