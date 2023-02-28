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

namespace Energinet.DataHub.Wholesale.Client;

public class WholesaleClientV3 : IWholesaleClientV3
{
    private readonly HttpClient _httpClient;

    public WholesaleClientV3(AuthorizedHttpClientFactory httpClientFactory, Uri wholesaleBaseUrl)
    {
        _httpClient = httpClientFactory.CreateClient(wholesaleBaseUrl);
    }

    public async Task<Stream> GetSettlementReportAsStreamAsync(Guid batchId, string gridAreaCode)
    {
        var response = await _httpClient
            .GetAsync($"v3/SettlementReport?batchId={batchId}&gridAreaCode={gridAreaCode}")
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Wholesale backend returned HTTP status code {(int)response.StatusCode}");

        return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
    }
}
