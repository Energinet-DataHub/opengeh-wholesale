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

using System.Globalization;
using Energinet.DataHub.Wholesale.Domain.ActorAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Integration.DataLake;
using TimeSeriesType = Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Infrastructure.Processes;

public class ProcessStepResultRepository : IProcessStepResultRepository
{
    private readonly IDataLakeClient _dataLakeClient;
    private readonly IJsonNewlineSerializer _jsonNewlineSerializer;

    public ProcessStepResultRepository(
        IDataLakeClient dataLakeClient,
        IJsonNewlineSerializer jsonNewlineSerializer)
    {
        _dataLakeClient = dataLakeClient;
        _jsonNewlineSerializer = jsonNewlineSerializer;
    }

    public async Task<ProcessStepResult> GetAsync(
        Guid batchId,
        GridAreaCode gridAreaCode,
        TimeSeriesType timeSeriesType,
        string? gln,
        MarketRole marketRole)
    {
        var (directory, extension) = marketRole != MarketRole.GridAccessProvider ? GetResultFilePerGridAreaSpecification(batchId, gridAreaCode, timeSeriesType, gln, marketRole)
            : GetResultFileTotalGridAreaSpecification(batchId, gridAreaCode, timeSeriesType, marketRole);

        var dataLakeFileClient = await _dataLakeClient.GetDataLakeFileClientAsync(directory, extension).ConfigureAwait(false);
        if (dataLakeFileClient == null)
        {
            throw new InvalidOperationException($"Blob for batch with id={batchId} was not found.");
        }

        var resultStream = await dataLakeFileClient.OpenReadAsync(false).ConfigureAwait(false);
        var points = await _jsonNewlineSerializer.DeserializeAsync<ProcessResultPoint>(resultStream).ConfigureAwait(false);

        return MapToProcessStepResultDto(points);
    }

    // cant find es_brp_ga
    public static (string Directory, string Extension) GetResultFilePerGridAreaSpecification(Guid batchId, GridAreaCode gridAreaCode, TimeSeriesType timeSeriesType, string gln, MarketRole marketRole)
        => ($"calculation-output/batch_id={batchId}/result/{MarketRoleMapper.Map(marketRole)}/time_series_type={TimeSeriesTypeMapper.Map(timeSeriesType)}/grid_area={gridAreaCode.Code}/gln={gln}/", ".json");

    public static (string Directory, string Extension) GetResultFileTotalGridAreaSpecification(Guid batchId, GridAreaCode gridAreaCode, TimeSeriesType timeSeriesType, MarketRole marketRole)
        => ($"calculation-output/batch_id={batchId}/result/{MarketRoleMapper.Map(marketRole)}/time_series_type={TimeSeriesTypeMapper.Map(timeSeriesType)}/grid_area={gridAreaCode.Code}/", ".json");

    private static ProcessStepResult MapToProcessStepResultDto(List<ProcessResultPoint> points)
    {
        var pointsDto = points.Select(
                point => new TimeSeriesPoint(
                    DateTimeOffset.Parse(point.quarter_time),
                    decimal.Parse(point.quantity, CultureInfo.InvariantCulture),
                    point.quality))
            .ToList();

        return new ProcessStepResult(pointsDto.ToArray());
    }
}
