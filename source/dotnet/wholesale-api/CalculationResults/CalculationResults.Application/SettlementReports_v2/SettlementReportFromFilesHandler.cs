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
using System.Text;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;

public sealed class SettlementReportFromFilesHandler : ISettlementReportFromFilesHandler
{
    private const int LargeTextFileThreshold = 1_000_000;
    private readonly ISettlementReportFileRepository _fileRepository;

    public SettlementReportFromFilesHandler(ISettlementReportFileRepository fileRepository)
    {
        _fileRepository = fileRepository;
    }

    public async Task<GeneratedSettlementReportDto> CombineAsync(
        SettlementReportRequestId requestId,
        IReadOnlyCollection<GeneratedSettlementReportFileDto> generatedFiles)
    {
        var reportFileName = "Report.zip";

        var compressedStream = await _fileRepository
            .OpenForWritingAsync(requestId, reportFileName)
            .ConfigureAwait(false);

        await using (compressedStream.ConfigureAwait(false))
        {
            using var archive = new ZipArchive(compressedStream, ZipArchiveMode.Create);

            foreach (var chunks in generatedFiles.GroupBy(x => x.FileInfo.FileName))
            {
                if (chunks.Any(c => c.FileInfo.PreventLargeTextFiles))
                {
                    await CombineChunksLimitSizeAsync(archive, chunks.Key, chunks).ConfigureAwait(false);
                }
                else
                {
                    await CombineChunksAsync(archive, chunks.Key, chunks).ConfigureAwait(false);
                }
            }
        }

        return new GeneratedSettlementReportDto(requestId, reportFileName, generatedFiles);
    }

    private async Task CombineChunksAsync(ZipArchive archive, string entryName, IEnumerable<GeneratedSettlementReportFileDto> chunks)
    {
        var entry = archive.CreateEntry(entryName);
        var entryStream = entry.Open();

        await using (entryStream.ConfigureAwait(false))
        {
            foreach (var chunk in chunks.OrderBy(c => c.FileInfo.ChunkOffset))
            {
                var readStream = await _fileRepository
                    .OpenForReadingAsync(chunk.RequestId, chunk.StorageFileName)
                    .ConfigureAwait(false);

                await using (readStream.ConfigureAwait(false))
                {
                    await readStream.CopyToAsync(entryStream).ConfigureAwait(false);
                }
            }
        }
    }

    private async Task CombineChunksLimitSizeAsync(ZipArchive archive, string entryName, IEnumerable<GeneratedSettlementReportFileDto> chunks)
    {
        var fileCount = 0;
        var rowCount = 0;

        var entryStream = StreamWriter.Null;
        var header = string.Empty;

        foreach (var chunk in chunks.OrderBy(c => c.FileInfo.ChunkOffset))
        {
            var readStream = await _fileRepository
                .OpenForReadingAsync(chunk.RequestId, chunk.StorageFileName)
                .ConfigureAwait(false);

            using var streamReader = new StreamReader(readStream, Encoding.UTF8, leaveOpen: false);

            while (!streamReader.EndOfStream)
            {
                var nextLine = await streamReader
                    .ReadLineAsync()
                    .ConfigureAwait(false);

                if (fileCount == 0 && rowCount == 0)
                    header = nextLine;

                if (entryStream == StreamWriter.Null || rowCount == LargeTextFileThreshold)
                {
                    await entryStream.DisposeAsync().ConfigureAwait(false);

                    var name = GenerateSplitFileName(entryName, fileCount);
                    var entry = archive.CreateEntry(name);

                    entryStream = new StreamWriter(entry.Open(), Encoding.UTF8, leaveOpen: false);
                    rowCount = 0;

                    if (fileCount != 0)
                    {
                        await entryStream.WriteLineAsync(header).ConfigureAwait(false);
                        rowCount++;
                    }

                    fileCount++;
                }

                await entryStream.WriteLineAsync(nextLine).ConfigureAwait(false);
                rowCount++;
            }
        }
    }

    private static string GenerateSplitFileName(string entryName, int fileCount)
    {
        if (fileCount == 0)
            return entryName;

        var ext = Path.GetExtension(entryName);
        var name = Path.GetFileNameWithoutExtension(entryName);

        return $"{name} - {fileCount + 1}{ext}";
    }
}
