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
using Energinet.DataHub.Wholesale.Infrastructure.HttpClient;

namespace Energinet.DataHub.Wholesale.Infrastructure.BasisData;

public class WebFilesZipper : IWebFilesZipper
{
    private readonly IHttpClient _httpClient;

    public WebFilesZipper(IHttpClient httpClient)
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
        var inputStream = await _httpClient.GetStreamAsync(inputFile.Url).ConfigureAwait(false);
        await using (inputStream.ConfigureAwait(false))
        {
            var zipArchiveEntry = archive.CreateEntry(inputFile.EntryPath);
            var entryStream = zipArchiveEntry.Open();
            await using (entryStream.ConfigureAwait(false))
            {
                await inputStream.CopyToAsync(entryStream).ConfigureAwait(false);
            }
        }
    }
}
