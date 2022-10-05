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

using Energinet.DataHub.Wholesale.Application.Batches;

namespace Energinet.DataHub.Wholesale.Client;

public interface IWholesaleClient
{
    Task<HttpResponseMessage> CreateBatchAsync(BatchRequestDto wholesaleBatchRequestDto);

    /// <summary>
    /// Returns batches matching the search criteria.
    /// In case of error the batches of the result is null.
    /// </summary>
    Task<(HttpResponseMessage Response, IEnumerable<BatchDto>? Batches)> GetBatchesAsync(BatchSearchDto batchSearchDto);
}
