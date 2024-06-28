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
    private readonly HashSet<int>? _quotedColumns;

    protected CsvFileGeneratorBase(int chunkSize, HashSet<int>? quotedColumns = null)
    {
        _chunkSize = chunkSize;
        _quotedColumns = quotedColumns;
    }

    public string FileExtension => ".csv";

    public async Task<int> CountChunksAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, long maximumCalculationVersion)
    {
        var count = await CountAsync(filter, actorInfo, maximumCalculationVersion).ConfigureAwait(false);
        return (int)Math.Ceiling(count / (double)_chunkSize);
    }

    public async Task WriteAsync(
        SettlementReportRequestFilterDto filter,
        SettlementReportRequestedByActor actorInfo,
        SettlementReportPartialFileInfo fileInfo,
        long maximumCalculationVersion,
        StreamWriter destination)
    {
        bool IncludeHeader()
        {
            return fileInfo is { FileOffset: 0, ChunkOffset: 0 };
        }

        bool IsHeaderRow(ShouldQuoteArgs args)
        {
            return IncludeHeader() && args.Row.Row == 1;
        }

        bool ShouldQuote(ShouldQuoteArgs args)
        {
            return _quotedColumns is { Count: > 0 } &&
                   _quotedColumns.Contains(args.Row.Index);
        }

        var csvHelper = new CsvWriter(destination, new CsvConfiguration(new CultureInfo(filter.CsvFormatLocale ?? "en-US"))
        {
            ShouldQuote = args => !IsHeaderRow(args) && ShouldQuote(args),
        });

        RegisterClassMap(csvHelper, filter, actorInfo);
        ConfigureCsv(csvHelper);

        await using (csvHelper.ConfigureAwait(false))
        {
            if (IncludeHeader())
            {
                WriteHeader(csvHelper);
                await csvHelper.NextRecordAsync().ConfigureAwait(false);
            }

            var dataSourceEnumerable = GetAsync(filter, actorInfo, maximumCalculationVersion, fileInfo.ChunkOffset * _chunkSize, _chunkSize);

            await foreach (var record in dataSourceEnumerable.ConfigureAwait(false))
            {
                csvHelper.WriteRecord(record);
                await csvHelper.NextRecordAsync().ConfigureAwait(false);
            }
        }
    }

    protected abstract Task<int> CountAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, long maximumCalculationVersion);

    protected abstract IAsyncEnumerable<TRow> GetAsync(
        SettlementReportRequestFilterDto filter,
        SettlementReportRequestedByActor actorInfo,
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

    protected virtual void RegisterClassMap(CsvWriter csvHelper, SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo)
    {
        csvHelper.Context.RegisterClassMap<TClassMap>();
    }
}
