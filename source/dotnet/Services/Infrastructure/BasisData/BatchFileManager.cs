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

namespace Energinet.DataHub.Wholesale.Infrastructure.BasisData;

public class BatchFileManager : IBatchFileManager
{
    private readonly DataLakeFileSystemClient _dataLakeFileSystemClient;
    private readonly List<Func<Guid, GridAreaCode, (string Directory, string Extension, string EntryPath)>> _fileIdentifierProviders;

    private readonly IStreamZipper _streamZipper;

    public BatchFileManager(DataLakeFileSystemClient dataLakeFileSystemClient, IStreamZipper streamZipper)
    {
        _dataLakeFileSystemClient = dataLakeFileSystemClient;
        _streamZipper = streamZipper;
        _fileIdentifierProviders = new List<Func<Guid, GridAreaCode, (string Directory, string Extension, string EntryPath)>>
        {
            GetResultFileSpecification,
            GetTimeSeriesHourBasisDataFileSpecification,
            GetTimeSeriesQuarterBasisDataFileSpecification,
            GetMasterBasisDataFileSpecification,
        };
    }

    public async Task CreateBasisDataZipAsync(Batch completedBatch)
    {
        var batchBasisFileStreams = await GetBatchBasisFileStreamsAsync(completedBatch).ConfigureAwait(false);

        var zipFileName = GetZipFileName(completedBatch);
        var zipStream = await GetWriteStreamAsync(zipFileName).ConfigureAwait(false);
        await using (zipStream)
            await _streamZipper.ZipAsync(batchBasisFileStreams, zipStream).ConfigureAwait(false);
    }

    public async Task<Stream> GetResultFileStreamAsync(Guid batchId, GridAreaCode gridAreaCode)
    {
        var (directory, extension, _) = GetResultFileSpecification(batchId, gridAreaCode);
        var dataLakeFileClient = await GetDataLakeFileClientAsync(directory, extension).ConfigureAwait(false);
        if (dataLakeFileClient == null)
        {
            throw new InvalidOperationException($"Blob for batch with id={batchId} was not found.");
        }

        return await dataLakeFileClient.OpenReadAsync(false).ConfigureAwait(false);
    }

    public async Task<Stream> GetZippedBasisDataStreamAsync(Batch batch)
    {
        var zipFileName = GetZipFileName(batch);
        var dataLakeFileClient = _dataLakeFileSystemClient.GetFileClient(zipFileName);
        var stream = (await dataLakeFileClient.ReadAsync().ConfigureAwait(false)).Value.Content;
        return stream;
    }

    public static (string Directory, string Extension, string ZipEntryPath) GetResultFileSpecification(Guid batchId, GridAreaCode gridAreaCode)
        => ($"results/batch_id={batchId}/grid_area={gridAreaCode.Code}/", ".json", $"{gridAreaCode.Code}/Result.json");

    public static (string Directory, string Extension, string ZipEntryPath) GetTimeSeriesHourBasisDataFileSpecification(Guid batchId, GridAreaCode gridAreaCode)
        => ($"results/basis-data/batch_id={batchId}/time-series-hour/grid_area={gridAreaCode.Code}/",
            ".csv",
            $"{gridAreaCode.Code}/Timeseries_PT1H.csv");

    public static (string Directory, string Extension, string ZipEntryPath) GetTimeSeriesQuarterBasisDataFileSpecification(Guid batchId, GridAreaCode gridAreaCode)
        => ($"results/basis-data/batch_id={batchId}/time-series-quarter/grid_area={gridAreaCode.Code}/",
            ".csv",
            $"{gridAreaCode.Code}/Timeseries_PT15M.csv");

    public static (string Directory, string Extension, string ZipEntryPath) GetMasterBasisDataFileSpecification(Guid batchId, GridAreaCode gridAreaCode)
        => ($"results/master-basis-data/batch_id={batchId}/grid_area={gridAreaCode.Code}/",
            ".csv",
            $"{gridAreaCode.Code}/MeteringPointMasterData.csv");

    public static string GetZipFileName(Batch batch) => $"results/zip/batch_{batch.Id}_{batch.PeriodStart}_{batch.PeriodEnd}.zip";

    private async Task<IEnumerable<(Stream FileStream, string EntryPath)>> GetBatchBasisFileStreamsAsync(Batch batch)
    {
        var batchBasisFiles = new List<(Stream FileStream, string EntryPath)>();

        foreach (var gridAreaCode in batch.GridAreaCodes)
        {
            var gridAreaFileUrls = await GetProcessBasisFileStreamsAsync(batch.Id, gridAreaCode).ConfigureAwait(false);
            batchBasisFiles.AddRange(gridAreaFileUrls);
        }

        return batchBasisFiles;
    }

    private async Task<List<(Stream FileStream, string EntryPath)>> GetProcessBasisFileStreamsAsync(Guid batchId, GridAreaCode gridAreaCode)
    {
        var processDataFilesUrls = new List<(Stream FileStream, string EntryPath)>();

        foreach (var fileIdentifierProvider in _fileIdentifierProviders)
        {
            var (directory, extension, entryPath) = fileIdentifierProvider(batchId, gridAreaCode);
            var processDataFile = await GetDataLakeFileClientAsync(directory, extension).ConfigureAwait(false);
            if (processDataFile == null) continue;
            var response = await processDataFile.ReadAsync().ConfigureAwait(false);
            processDataFilesUrls.Add((response.Value.Content, entryPath));
        }

        return processDataFilesUrls;
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

    private Task<Stream> GetWriteStreamAsync(string fileName)
    {
        var dataLakeFileClient = _dataLakeFileSystemClient.GetFileClient(fileName);
        return dataLakeFileClient.OpenWriteAsync(false);
    }
}
