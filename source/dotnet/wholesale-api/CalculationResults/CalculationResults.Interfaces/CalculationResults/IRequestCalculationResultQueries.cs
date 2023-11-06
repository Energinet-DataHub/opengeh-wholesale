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
using Energinet.DataHub.Wholesale.Common.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;

public interface IRequestCalculationResultQueries
{
    /// <summary>
    /// Gets the latest result for a given request.
    /// </summary>
    /// <returns>Returns null if no result was found</returns>
    IAsyncEnumerable<EnergyResult> GetAsync(EnergyResultQuery query);

    /// <summary>
    /// Get the latest correction version for the given request. Defaults to FirstCorrection if there is no corrections available.
    /// </summary>
    /// <returns>Returns the latest correction that exists. If none exists it defaults to FirstCorrection</returns>
    Task<ProcessType> GetLatestCorrectionAsync(IEnergyResultFilter query);
}
