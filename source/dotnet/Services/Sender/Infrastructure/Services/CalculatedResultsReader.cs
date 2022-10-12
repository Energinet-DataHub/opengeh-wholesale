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

using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Application.Infrastructure;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence.Processes;

namespace Energinet.DataHub.Wholesale.Sender.Infrastructure.Services;

public class CalculatedResultsReader : ICalculatedResultReader
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IBatchFileManager _batchFileManager;

    public CalculatedResultsReader(
        IJsonSerializer jsonSerializer,
        IBatchFileManager batchFileManager)
    {
        _jsonSerializer = jsonSerializer;
        _batchFileManager = batchFileManager ?? throw new ArgumentNullException(nameof(batchFileManager));
    }

    public async Task<BalanceFixingResultDto> ReadResultAsync(Process process)
    {
        var resultFileStream = await _batchFileManager.GetResultFileStreamAsync(process.BatchId, new GridAreaCode(process.GridAreaCode)).ConfigureAwait(false);
        await using (resultFileStream.ConfigureAwait(false))
        {
            using var reader = new StreamReader(resultFileStream);

            var points = new List<PointDto>();

            while (await reader.ReadLineAsync().ConfigureAwait(false) is { } nextLine)
            {
                var point = _jsonSerializer.Deserialize<PointDto>(nextLine);
                points.Add(point);
            }

            return new BalanceFixingResultDto(points.ToArray());
        }
    }
}
