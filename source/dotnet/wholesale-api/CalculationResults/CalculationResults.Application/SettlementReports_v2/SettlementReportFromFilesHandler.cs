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

using System.IO.Compression;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;

public sealed class SettlementReportFromFilesHandler : ISettlementReportFromFilesHandler
{
    private readonly ISettlementReportFileRepository _fileRepository;

    public SettlementReportFromFilesHandler(ISettlementReportFileRepository fileRepository)
    {
        _fileRepository = fileRepository;
    }

    public async Task<GeneratedSettlementReportDto> CombineAsync(IReadOnlyCollection<GeneratedSettlementReportFileDto> generatedFiles)
    {
        var reportFileName = "Report.zip";
        var requestId = generatedFiles
            .Select(file => file.RequestId)
            .Distinct()
            .Single();

        var compressedStream = await _fileRepository
            .OpenForWritingAsync(requestId, reportFileName)
            .ConfigureAwait(false);

        await using (compressedStream.ConfigureAwait(false))
        {
            using var archive = new ZipArchive(compressedStream, ZipArchiveMode.Create);

            foreach (var reportFilesGrouped in generatedFiles.GroupBy(x => x.FileName))
            {
                var entry = archive.CreateEntry(reportFilesGrouped.Key);
                var entryStream = entry.Open();
                await using (entryStream.ConfigureAwait(false))
                {
                    foreach (var reportFile in reportFilesGrouped)
                    {
                        var readStream = await _fileRepository
                            .OpenForReadingAsync(requestId, reportFile.PartialFileName ?? reportFile.FileName)
                            .ConfigureAwait(false);

                        await using (readStream.ConfigureAwait(false))
                        {
                            await readStream.CopyToAsync(entryStream).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        return new GeneratedSettlementReportDto(
            requestId,
            new GeneratedSettlementReportFileDto(requestId, reportFileName),
            generatedFiles);
    }
}
