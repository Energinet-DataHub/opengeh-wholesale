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
using Energinet.DataHub.Wholesale.Contracts;

namespace Energinet.DataHub.Wholesale.Client;

public class WholesaleClient : IWholesaleClient
{
    private readonly HttpClient _httpClient;

    public WholesaleClient(AuthorizedHttpClientFactory httpClientFactory, Uri wholesaleBaseUrl)
    {
        _httpClient = httpClientFactory.CreateClient(wholesaleBaseUrl);
    }

    public async Task<Guid> CreateBatchAsync(BatchRequestDto wholesaleBatchRequestDto)
    {
        var response = await _httpClient
            .PostAsJsonAsync("v2/Batch", wholesaleBatchRequestDto)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Wholesale backend returned HTTP status code {(int)response.StatusCode}");

        return await response.Content.ReadFromJsonAsync<Guid>().ConfigureAwait(false);
    }

    public async Task<IEnumerable<BatchDtoV2>> GetBatchesAsync(
        BatchSearchDto batchSearchDto)
    {
        var response = await _httpClient
            .PostAsJsonAsync("v2/Batch/search", batchSearchDto)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Wholesale backend returned HTTP status code {(int)response.StatusCode}");

        var batches = await response.Content.ReadFromJsonAsync<IEnumerable<BatchDtoV2>>().ConfigureAwait(false);
        return batches ?? new List<BatchDtoV2>();
    }

    public async Task<Stream> GetZippedBasisDataStreamAsync(
        Guid batchId)
    {
        var response = await _httpClient
            .PostAsJsonAsync("v2/Batch/ZippedBasisDataStream", batchId)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Wholesale backend returned HTTP status code {(int)response.StatusCode}");
        return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
    }

    public async Task<BatchDtoV2?> GetBatchAsync(Guid batchId)
    {
        var response = await _httpClient.GetAsync($"v2/Batch?batchId={batchId}").ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Wholesale backend returned HTTP status code {(int)response.StatusCode}");

        var batch = await response.Content.ReadFromJsonAsync<BatchDtoV2>().ConfigureAwait(false);
        return batch;
    }

    public async Task<BatchActorDto[]?> GetActorsAsync(BatchActorsRequestDto batchActorsRequestDto)
    {
        var response = await _httpClient
            .PostAsJsonAsync("v2/Batch/Actors", batchActorsRequestDto)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Wholesale backend returned HTTP status code {(int)response.StatusCode}");

        var batch = await response.Content.ReadFromJsonAsync<BatchActorDto[]>().ConfigureAwait(false);
        return batch;
    }

    public async Task<ProcessStepResultDto?> GetProcessStepResultAsync(ProcessStepResultRequestDto processStepResultRequestDto)
    {
        var response = await _httpClient
            .PostAsJsonAsync($"v2/ProcessStepResult", processStepResultRequestDto)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Wholesale backend returned HTTP status code {(int)response.StatusCode}");

        var batch = await response.Content.ReadFromJsonAsync<ProcessStepResultDto>().ConfigureAwait(false);
        return batch;
    }

    public async Task<ProcessStepResultDto?> GetProcessResultAsync(ProcessResultRequestDto processResultRequestDto)
    {
        var response = await _httpClient.PostAsJsonAsync($"v2/ProcessResult", processResultRequestDto).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Wholesale backend returned HTTP status code {(int)response.StatusCode}");

        var batch = await response.Content.ReadFromJsonAsync<ProcessStepResultDto>().ConfigureAwait(false);
        return batch;
    }
}
