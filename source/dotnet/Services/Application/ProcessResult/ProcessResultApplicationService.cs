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

public class ProcessResultApplicationService : IProcessResultApplicationService
{
    private readonly IBatchFileManager _batchFileManager;

    public ProcessResultApplicationService(IBatchFileManager batchFileManager)
    {
        _batchFileManager = batchFileManager;
    }

    public async Task<ProcessStepResultDto> GetResultAsync(Guid batchId, string gridAreaCode, ProcessStepType processStepType)
    {
        var resultStream = await _batchFileManager.GetResultFileStreamAsync(batchId, new GridAreaCode(gridAreaCode))
            .ConfigureAwait(false);

        var points = await GetPointsFromJsonStreamAsync(resultStream).ConfigureAwait(false);
        var pointsDto = new List<TimeSeriesPointDto>();
        foreach (var point in points)
        {
            pointsDto.Add(new TimeSeriesPointDto(DateTimeOffset.Now, decimal.Parse(point.Quantity)));
        }

        return new ProcessStepResultDto(
            MeteringPointType.Production,
            decimal.Zero,
            decimal.One,
            decimal.One + decimal.One,
            pointsDto.ToArray());
    }

    private static async Task<List<PointTypeReal>> GetPointsFromJsonStreamAsync(Stream resultStream)
    {
        var list = new List<PointTypeReal>();

        var streamer = new StreamReader(resultStream);

        var nextline = await streamer.ReadLineAsync().ConfigureAwait(false);
        while (nextline != null)
        {
            var dto = JsonSerializer.Deserialize<PointTypeReal>(nextline, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (dto != null)
                list.Add(dto);

            nextline = await streamer.ReadLineAsync().ConfigureAwait(false);
        }

        return list;
    }
}
