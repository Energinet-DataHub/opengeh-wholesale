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
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.SettlementReportAggregate;

namespace Energinet.DataHub.Wholesale.Infrastructure.SettlementReports;

public class SettlementReportRepository : ISettlementReportRepository
{
    private readonly DataLakeFileSystemClient _dataLakeFileSystemClient;

    private readonly List<Func<Guid, GridAreaCode, (string Directory, string Extension, string EntryPath)>>
        _fileIdentifierProviders;

    private readonly IStreamZipper _streamZipper;

    public SettlementReportRepository(
        DataLakeFileSystemClient dataLakeFileSystemClient,
        IStreamZipper streamZipper)
    {
        _dataLakeFileSystemClient = dataLakeFileSystemClient;
        _streamZipper = streamZipper;
        _fileIdentifierProviders =
            new List<Func<Guid, GridAreaCode, (string Directory, string Extension, string EntryPath)>>
            {
                GetTimeSeriesHourBasisDataForTotalGridAreaFileSpecification,
                GetTimeSeriesQuarterBasisDataForTotalGridAreaFileSpecification,
                GetMasterBasisDataFileForTotalGridAreaSpecification,
            };
    }

    public async Task CreateSettlementReportsAsync(Batch completedBatch)
    {
        var batchBasisFileStreams = await GetBatchBasisFileStreamsAsync(completedBatch).ConfigureAwait(false);

        var zipFileName = GetZipFileName(completedBatch);
        var zipStream = await GetWriteStreamAsync(zipFileName).ConfigureAwait(false);
        await using (zipStream)
            await _streamZipper.ZipAsync(batchBasisFileStreams, zipStream).ConfigureAwait(false);
    }

    public async Task<SettlementReport> GetSettlementReportAsync(Batch batch)
    {
        var zipFileName = GetZipFileName(batch);
        var dataLakeFileClient = _dataLakeFileSystemClient.GetFileClient(zipFileName);
        var stream = (await dataLakeFileClient.ReadAsync().ConfigureAwait(false)).Value.Content;
        return new SettlementReport(stream);
    }

    public async Task GetSettlementReportAsync(Batch completedBatch, GridAreaCode gridAreaCode, Stream outputStream)
    {
        var batchBasisFileStreams = await GetProcessBasisFileStreamsAsync(completedBatch.Id, gridAreaCode)
            .ConfigureAwait(false);
        await _streamZipper.ZipAsync(batchBasisFileStreams, outputStream).ConfigureAwait(false);
    }

    public static (string Directory, string Extension, string ZipEntryPath) GetTimeSeriesHourBasisDataForTotalGridAreaFileSpecification(Guid batchId, GridAreaCode gridAreaCode)
        => ($"calculation-output/batch_id={batchId}/basis_data/time_series_hour/grouping=total_ga/grid_area={gridAreaCode.Code}/",
            ".csv",
            $"{gridAreaCode.Code}/Timeseries_PT1H.csv");

    public static (string Directory, string Extension, string ZipEntryPath)
        GetTimeSeriesQuarterBasisDataForTotalGridAreaFileSpecification(Guid batchId, GridAreaCode gridAreaCode)
        => ($"calculation-output/batch_id={batchId}/basis_data/time_series_quarter/grouping=total_ga/grid_area={gridAreaCode.Code}/",
            ".csv",
            $"{gridAreaCode.Code}/Timeseries_PT15M.csv");

    public static (string Directory, string Extension, string ZipEntryPath)
        GetMasterBasisDataFileForTotalGridAreaSpecification(Guid batchId, GridAreaCode gridAreaCode)
        => ($"calculation-output/batch_id={batchId}/basis_data/master_basis_data/grouping=total_ga/grid_area={gridAreaCode.Code}/",
            ".csv",
            $"{gridAreaCode.Code}/MeteringPointMasterData.csv");

    public static (string Directory, string Extension, string ZipEntryPath)
        GetTimeSeriesHourBasisDataForEsPerGaGridAreaFileSpecification(Guid batchId, GridAreaCode gridAreaCode, string energySupplierId)
        => ($"calculation-output/batch_id={batchId}/basis_data/time_series_hour/grouping=es_ga/grid_area={gridAreaCode.Code}/energy_supplier_gln={energySupplierId}/",
            ".csv",
            $"{gridAreaCode.Code}/Timeseries_PT1H.csv");

    public static (string Directory, string Extension, string ZipEntryPath)
        GetTimeSeriesQuarterBasisDataForEsPerGaFileSpecification(Guid batchId, GridAreaCode gridAreaCode, string energySupplierId)
        => ($"calculation-output/batch_id={batchId}/basis_data/time_series_quarter/grouping=es_ga/grid_area={gridAreaCode.Code}/energy_supplier_gln={energySupplierId}/",
            ".csv",
            $"{gridAreaCode.Code}/Timeseries_PT15M.csv");

    public static (string Directory, string Extension, string ZipEntryPath)
        GetMasterBasisDataFileForForEsPerGaSpecification(Guid batchId, GridAreaCode gridAreaCode, string energySupplierId)
        => ($"calculation-output/batch_id={batchId}/basis_data/master_basis_data/grouping=es_ga/grid_area={gridAreaCode.Code}/energy_supplier_gln={energySupplierId}/",
            ".csv",
            $"{gridAreaCode.Code}/MeteringPointMasterData.csv");

    public static string GetZipFileName(Batch batch) => $"calculation-output/batch_id={batch.Id}/zip/gln=grid_area/batch_{batch.Id}_{batch.PeriodStart}_{batch.PeriodEnd}.zip";

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
