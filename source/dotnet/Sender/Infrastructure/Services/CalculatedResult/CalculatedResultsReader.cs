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

using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence.Processes;

namespace Energinet.DataHub.Wholesale.Sender.Infrastructure.Services.CalculatedResult;

public class CalculatedResultsReader : ICalculatedResultReader
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly DataLakeFileSystemClient _dataLakeFileSystemClient;

    public CalculatedResultsReader(
        IJsonSerializer jsonSerializer,
        DataLakeFileSystemClient dataLakeFileSystemClient)
    {
        _jsonSerializer = jsonSerializer;
        _dataLakeFileSystemClient = dataLakeFileSystemClient;
    }

    public async Task<BalanceFixingResultDto> ReadResultAsync(Process process)
    {
        var resultPath = await SelectResultFilePathAsync(process).ConfigureAwait(false);

        var fileClient = _dataLakeFileSystemClient.GetFileClient(resultPath);
        var stream = await fileClient.OpenReadAsync().ConfigureAwait(false);

        await using (stream.ConfigureAwait(false))
        {
            using var reader = new StreamReader(stream);

            var points = new List<PointDto>();

            while (await reader.ReadLineAsync().ConfigureAwait(false) is { } nextLine)
            {
                var point = (PointDto)_jsonSerializer.Deserialize(nextLine, typeof(PointDto));
                points.Add(point);
            }

            return new BalanceFixingResultDto(points.ToArray());
        }
    }

    private async Task<string> SelectResultFilePathAsync(Process process)
    {
        var normBatchId = process.BatchId.ToString().ToLowerInvariant();
        var folder = $"results/batch_id={normBatchId}/grid_area={process.GridAreaCode}/";

        var directory = _dataLakeFileSystemClient.GetDirectoryClient(folder);

        await foreach (var pathItem in directory.GetPathsAsync())
        {
            if (Path.GetExtension(pathItem.Name) == ".json")
            {
                return folder + pathItem.Name;
            }
        }

        throw new InvalidOperationException($"Blob for process {process.Id} was not found.");
    }
}
