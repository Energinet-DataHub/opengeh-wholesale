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

namespace Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;

public sealed class SettlementReportInChunksRequestHandler : ISettlementReportInChunksRequestHandler
{
    private readonly ISettlementReportFileGeneratorFactory _fileGeneratorFactory;

    public SettlementReportInChunksRequestHandler(ISettlementReportFileGeneratorFactory fileGeneratorFactory)
    {
        _fileGeneratorFactory = fileGeneratorFactory;
    }

    public async Task<IEnumerable<SettlementReportFileRequestDto>> RequestReportInChunksAsync(
        IEnumerable<SettlementReportFileRequestDto> settlementReportFileRequest,
        SettlementReportRequestedByActor actorInfo)
    {
        var filesToRequest = new List<SettlementReportFileRequestDto>();
        foreach (var settlementReportFile in settlementReportFileRequest)
        {
            await foreach (var splitFileRequest in SplitFileRequestIntoChunksAsync(settlementReportFile, actorInfo)
                               .ConfigureAwait(false))
            {
                filesToRequest.Add(splitFileRequest);
            }
        }

        return filesToRequest;
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
}
