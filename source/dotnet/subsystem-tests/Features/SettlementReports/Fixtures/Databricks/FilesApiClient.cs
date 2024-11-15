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

using Microsoft.Azure.Databricks.Client;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.SettlementReports.Fixtures.Databricks;

/// <summary>
/// Extend the Databricks Client with operations from the Files REST API: https://docs.databricks.com/api/azure/workspace/files
/// Inspired by the design of the Databricks Client library: https://github.com/Azure/azure-databricks-client
/// </summary>
public sealed class FilesApiClient : ApiClient, IFilesApi
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilesApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    public FilesApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    /// <inheritdoc cref="IFilesApi"/>
    public async Task<FileInfo> GetFileInfoAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiVersion}/fs/files{filePath}";

        var request = new HttpRequestMessage(HttpMethod.Head, url);
        using var response = await HttpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw CreateApiException(response);
        }

        return new FileInfo(
            ContentType: response.Content.Headers.ContentType?.ToString() ?? string.Empty,
            ContentLength: response.Content.Headers.ContentLength ?? -1,
            LastModified: response.Content.Headers.LastModified);
    }

    /// <summary>
    /// Gets a stream to the file.
    /// </summary>
    /// <param name="filePath">The absolute path of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A stream to the file.</returns>
    public async Task<Stream> GetFileStreamAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiVersion}/fs/files{filePath}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw CreateApiException(response);
        }

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
}
