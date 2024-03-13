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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;

namespace Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;

public interface IWholesaleResultQueries
{
    /// <summary>
    /// Get all wholesale results for a given calculation.
    /// </summary>
    IAsyncEnumerable<WholesaleResult> GetAsync(Guid calculationId);

    /// <summary>
    /// Get all wholesale results for the given query parameters, including a list of calculations with periods
    /// </summary>
    IAsyncEnumerable<WholesaleResult> GetAsync(WholesaleResultQueryParameters queryParameters);

    /// <summary>
    /// Get if any wholesale results exists for the given query parameters, including a list of calculations with periods
    /// </summary>
    Task<bool> AnyAsync(WholesaleResultQueryParameters queryParameters);
}
