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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;

namespace Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;

public interface IAggregatedTimeSeriesQueries
{
    /// <summary>
    /// Gets the latest aggregated time series per grid area
    /// </summary>
    /// <returns>Returns an empty list if the aggregated time series does not contain any points.</returns>
    IAsyncEnumerable<AggregatedTimeSeries> GetAsync(AggregatedTimeSeriesQueryParameters parameters);

    /// <summary>
    /// Gets the most recent aggregated time series for the last correction settlements.
    /// </summary>
    /// <returns>Returns an empty list if the aggregated time series does not contain any points.</returns>
    IAsyncEnumerable<AggregatedTimeSeries> GetLatestCorrectionForGridAreaAsync(AggregatedTimeSeriesQueryParameters parameters);
}
