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
using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Wholesale.Application.ProcessResult;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;

namespace Energinet.DataHub.Wholesale.Infrastructure.Processes;

public class ProcessStepResultRepository : DataLakeRepositoryBase, IProcessStepResultRepository
{
    private readonly IProcessResultPointFactory _processResultPointFactory;

    public ProcessStepResultRepository(
        DataLakeFileSystemClient dataLakeFileSystemClient,
        IProcessResultPointFactory processResultPointFactory)
        : base(dataLakeFileSystemClient)
    {
        _processResultPointFactory = processResultPointFactory;
    }

    public async Task<ProcessStepResult> GetAsync(Guid batchId, GridAreaCode gridAreaCode, TimeSeriesType timeSeriesType, string gln)
    {
        var (directory, extension, _) = GetResultFileSpecification(batchId, gridAreaCode, timeSeriesType, gln);
        var dataLakeFileClient = await GetDataLakeFileClientAsync(directory, extension).ConfigureAwait(false);
        if (dataLakeFileClient == null)
        {
            throw new InvalidOperationException($"Blob for batch with id={batchId} was not found.");
        }

        var resultStream = await dataLakeFileClient.OpenReadAsync(false).ConfigureAwait(false);
        var points = await _processResultPointFactory.GetPointsFromJsonStreamAsync(resultStream).ConfigureAwait(false);

        return MapToProcessStepResultDto(points);
    }

    public static (string Directory, string Extension, string ZipEntryPath) GetResultFileSpecification(Guid batchId, GridAreaCode gridAreaCode, TimeSeriesType timeSeriesType, string gln)
        => ($"calculation-output/batch_id={batchId}/result/grid_area={gridAreaCode.Code}/gln={gln}/time_series_type={TimeSeriesTypeMapper.Map(timeSeriesType)}/", ".json", $"{gridAreaCode.Code}/Result.json");

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
