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

    public SettlementReportRequestHandler(ISettlementReportFileGeneratorFactory fileGeneratorFactory)
    {
        _fileGeneratorFactory = fileGeneratorFactory;
    }

    public async Task<IEnumerable<SettlementReportFileRequestDto>> RequestReportAsync(
        SettlementReportRequestId requestId,
        SettlementReportRequestDto reportRequest)
    {
        var setsOfFiles = new List<IAsyncEnumerable<SettlementReportFileRequestDto>>();

        switch (reportRequest.CalculationType)
        {
            case CalculationType.BalanceFixing:
                setsOfFiles.Add(RequestFilesForEnergyResultsAsync(true, requestId, reportRequest));
                break;
            case CalculationType.WholesaleFixing:
                setsOfFiles.Add(RequestFilesForEnergyResultsAsync(false, requestId, reportRequest));
                setsOfFiles.Add(RequestFilesForWholesaleResultsAsync(SettlementReportFileContent.WholesaleResult, requestId, reportRequest));
                break;
            case CalculationType.FirstCorrectionSettlement:
                setsOfFiles.Add(RequestFilesForEnergyResultsAsync(false, requestId, reportRequest));
                setsOfFiles.Add(RequestFilesForWholesaleResultsAsync(SettlementReportFileContent.FirstCorrectionResult, requestId, reportRequest));
                break;
            case CalculationType.SecondCorrectionSettlement:
                setsOfFiles.Add(RequestFilesForEnergyResultsAsync(false, requestId, reportRequest));
                setsOfFiles.Add(RequestFilesForWholesaleResultsAsync(SettlementReportFileContent.SecondCorrectionResult, requestId, reportRequest));
                break;
            case CalculationType.ThirdCorrectionSettlement:
                setsOfFiles.Add(RequestFilesForEnergyResultsAsync(false, requestId, reportRequest));
                setsOfFiles.Add(RequestFilesForWholesaleResultsAsync(SettlementReportFileContent.ThirdCorrectionResult, requestId, reportRequest));
                break;
            default:
                throw new InvalidOperationException($"Cannot generate report for calculation type {reportRequest.CalculationType}.");
        }

        var filesToRequest = new List<SettlementReportFileRequestDto>();

        foreach (var fileSet in setsOfFiles)
        {
            await foreach (var fileRequest in fileSet.ConfigureAwait(false))
            {
                filesToRequest.Add(fileRequest);
            }
        }

        return filesToRequest;
    }

    private async IAsyncEnumerable<SettlementReportFileRequestDto> RequestFilesForEnergyResultsAsync(
        bool takeLatestPerDay,
        SettlementReportRequestId requestId,
        SettlementReportRequestDto reportRequest)
    {
        var fileContent = takeLatestPerDay
            ? SettlementReportFileContent.EnergyResultLatestPerDay
            : SettlementReportFileContent.EnergyResultForCalculationId;

        var resultEnergy = new SettlementReportFileRequestDto(
            fileContent,
            new SettlementReportPartialFileInfo("Result Energy"),
            requestId,
            reportRequest.Filter);

        await foreach (var splitFileRequest in SplitFileRequestPerGridAreaAsync(resultEnergy, reportRequest.SplitReportPerGridArea).ConfigureAwait(false))
        {
            yield return splitFileRequest;
        }
    }

    private async IAsyncEnumerable<SettlementReportFileRequestDto> RequestFilesForWholesaleResultsAsync(
        SettlementReportFileContent wholesaleFileContent,
        SettlementReportRequestId requestId,
        SettlementReportRequestDto reportRequest)
    {
        var resultWholesale = new SettlementReportFileRequestDto(
                wholesaleFileContent,
                new SettlementReportPartialFileInfo("Result Wholesale"),
                requestId,
                reportRequest.Filter);

        await foreach (var splitFileRequest in SplitFileRequestPerGridAreaAsync(resultWholesale, reportRequest.SplitReportPerGridArea).ConfigureAwait(false))
        {
            yield return splitFileRequest;
        }
    }

    private async IAsyncEnumerable<SettlementReportFileRequestDto> SplitFileRequestPerGridAreaAsync(
        SettlementReportFileRequestDto fileRequest,
        bool splitReportPerGridArea)
    {
        var partialFileInfo = fileRequest.PartialFileInfo;

        foreach (var (gridAreaCode, calculationId) in fileRequest.RequestFilter.GridAreas)
        {
            if (splitReportPerGridArea)
            {
                partialFileInfo = fileRequest.PartialFileInfo with
                {
                    FileName = fileRequest.PartialFileInfo.FileName + $" ({gridAreaCode})",
                };
            }

            var requestForSingleGridArea = fileRequest with
            {
                PartialFileInfo = partialFileInfo,

                // Create a request with a single grid area.
                RequestFilter = fileRequest.RequestFilter with { GridAreas = new Dictionary<string, CalculationId> { { gridAreaCode, calculationId } } },
            };

            // Split the single grid area request into further chunks.
            await foreach (var splitFileRequest in SplitFileRequestIntoChunksAsync(requestForSingleGridArea).ConfigureAwait(false))
            {
                yield return splitFileRequest;

                // Keep track of the offset of the chunks, so that the next grid area begins at the correct offset.
                partialFileInfo = splitFileRequest.PartialFileInfo with
                {
                    ChunkOffset = splitFileRequest.PartialFileInfo.ChunkOffset + 1,
                };
            }
        }
    }

    // Note: Always return ChunkOffset in increasing order, as SplitFileRequestPerGridAreaAsync expects last ChunkOffset to be the highest.
    private async IAsyncEnumerable<SettlementReportFileRequestDto> SplitFileRequestIntoChunksAsync(
        SettlementReportFileRequestDto fileRequest)
    {
        var partialFileInfo = fileRequest.PartialFileInfo;

        var fileGenerator = _fileGeneratorFactory.Create(fileRequest.FileContent);
        var chunks = await fileGenerator
            .CountChunksAsync(fileRequest.RequestFilter)
            .ConfigureAwait(false);

        for (var i = 0; i < chunks; i++)
        {
            yield return fileRequest with
            {
                PartialFileInfo = partialFileInfo with { ChunkOffset = partialFileInfo.ChunkOffset + i },
            };
        }
    }
}
