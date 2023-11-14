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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.DataLake;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;

public class SettlementReportRepository : ISettlementReportRepository
{
    private readonly IDataLakeClient _dataLakeClient;

    private readonly List<Func<Guid, string, (string Directory, string Extension, string EntryPath)>>
        _fileIdentifierProviders;

    private readonly IStreamZipper _streamZipper;

    public SettlementReportRepository(
        IDataLakeClient dataLakeClient,
        IStreamZipper streamZipper)
    {
        _dataLakeClient = dataLakeClient;
        _streamZipper = streamZipper;
        _fileIdentifierProviders = new List<Func<Guid, string, (string Directory, string Extension, string EntryPath)>>
        {
            GetTimeSeriesHourBasisDataForTotalGridAreaFileSpecification,
            GetTimeSeriesQuarterBasisDataForTotalGridAreaFileSpecification,
            GetMasterBasisDataFileForTotalGridAreaSpecification,
        };
    }

    public async Task CreateSettlementReportsAsync(BatchInfo completedBatchInfo)
    {
        var batchBasisFileStreams = await GetBatchBasisFileStreamsAsync(completedBatchInfo).ConfigureAwait(false);

        var zipFileName = GetZipFileName(completedBatchInfo);
        var zipStream = await _dataLakeClient.GetWriteableFileStreamAsync(zipFileName).ConfigureAwait(false);
        await using (zipStream)
            await _streamZipper.ZipAsync(batchBasisFileStreams, zipStream).ConfigureAwait(false);
    }

    public async Task<SettlementReport> GetSettlementReportAsync(BatchInfo batchInfo)
    {
        var zipFileName = GetZipFileName(batchInfo);
        var stream = await _dataLakeClient.GetReadableFileStreamAsync(zipFileName).ConfigureAwait(false);
        return new SettlementReport(stream);
    }

    public async Task GetSettlementReportAsync(BatchInfo completedBatchInfo, string gridAreaCode, Stream outputStream)
    {
        var batchBasisFileStreams = await GetProcessBasisFileStreamsAsync(completedBatchInfo.Id, gridAreaCode)
            .ConfigureAwait(false);
        await _streamZipper.ZipAsync(batchBasisFileStreams, outputStream).ConfigureAwait(false);
    }

    public static (string Directory, string Extension, string ZipEntryPath) GetTimeSeriesHourBasisDataForTotalGridAreaFileSpecification(Guid batchId, string gridAreaCode)
        => ($"calculation-output/batch_id={batchId}/basis_data/time_series_hour/grouping=total_ga/grid_area={gridAreaCode}/",
            ".csv",
            $"{gridAreaCode}/Timeseries_PT1H.csv");

    public static (string Directory, string Extension, string ZipEntryPath) GetTimeSeriesQuarterBasisDataForTotalGridAreaFileSpecification(Guid batchId, string gridAreaCode)
        => ($"calculation-output/batch_id={batchId}/basis_data/time_series_quarter/grouping=total_ga/grid_area={gridAreaCode}/",
            ".csv",
            $"{gridAreaCode}/Timeseries_PT15M.csv");

    public static (string Directory, string Extension, string ZipEntryPath) GetMasterBasisDataFileForTotalGridAreaSpecification(Guid batchId, string gridAreaCode)
        => ($"calculation-output/batch_id={batchId}/basis_data/master_basis_data/grouping=total_ga/grid_area={gridAreaCode}/",
            ".csv",
            $"{gridAreaCode}/MeteringPointMasterData.csv");

    public static (string Directory, string Extension, string ZipEntryPath) GetTimeSeriesHourBasisDataForEsPerGaGridAreaFileSpecification(Guid batchId, string gridAreaCode, string energySupplierId)
        => ($"calculation-output/batch_id={batchId}/basis_data/time_series_hour/grouping=es_ga/grid_area={gridAreaCode}/energy_supplier_gln={energySupplierId}/",
            ".csv",
            $"{gridAreaCode}/Timeseries_PT1H.csv");

    public static (string Directory, string Extension, string ZipEntryPath) GetTimeSeriesQuarterBasisDataForEsPerGaFileSpecification(Guid batchId, string gridAreaCode, string energySupplierId)
        => ($"calculation-output/batch_id={batchId}/basis_data/time_series_quarter/grouping=es_ga/grid_area={gridAreaCode}/energy_supplier_gln={energySupplierId}/",
            ".csv",
            $"{gridAreaCode}/Timeseries_PT15M.csv");

    public static (string Directory, string Extension, string ZipEntryPath) GetMasterBasisDataFileForForEsPerGaSpecification(Guid batchId, string gridAreaCode, string energySupplierId)
        => ($"calculation-output/batch_id={batchId}/basis_data/master_basis_data/grouping=es_ga/grid_area={gridAreaCode}/energy_supplier_gln={energySupplierId}/",
            ".csv",
            $"{gridAreaCode}/MeteringPointMasterData.csv");

    public static string GetZipFileName(BatchInfo batchInfo) => $"calculation-output/batch_id={batchInfo.Id}/zip/gln=grid_area/batch_{batchInfo.Id}_{batchInfo.PeriodStart}_{batchInfo.PeriodEnd}.zip";

    private async Task<IEnumerable<(Stream FileStream, string EntryPath)>> GetBatchBasisFileStreamsAsync(BatchInfo batchInfo)
    {
        var batchBasisFiles = new List<(Stream FileStream, string EntryPath)>();

        foreach (var gridAreaCode in batchInfo.GridAreaCodes)
        {
            var gridAreaFileUrls = await GetProcessBasisFileStreamsAsync(batchInfo.Id, gridAreaCode).ConfigureAwait(false);
            batchBasisFiles.AddRange(gridAreaFileUrls);
        }

        return batchBasisFiles;
    }

    private async Task<List<(Stream FileStream, string EntryPath)>> GetProcessBasisFileStreamsAsync(Guid batchId, string gridAreaCode)
    {
        var processDataFilesUrls = new List<(Stream FileStream, string EntryPath)>();

        foreach (var fileIdentifierProvider in _fileIdentifierProviders)
        {
            var (directory, extension, entryPath) = fileIdentifierProvider(batchId, gridAreaCode);
            var filepath = await _dataLakeClient.FindFileAsync(directory, extension).ConfigureAwait(false);
            var stream = await _dataLakeClient.GetReadableFileStreamAsync(filepath).ConfigureAwait(false);
            processDataFilesUrls.Add((stream, entryPath));
        }

        return processDataFilesUrls;
    }
}
