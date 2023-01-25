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

public class ProcessStepResultRepository : IProcessStepResultRepository
{
    // These strings represents how write our results from spark.
    // They should only be changed with changing how we write down the results.
    private const string NonProfiledConsumption = "non_profiled_consumption";
    private const string Consumption = "consumption";
    private const string Production = "production";

    private readonly DataLakeFileSystemClient _dataLakeFileSystemClient;

    private readonly IProcessResultPointFactory _processResultPointFactory;

    public ProcessStepResultRepository(
        DataLakeFileSystemClient dataLakeFileSystemClient,
        IProcessResultPointFactory processResultPointFactory)
    {
        _dataLakeFileSystemClient = dataLakeFileSystemClient;
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
        => ($"calculation-output/batch_id={batchId}/result/grid_area={gridAreaCode.Code}/gln={gln}/time_series_type={MapTimeSeriesType(timeSeriesType)}/", ".json", $"{gridAreaCode.Code}/Result.json");

    private static string MapTimeSeriesType(TimeSeriesType timeSeriesType)
    {
        return timeSeriesType switch
        {
            TimeSeriesType.NonProfiledConsumption => NonProfiledConsumption,
            TimeSeriesType.Consumption => Consumption,
            TimeSeriesType.Production => Production,
            _ => throw new ArgumentOutOfRangeException(nameof(timeSeriesType), timeSeriesType, null),
        };
    }

    /// <summary>
    /// Search for a file by a given extension in a blob directory.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="extension"></param>
    /// <returns>The first file with matching file extension. If no directory was found, return null</returns>
    private async Task<DataLakeFileClient?> GetDataLakeFileClientAsync(string directory, string extension)
    {
        var directoryClient = _dataLakeFileSystemClient.GetDirectoryClient(directory);
        var directoryExists = await directoryClient.ExistsAsync().ConfigureAwait(false);
        if (!directoryExists.Value)
            return null;

        await foreach (var pathItem in directoryClient.GetPathsAsync())
        {
            if (Path.GetExtension(pathItem.Name) == extension)
                return _dataLakeFileSystemClient.GetFileClient(pathItem.Name);
        }

        throw new Exception($"No Data Lake file with extension '{extension}' was found in directory '{directory}'");
    }

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
