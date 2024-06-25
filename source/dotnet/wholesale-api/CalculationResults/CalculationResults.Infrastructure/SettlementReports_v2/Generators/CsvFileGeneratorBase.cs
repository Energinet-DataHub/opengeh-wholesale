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
using CsvHelper;
using CsvHelper.Configuration;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Generators;

public abstract class CsvFileGeneratorBase<TRow, TClassMap> : ISettlementReportFileGenerator
    where TClassMap : ClassMap<TRow>
{
    private readonly int _chunkSize;

    protected CsvFileGeneratorBase(int chunkSize)
    {
        _chunkSize = chunkSize;
    }

    public string FileExtension => ".csv";

    public async Task<int> CountChunksAsync(MarketRole marketRole, SettlementReportRequestFilterDto filter, long maximumCalculationVersion)
    {
        var count = await CountAsync(marketRole, filter, maximumCalculationVersion).ConfigureAwait(false);
        return (int)Math.Ceiling(count / (double)_chunkSize);
    }

    public async Task WriteAsync(
        MarketRole marketRole,
        SettlementReportRequestFilterDto filter,
        SettlementReportPartialFileInfo fileInfo,
        long maximumCalculationVersion,
        StreamWriter destination)
    {
        var csvHelper = new CsvWriter(destination, new CultureInfo(filter.CsvFormatLocale ?? "en-US"));
        RegisterClassMap(csvHelper, marketRole);
        ConfigureCsv(csvHelper);

        await using (csvHelper.ConfigureAwait(false))
        {
            if (fileInfo is { FileOffset: 0, ChunkOffset: 0 })
            {
                WriteHeader(csvHelper);
                await csvHelper.NextRecordAsync().ConfigureAwait(false);
            }

            var dataSourceEnumerable = GetAsync(marketRole, filter, maximumCalculationVersion, fileInfo.ChunkOffset * _chunkSize, _chunkSize);

            await foreach (var record in dataSourceEnumerable.ConfigureAwait(false))
            {
                csvHelper.WriteRecord(record);
                await csvHelper.NextRecordAsync().ConfigureAwait(false);
            }
        }
    }

    protected abstract Task<int> CountAsync(MarketRole marketRole, SettlementReportRequestFilterDto filter, long maximumCalculationVersion);

    protected abstract IAsyncEnumerable<TRow> GetAsync(
        MarketRole marketRole,
        SettlementReportRequestFilterDto filter,
        long maximumCalculationVersion,
        int skipChunks,
        int takeChunks);

    protected virtual void ConfigureCsv(CsvWriter csvHelper)
    {
    }

    protected virtual void WriteHeader(CsvWriter csvHelper)
    {
        csvHelper.WriteHeader<TRow>();
    }

    protected virtual void RegisterClassMap(CsvWriter csvHelper, MarketRole marketRole)
    {
        csvHelper.Context.RegisterClassMap<TClassMap>();
    }
}
