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
    private const int LargeTextFileThreshold = 1_000;
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
            foreach (var chunk in chunks
                         .OrderBy(c => c.FileInfo.FileOffset)
                         .ThenBy(c => c.FileInfo.ChunkOffset))
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
        var sizeLimitedEntry = new SizeLimitedEntry(archive, entryName);

        foreach (var chunk in chunks
                     .OrderBy(c => c.FileInfo.FileOffset)
                     .ThenBy(c => c.FileInfo.ChunkOffset))
        {
            var readStream = await _fileRepository
                .OpenForReadingAsync(chunk.RequestId, chunk.StorageFileName)
                .ConfigureAwait(false);

            using var source = new LargeTextFileSource(readStream);

            while (!source.IsEmpty)
            {
                if (sizeLimitedEntry.IsFull)
                {
                    sizeLimitedEntry.Dispose();
                    sizeLimitedEntry = sizeLimitedEntry.CreateNextEntry();
                }

                while (!sizeLimitedEntry.IsFull && !source.IsEmpty)
                {
                    await source.WriteToAsync(sizeLimitedEntry).ConfigureAwait(false);
                }
            }
        }

        sizeLimitedEntry.Dispose();
    }

    private sealed class SizeLimitedEntry : IDisposable
    {
        private readonly ZipArchive _archive;
        private readonly string _entryBaseName;

        private int _rows;
        private int _entrySequenceNumber;

        private string _header = string.Empty;
        private StreamWriter _entryStream = StreamWriter.Null;

        public SizeLimitedEntry(ZipArchive archive, string entryName)
        {
            _archive = archive;
            _entryBaseName = entryName;
        }

        public bool IsFull => _rows == LargeTextFileThreshold;

        public async ValueTask AppendAsync(string line)
        {
            if (_rows == 0 && _entrySequenceNumber == 0)
                _header = line;

            if (_entryStream == StreamWriter.Null)
            {
                var entry = _archive.CreateEntry(GenerateFileName());
                _entryStream = new StreamWriter(entry.Open(), Encoding.UTF8, leaveOpen: false);

                if (_entrySequenceNumber != 0)
                {
                    await _entryStream.WriteLineAsync(_header).ConfigureAwait(false);
                    _rows++;
                }
            }

            await _entryStream.WriteLineAsync(line).ConfigureAwait(false);

            _rows++;
        }

        public SizeLimitedEntry CreateNextEntry()
        {
            return new SizeLimitedEntry(_archive, _entryBaseName) { _header = _header, _entrySequenceNumber = _entrySequenceNumber + 1 };
        }

        public void Dispose()
        {
            _entryStream.Dispose();
        }

        private string GenerateFileName()
        {
            if (_entrySequenceNumber == 0)
                return _entryBaseName;

            var ext = Path.GetExtension(_entryBaseName);
            var name = Path.GetFileNameWithoutExtension(_entryBaseName);

            return $"{name} - {_entrySequenceNumber + 1}{ext}";
        }
    }

    private sealed class LargeTextFileSource : IDisposable
    {
        private readonly StreamReader _reader;

        public LargeTextFileSource(Stream dataSource)
        {
            _reader = new StreamReader(dataSource, Encoding.UTF8, leaveOpen: false);
        }

        public bool IsEmpty => _reader.EndOfStream;

        public async ValueTask WriteToAsync(SizeLimitedEntry sizeLimitedEntry)
        {
            var nextLine = await _reader.ReadLineAsync().ConfigureAwait(false);
            if (nextLine != null)
            {
                await sizeLimitedEntry.AppendAsync(nextLine).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
