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

using System.Net.Http.Json;
using Energinet.DataHub.Wholesale.Application.Batches;

namespace Energinet.DataHub.Wholesale.Client;

public class WholesaleClient : IWholesaleClient
{
    private readonly HttpClient _httpClient;

    public WholesaleClient(AuthorizedHttpClientFactory httpClientFactory, Uri wholesaleBaseUrl)
    {
        _httpClient = httpClientFactory.CreateClient(wholesaleBaseUrl);
    }

    public async Task<HttpResponseMessage> CreateBatchAsync(BatchRequestDto wholesaleBatchRequestDto)
    {
        return await _httpClient
            .PostAsJsonAsync("v1/Batch", wholesaleBatchRequestDto)
            .ConfigureAwait(false);
    }

    public async Task<(HttpResponseMessage Response, IEnumerable<BatchDto>? Batches)> GetBatchesAsync(
        BatchSearchDto batchSearchDto)
    {
        var response = await _httpClient
            .PostAsJsonAsync("v1/Batch/search", batchSearchDto)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            return new(response, null);

        var batches = await response.Content.ReadFromJsonAsync<IEnumerable<BatchDto>>().ConfigureAwait(false);
        return new(response, batches ?? new List<BatchDto>());
    }
}
