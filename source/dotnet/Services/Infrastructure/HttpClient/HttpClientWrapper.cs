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

namespace Energinet.DataHub.Wholesale.Infrastructure.HttpClient;

public class HttpClientWrapper : IHttpClient
{
    private readonly System.Net.Http.HttpClient _httpClient;

    public HttpClientWrapper(System.Net.Http.HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Stream> GetStreamAsync(Uri uri)
    {
        using var response = await _httpClient.GetAsync(uri).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to access web file {uri}, HTTP status code was {response.StatusCode}");

        return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
    }
}
