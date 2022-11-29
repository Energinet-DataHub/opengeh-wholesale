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

using System.Text.Json;
using Energinet.DataHub.Wholesale.Application.Infrastructure;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;

namespace Energinet.DataHub.Wholesale.Application.ProcessResult;

/// <summary>
/// This class provides the ability to retrieve a calculated result for a given step for a batch.
/// </summary>
public class ProcessStepResultApplicationService : IProcessStepResultApplicationService
{
    private readonly IBatchFileManager _batchFileManager;

    public ProcessStepResultApplicationService(IBatchFileManager batchFileManager)
    {
        _batchFileManager = batchFileManager;
    }

    public async Task<ProcessStepResultDto> GetResultAsync(ProcessStepResultRequestDto processStepResultRequestDto)
    {
        var resultStream = await _batchFileManager.GetResultFileStreamAsync(
                processStepResultRequestDto.BatchId,
                new GridAreaCode(processStepResultRequestDto.GridAreaCode))
            .ConfigureAwait(false);

        var points = await GetPointsFromJsonStreamAsync(resultStream).ConfigureAwait(false);

        var pointsDto = points.Select(
                point => new TimeSeriesPointDto(
                    DateTimeOffset.Parse(point.quarter_time),
                    decimal.Parse(point.quantity)))
            .ToList();

        return new ProcessStepResultDto(
            MeteringPointType.Production,
            pointsDto.Sum(x => x.Quantity),
            pointsDto.Min(x => x.Quantity),
            pointsDto.Max(x => x.Quantity),
            pointsDto.ToArray());
    }

    private static async Task<List<ProcessResultPoint>> GetPointsFromJsonStreamAsync(Stream resultStream)
    {
        var list = new List<ProcessResultPoint>();

        var streamer = new StreamReader(resultStream);

        var nextline = await streamer.ReadLineAsync().ConfigureAwait(false);
        while (nextline != null)
        {
            var dto = JsonSerializer.Deserialize<ProcessResultPoint>(
                nextline,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (dto != null)
                list.Add(dto);

            nextline = await streamer.ReadLineAsync().ConfigureAwait(false);
        }

        return list;
    }
}
