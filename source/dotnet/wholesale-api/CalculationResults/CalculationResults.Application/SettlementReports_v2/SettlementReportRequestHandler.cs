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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;

public sealed class SettlementReportRequestHandler : ISettlementReportRequestHandler
{
    private readonly ISettlementReportFileGeneratorFactory _fileGeneratorFactory;
    private readonly ILatestCalculationVersionRepository _latestCalculationVersionRepository;

    public SettlementReportRequestHandler(
        ISettlementReportFileGeneratorFactory fileGeneratorFactory,
        ILatestCalculationVersionRepository latestCalculationVersionRepository)
    {
        _fileGeneratorFactory = fileGeneratorFactory;
        _latestCalculationVersionRepository = latestCalculationVersionRepository;
    }

    public async Task<IEnumerable<SettlementReportFileRequestDto>> RequestReportAsync(
        SettlementReportRequestId requestId,
        SettlementReportRequestDto reportRequest,
        SettlementReportRequestedByActor actorInfo)
    {
        const string energyResultFileName = "RESULTENERGY";
        const string wholesaleResultFileName = "RESULTWHOLESALE";

        var filesInReport = reportRequest.Filter.CalculationType switch
        {
            CalculationType.BalanceFixing => new[]
            {
                new { Content = SettlementReportFileContent.EnergyResult, Name = energyResultFileName, reportRequest.SplitReportPerGridArea },
            },
            CalculationType.WholesaleFixing or CalculationType.FirstCorrectionSettlement or CalculationType.SecondCorrectionSettlement or CalculationType.ThirdCorrectionSettlement =>
            [
                new { Content = SettlementReportFileContent.EnergyResult, Name = energyResultFileName, reportRequest.SplitReportPerGridArea },
                new { Content = SettlementReportFileContent.WholesaleResult, Name = wholesaleResultFileName, reportRequest.SplitReportPerGridArea }
            ],
            _ => throw new InvalidOperationException($"Cannot generate report for calculation type {reportRequest.Filter.CalculationType}."),
        };

        if (reportRequest.IncludeBasisData)
        {
            filesInReport =
            [
                ..filesInReport,
                ..reportRequest.Filter.CalculationType switch
                {
                    CalculationType.WholesaleFixing or CalculationType.FirstCorrectionSettlement or CalculationType.SecondCorrectionSettlement or CalculationType.ThirdCorrectionSettlement => new[]
                    {
                        new { Content = SettlementReportFileContent.ChargeLinksPeriods, Name = "CHARGELINK", SplitReportPerGridArea = true },
                        new { Content = SettlementReportFileContent.MeteringPointMasterData, Name = "MDMP", SplitReportPerGridArea = true },
                        new { Content = SettlementReportFileContent.Pt15M, Name = "TSSD15", SplitReportPerGridArea = true },
                        new { Content = SettlementReportFileContent.Pt1H, Name = "TSSD60", SplitReportPerGridArea = true },
                        new { Content = SettlementReportFileContent.ChargePrice, Name = "CHARGEPRICE", SplitReportPerGridArea = true },
                    },
                    CalculationType.BalanceFixing =>
                    [
                        new { Content = SettlementReportFileContent.MeteringPointMasterData, Name = "MDMP", SplitReportPerGridArea = true }
                    ],
                    _ => throw new InvalidOperationException($"Cannot generate basis data for calculation type {reportRequest.Filter.CalculationType}."),
                }
            ];
        }

        if (reportRequest.IncludeMonthlyAmount && IsWholeMonth(reportRequest.Filter.PeriodStart, reportRequest.Filter.PeriodEnd)
                                               && reportRequest.Filter.CalculationType
                                                   is CalculationType.WholesaleFixing
                                                   or CalculationType.FirstCorrectionSettlement
                                                   or CalculationType.SecondCorrectionSettlement
                                                   or CalculationType.ThirdCorrectionSettlement)
        {
            filesInReport =
            [
                ..filesInReport,
                ..new[]
                {
                    new { Content = SettlementReportFileContent.MonthlyAmount, Name = "RESULTMONTHLY", SplitReportPerGridArea = true },
                    new { Content = SettlementReportFileContent.MonthlyAmountTotal, Name = "RESULTMONTHLY", SplitReportPerGridArea = true },
                }
            ];
        }

        var maxCalculationVersion = await GetLatestCalculationVersionAsync(reportRequest.Filter.CalculationType).ConfigureAwait(false);
        var filesToRequest = new List<SettlementReportFileRequestDto>();
        foreach (var file in filesInReport)
        {
            var fileRequest = new SettlementReportFileRequestDto(
                requestId,
                file.Content,
                new SettlementReportPartialFileInfo(file.Name, true),
                reportRequest.Filter,
                maxCalculationVersion);

            if (file.Content == SettlementReportFileContent.MonthlyAmountTotal)
            {
                    fileRequest = new SettlementReportFileRequestDto(
                    requestId,
                    file.Content,
                    new SettlementReportPartialFileInfo(file.Name, true) { FileOffset = int.MaxValue },
                    reportRequest.Filter,
                    maxCalculationVersion);
            }

            await foreach (var splitFileRequest in SplitFileRequestPerGridAreaAsync(fileRequest, actorInfo, file.SplitReportPerGridArea).ConfigureAwait(false))
            {
                filesToRequest.Add(splitFileRequest);
            }
        }

        return filesToRequest;
    }

    private async IAsyncEnumerable<SettlementReportFileRequestDto> SplitFileRequestPerGridAreaAsync(
        SettlementReportFileRequestDto fileRequest,
        SettlementReportRequestedByActor actorInfo,
        bool splitReportPerGridArea)
    {
        var partialFileInfo = fileRequest.PartialFileInfo;

        foreach (var (gridAreaCode, calculationId) in fileRequest.RequestFilter.GridAreas)
        {
            if (splitReportPerGridArea)
            {
                partialFileInfo = fileRequest.PartialFileInfo with
                {
                    FileName = fileRequest.PartialFileInfo.FileName + $"_{gridAreaCode}",
                };
            }

            var requestForSingleGridArea = fileRequest with
            {
                PartialFileInfo = partialFileInfo,

                // Create a request with a single grid area.
                RequestFilter = fileRequest.RequestFilter with { GridAreas = new Dictionary<string, CalculationId?> { { gridAreaCode, calculationId } } },
            };

            // Split the single grid area request into further chunks.
            await foreach (var splitFileRequest in SplitFileRequestIntoChunksAsync(requestForSingleGridArea, actorInfo).ConfigureAwait(false))
            {
                yield return splitFileRequest;

                partialFileInfo = splitFileRequest.PartialFileInfo with
                {
                    FileOffset = splitFileRequest.PartialFileInfo.FileOffset + 1,
                    ChunkOffset = 0,
                };
            }
        }
    }

    private async IAsyncEnumerable<SettlementReportFileRequestDto> SplitFileRequestIntoChunksAsync(
        SettlementReportFileRequestDto fileRequest,
        SettlementReportRequestedByActor actorInfo)
    {
        var partialFileInfo = fileRequest.PartialFileInfo;

        var fileGenerator = _fileGeneratorFactory.Create(fileRequest.FileContent);
        var chunks = await fileGenerator
            .CountChunksAsync(fileRequest.RequestFilter, actorInfo, fileRequest.MaximumCalculationVersion)
            .ConfigureAwait(false);

        for (var i = 0; i < chunks; i++)
        {
            yield return fileRequest with
            {
                PartialFileInfo = partialFileInfo with { ChunkOffset = partialFileInfo.ChunkOffset + i },
            };
        }
    }

    private static bool IsWholeMonth(DateTimeOffset start, DateTimeOffset end)
    {
        var convertedStart = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(start, "Romance Standard Time");
        var convertedEnd = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(end, "Romance Standard Time");
        return convertedEnd.TimeOfDay.Ticks == 0
            && convertedStart.Day == 1
            && convertedEnd.Day == 1
            && convertedEnd.Month - convertedStart.Month == 1;
    }

    private Task<long> GetLatestCalculationVersionAsync(CalculationType calculationType)
    {
        return calculationType == CalculationType.BalanceFixing
            ? _latestCalculationVersionRepository.GetLatestCalculationVersionAsync()
            : Task.FromResult(long.MaxValue);
    }
}
