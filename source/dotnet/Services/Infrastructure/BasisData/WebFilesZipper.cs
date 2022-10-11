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

using System.IO.Compression;

namespace Energinet.DataHub.Wholesale.Infrastructure.BasisData;

public class WebFilesZipper : IWebFilesZipper
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// The <paramref name="httpClient"/> should be registered as a singleton.
    /// </summary>
    public WebFilesZipper(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task ZipAsync(IEnumerable<(Uri Url, string EntryPath)> inputFiles, Stream zipFileStream)
    {
        using var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create);

        foreach (var inputFile in inputFiles)
            await AddEntryAsync(archive, inputFile).ConfigureAwait(false);
    }

    private async Task AddEntryAsync(ZipArchive archive, (Uri Url, string EntryPath) inputFile)
    {
        var inputStream = await GetStreamAsync(inputFile.Url).ConfigureAwait(false);
        var readmeEntry = archive.CreateEntry(inputFile.EntryPath);
        await inputStream.CopyToAsync(readmeEntry.Open()).ConfigureAwait(false);
    }

    private async Task<Stream> GetStreamAsync(Uri webFileUrl)
    {
        // TODO BJARKE: Log errors fetching file
        using var response = await _httpClient.GetAsync(webFileUrl).ConfigureAwait(false);
        return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
    }
}
