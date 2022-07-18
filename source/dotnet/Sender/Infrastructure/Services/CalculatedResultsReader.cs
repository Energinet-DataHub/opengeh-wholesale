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
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence.Processes;

namespace Energinet.DataHub.Wholesale.Sender.Infrastructure.Services;

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
        var resultsFile = await GetResultsFileAsync(process).ConfigureAwait(false);
        var stream = await resultsFile.OpenReadAsync().ConfigureAwait(false);

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

    private async Task<DataLakeFileClient> GetResultsFileAsync(Process process)
    {
        var directory = $"results/batch_id={process.BatchId}/grid_area={process.GridAreaCode}/";
        var directoryClient = _dataLakeFileSystemClient.GetDirectoryClient(directory);

        await foreach (var pathItem in directoryClient.GetPathsAsync())
        {
            if (Path.GetExtension(pathItem.Name) == ".json")
                return directoryClient.GetFileClient(pathItem.Name);
        }

        throw new InvalidOperationException($"Blob for process {process.Id} was not found.");
    }
}
