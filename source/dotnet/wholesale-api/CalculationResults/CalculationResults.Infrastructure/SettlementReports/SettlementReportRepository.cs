﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Wholesale.CalculationResults.Application;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.DataLake;

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

    public async Task CreateSettlementReportsAsync(CalculationInfo completedCalculationInfo)
    {
        var calculationBasisFileStreams = await GetCalculationBasisFileStreamsAsync(completedCalculationInfo).ConfigureAwait(false);

        var zipFileName = GetZipFileName(completedCalculationInfo);
        var zipStream = await _dataLakeClient.GetWriteableFileStreamAsync(zipFileName).ConfigureAwait(false);
        await using (zipStream)
            await _streamZipper.ZipAsync(calculationBasisFileStreams, zipStream).ConfigureAwait(false);
    }

    public async Task<SettlementReport> GetSettlementReportAsync(CalculationInfo calculationInfo)
    {
        var zipFileName = GetZipFileName(calculationInfo);
        var stream = await _dataLakeClient.GetReadableFileStreamAsync(zipFileName).ConfigureAwait(false);
        return new SettlementReport(stream);
    }

    public async Task GetSettlementReportAsync(CalculationInfo completedCalculationInfo, string gridAreaCode, Stream outputStream)
    {
        var calculationBasisFileStreams = await GetProcessBasisFileStreamsAsync(completedCalculationInfo.Id, gridAreaCode)
            .ConfigureAwait(false);
        await _streamZipper.ZipAsync(calculationBasisFileStreams, outputStream).ConfigureAwait(false);
    }

    public static (string Directory, string Extension, string ZipEntryPath) GetTimeSeriesHourBasisDataForTotalGridAreaFileSpecification(Guid calculationId, string gridAreaCode)
        => ($"calculation-output/calculation_id={calculationId}/basis_data/time_series_hour/grouping=total_ga/grid_area={gridAreaCode}/",
            ".csv",
            $"{gridAreaCode}/Timeseries_PT1H.csv");

    public static (string Directory, string Extension, string ZipEntryPath) GetTimeSeriesQuarterBasisDataForTotalGridAreaFileSpecification(Guid calculationId, string gridAreaCode)
        => ($"calculation-output/calculation_id={calculationId}/basis_data/time_series_quarter/grouping=total_ga/grid_area={gridAreaCode}/",
            ".csv",
            $"{gridAreaCode}/Timeseries_PT15M.csv");

    public static (string Directory, string Extension, string ZipEntryPath) GetMasterBasisDataFileForTotalGridAreaSpecification(Guid calculationId, string gridAreaCode)
        => ($"calculation-output/calculation_id={calculationId}/basis_data/master_basis_data/grouping=total_ga/grid_area={gridAreaCode}/",
            ".csv",
            $"{gridAreaCode}/MeteringPointMasterData.csv");

    public static (string Directory, string Extension, string ZipEntryPath) GetTimeSeriesHourBasisDataForEsPerGaGridAreaFileSpecification(Guid calculationId, string gridAreaCode, string energySupplierId)
        => ($"calculation-output/calculation_id={calculationId}/basis_data/time_series_hour/grouping=es_ga/grid_area={gridAreaCode}/energy_supplier_gln={energySupplierId}/",
            ".csv",
            $"{gridAreaCode}/Timeseries_PT1H.csv");

    public static (string Directory, string Extension, string ZipEntryPath) GetTimeSeriesQuarterBasisDataForEsPerGaFileSpecification(Guid calculationId, string gridAreaCode, string energySupplierId)
        => ($"calculation-output/calculation_id={calculationId}/basis_data/time_series_quarter/grouping=es_ga/grid_area={gridAreaCode}/energy_supplier_gln={energySupplierId}/",
            ".csv",
            $"{gridAreaCode}/Timeseries_PT15M.csv");

    public static (string Directory, string Extension, string ZipEntryPath) GetMasterBasisDataFileForForEsPerGaSpecification(Guid calculationId, string gridAreaCode, string energySupplierId)
        => ($"calculation-output/calculation_id={calculationId}/basis_data/master_basis_data/grouping=es_ga/grid_area={gridAreaCode}/energy_supplier_gln={energySupplierId}/",
            ".csv",
            $"{gridAreaCode}/MeteringPointMasterData.csv");

    public static string GetZipFileName(CalculationInfo calculationInfo) => $"calculation-output/calculation_id={calculationInfo.Id}/zip/gln=grid_area/calculation_{calculationInfo.Id}_{calculationInfo.PeriodStart}_{calculationInfo.PeriodEnd}.zip";

    private async Task<IEnumerable<(Stream FileStream, string EntryPath)>> GetCalculationBasisFileStreamsAsync(CalculationInfo calculationInfo)
    {
        var calculationBasisFiles = new List<(Stream FileStream, string EntryPath)>();

        foreach (var gridAreaCode in calculationInfo.GridAreaCodes)
        {
            var gridAreaFileUrls = await GetProcessBasisFileStreamsAsync(calculationInfo.Id, gridAreaCode).ConfigureAwait(false);
            calculationBasisFiles.AddRange(gridAreaFileUrls);
        }

        return calculationBasisFiles;
    }

    private async Task<List<(Stream FileStream, string EntryPath)>> GetProcessBasisFileStreamsAsync(Guid calculationId, string gridAreaCode)
    {
        var processDataFilesUrls = new List<(Stream FileStream, string EntryPath)>();

        foreach (var fileIdentifierProvider in _fileIdentifierProviders)
        {
            var (directory, extension, entryPath) = fileIdentifierProvider(calculationId, gridAreaCode);
            var filepath = await _dataLakeClient.FindFileAsync(directory, extension).ConfigureAwait(false);
            var stream = await _dataLakeClient.GetReadableFileStreamAsync(filepath).ConfigureAwait(false);
            processDataFilesUrls.Add((stream, entryPath));
        }

        return processDataFilesUrls;
    }
}
