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
using Energinet.DataHub.Wholesale.Application.Infrastructure;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Infrastructure.BasisData;

public class BatchFileManager : IBatchFileManager
{
    private readonly DataLakeFileSystemClient _dataLakeFileSystemClient;
    private readonly List<Func<Guid, GridAreaCode, (string Directory, string Extension, string EntryPath)>> _fileIdentifierProviders;

    private readonly IWebFilesZipper _webFilesZipper;
    private readonly ILogger _logger;

    public BatchFileManager(DataLakeFileSystemClient dataLakeFileSystemClient, IWebFilesZipper webFilesZipper, ILogger<BatchFileManager> logger)
    {
        _dataLakeFileSystemClient = dataLakeFileSystemClient;
        _webFilesZipper = webFilesZipper;
        _logger = logger;
        _fileIdentifierProviders = new List<Func<Guid, GridAreaCode, (string Directory, string Extension, string EntryPath)>>
        {
            GetResultDirectory,
            GetTimeSeriesHourBasisDataDirectory,
            GetTimeSeriesQuarterBasisDataDirectory,
            GetMasterBasisDataDirectory,
        };
    }

    public async Task CreateBasisDataZipAsync(Batch completedBatch)
    {
        var batchBasisFileUrls = await GetBatchBasisFileUrlsAsync(completedBatch).ConfigureAwait(false);

        var zipFileName = GetZipFileName(completedBatch);
        var zipStream = await GetWriteStreamAsync(zipFileName).ConfigureAwait(false);
        await using (zipStream)
            await _webFilesZipper.ZipAsync(batchBasisFileUrls, zipStream).ConfigureAwait(false);
    }

    public async Task<Stream> GetResultFileStreamAsync(Guid batchId, GridAreaCode gridAreaCode)
    {
        var (directory, extension, _) = GetResultDirectory(batchId, gridAreaCode);
        var dataLakeFileClient = await TryGetDataLakeFileClientAsync(directory, extension).ConfigureAwait(false);
        if (dataLakeFileClient == null)
        {
            throw new InvalidOperationException($"Blob for batch with id={batchId} was not found.");
        }

        return await dataLakeFileClient.OpenReadAsync(false).ConfigureAwait(false);
    }

    // TODO: Test that the directory paths match the directory used by Databricks (see calculator.py).
    public static (string Directory, string Extension, string ZipEntryPath) GetResultDirectory(Guid batchId, GridAreaCode gridAreaCode)
        => ($"results/batch_id={batchId}/grid_area={gridAreaCode.Code}/", ".json", $"{gridAreaCode.Code}/Result.json");

    public static (string Directory, string Extension, string ZipEntryPath) GetTimeSeriesHourBasisDataDirectory(Guid batchId, GridAreaCode gridAreaCode)
        => ($"results/basis-data/batch_id={batchId}/time-series-hour/grid_area={gridAreaCode.Code}/",
            ".csv",
            $"{gridAreaCode.Code}/Timeseries_PT1H.csv");

    public static (string Directory, string Extension, string ZipEntryPath) GetTimeSeriesQuarterBasisDataDirectory(Guid batchId, GridAreaCode gridAreaCode)
        => ($"results/basis-data/batch_id={batchId}/time-series-quarter/grid_area={gridAreaCode.Code}/",
            ".csv",
            $"{gridAreaCode.Code}/Timeseries_PT15M.csv");

    public static (string Directory, string Extension, string ZipEntryPath) GetMasterBasisDataDirectory(Guid batchId, GridAreaCode gridAreaCode)
        => ($"results/master-basis-data/batch_id={batchId}/grid_area={gridAreaCode.Code}/",
            ".csv",
            $"{gridAreaCode.Code}/MeteringPointMasterData.csv");

    public static string GetZipFileName(Batch batch) => $"results/zip/batch_{batch.Id}_{batch.PeriodStart}_{batch.PeriodEnd}.zip";

    private async Task<IEnumerable<(Uri Url, string EntryPath)>> GetBatchBasisFileUrlsAsync(Batch batch)
    {
        var basisDataFileUrls = new List<(Uri Url, string EntryPath)>();

        foreach (var gridAreaCode in batch.GridAreaCodes)
        {
            var gridAreaFileUrls = await GetProcessBasisFileUrlsAsync(batch.Id, gridAreaCode).ConfigureAwait(false);
            basisDataFileUrls.AddRange(gridAreaFileUrls);
        }

        return basisDataFileUrls;
    }

    private async Task<List<(Uri Url, string EntryPath)>> GetProcessBasisFileUrlsAsync(Guid batchId, GridAreaCode gridAreaCode)
    {
        var processDataFilesUrls = new List<(Uri Url, string EntryPath)>();

        foreach (var fileIdentifierProvider in _fileIdentifierProviders)
        {
            var (directory, extension, entryPath) = fileIdentifierProvider(batchId, gridAreaCode);
            var processDataFile = await TryGetDataLakeFileClientAsync(directory, extension).ConfigureAwait(false);

            if (processDataFile == null) continue;

            processDataFilesUrls.Add((processDataFile.Uri, entryPath));
        }

        return processDataFilesUrls;
    }

    /// <summary>
    /// Search for a file by a given extension in a blob directory.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="extension"></param>
    /// <returns>The first file with matching file extension.</returns>
    private async Task<DataLakeFileClient?> TryGetDataLakeFileClientAsync(string directory, string extension)
    {
        var directoryClient = _dataLakeFileSystemClient.GetDirectoryClient(directory);
        var directoryExists = await directoryClient.ExistsAsync().ConfigureAwait(false);
        if (!directoryExists.Value)
        {
            _logger.LogError("Calculation storage directory '{Directory}' does not exist", directory);
            return null;
        }

        await foreach (var pathItem in directoryClient.GetPathsAsync())
        {
            if (Path.GetExtension(pathItem.Name) == extension)
            {
                return _dataLakeFileSystemClient.GetFileClient(pathItem.Name);
            }
        }

        _logger.LogError("Blob matching '{Directory}*{Extension}' not found", directory, extension);
        return null;
    }

    private Task<Stream> GetWriteStreamAsync(string fileName)
    {
        var dataLakeFileClient = _dataLakeFileSystemClient.GetFileClient(fileName);
        return dataLakeFileClient.OpenWriteAsync(false);
    }
}
