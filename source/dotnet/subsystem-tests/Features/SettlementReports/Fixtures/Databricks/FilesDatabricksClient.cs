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

using System.Net;
using System.Net.Http.Headers;
using Microsoft.Azure.Databricks.Client;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.SettlementReports.Fixtures.Databricks;

/// <summary>
/// Inspired by the implementation of the <see cref="DatabricksClient"/>:
/// https://github.com/Azure/azure-databricks-client/blob/f7ffd8a2b4835351c000bf8a46797fd54addc7e5/csharp/Microsoft.Azure.Databricks.Client/DatabricksClient.cs
/// </summary>
public sealed class FilesDatabricksClient : IDisposable
{
    public FilesDatabricksClient(string baseUrl, string token)
    {
        Files = new FilesApiClient(CreateHttpClient(baseUrl, token));
    }

    public IFilesApi Files { get; }

    public void Dispose()
    {
        Files.Dispose();
        GC.SuppressFinalize(this);
    }

    private static HttpClient CreateHttpClient(string baseUrl, string bearerToken, long timeoutSeconds = 30)
    {
        var apiUrl = new Uri(new Uri(baseUrl), "api/");

        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        };

        var httpClient = new HttpClient(handler, false)
        {
            BaseAddress = apiUrl,
            Timeout = TimeSpan.FromSeconds(timeoutSeconds),
        };

        SetDefaultHttpHeaders(httpClient);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return httpClient;
    }

    private static void SetDefaultHttpHeaders(HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
    }
}
