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

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence.Processes;

namespace Energinet.DataHub.Wholesale.Sender.Infrastructure.Services.CalculatedResult;

public class CalculatedResultsReader : ICalculatedResultReader
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly BlobContainerClient _blobContainerClient;

    public CalculatedResultsReader(
        IJsonSerializer jsonSerializer,
        BlobContainerClient blobContainerClient)
    {
        _jsonSerializer = jsonSerializer;
        _blobContainerClient = blobContainerClient;
    }

    public async Task<BalanceFixingResultDto> ReadResultAsync(Process process)
    {
        var resultBlob = await SelectResultBlobAsync(process).ConfigureAwait(false);

        var blobClient = _blobContainerClient.GetBlobClient(resultBlob.Name);
        var blobStream = await blobClient.DownloadStreamingAsync().ConfigureAwait(false);

        using var stream = blobStream.Value;
        var reader = new StreamReader(stream.Content);

        var points = new List<PointDto>();

        while (await reader.ReadLineAsync().ConfigureAwait(false) is { } nextLine)
        {
            var point = (PointDto)_jsonSerializer.Deserialize(nextLine, typeof(PointDto));
            points.Add(point);
        }

        return new BalanceFixingResultDto(points.ToArray());
    }

    private async Task<BlobItem> SelectResultBlobAsync(Process process)
    {
        var normBatchId = process.BatchId.ToString().ToLowerInvariant();
        var folder = $"results/batch_id={normBatchId}/grid_area={process.GridAreaCode}/";

        var blobs = _blobContainerClient
            .GetBlobsAsync(prefix: folder)
            .ConfigureAwait(false);

        await foreach (var blob in blobs)
        {
            if (Path.GetExtension(blob.Name) == ".json")
            {
                return blob;
            }
        }

        throw new InvalidOperationException($"Blob for process {process.Id} was not found.");
    }
}
