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

public sealed class SettlementReportFileRequestHandler : ISettlementReportFileRequestHandler
{
    private readonly ISettlementReportFileGeneratorFactory _fileGeneratorFactory;
    private readonly ISettlementReportFileRepository _fileRepository;

    public SettlementReportFileRequestHandler(
        ISettlementReportFileGeneratorFactory fileGeneratorFactory,
        ISettlementReportFileRepository fileRepository)
    {
        _fileGeneratorFactory = fileGeneratorFactory;
        _fileRepository = fileRepository;
    }

    public async Task<GeneratedSettlementReportFileDto> RequestFileAsync(SettlementReportFileRequestDto fileRequest)
    {
        var fileGenerator = _fileGeneratorFactory.Create(fileRequest.FileContent);

        var resultingFileName = GenerateFilename(fileRequest) + fileGenerator.FileExtension;
        var storageFileName = $"{fileRequest.PartialFileInfo.FileName}_{fileRequest.PartialFileInfo.FileOffset}_{fileRequest.PartialFileInfo.ChunkOffset}{fileGenerator.FileExtension}";

        var writeStream = await _fileRepository
            .OpenForWritingAsync(fileRequest.RequestId, storageFileName)
            .ConfigureAwait(false);

        await using (writeStream.ConfigureAwait(false))
        {
            var streamWriter = new StreamWriter(writeStream);
            await using (streamWriter.ConfigureAwait(false))
            {
                await fileGenerator
                    .WriteAsync(
                        fileRequest.MarketRole,
                        fileRequest.RequestFilter,
                        fileRequest.PartialFileInfo,
                        fileRequest.MaximumCalculationVersion,
                        streamWriter)
                    .ConfigureAwait(false);
            }
        }

        return new GeneratedSettlementReportFileDto(
            fileRequest.RequestId,
            fileRequest.PartialFileInfo with { FileName = resultingFileName },
            storageFileName);
    }

    private string GenerateFilename(SettlementReportFileRequestDto fileRequest)
    {
        var filename = $"{fileRequest.PartialFileInfo.FileName}";

        if (!string.IsNullOrWhiteSpace(fileRequest.RequestFilter.EnergySupplier))
        {
            filename += $"_{fileRequest.RequestFilter.EnergySupplier}";
        }

        switch (fileRequest.MarketRole)
        {
            case MarketRole.EnergySupplier:
                filename += "_DDQ";
                break;
            case MarketRole.GridAccessProvider:
                filename += "_DDQ";
                break;
        }

        filename += $"_{fileRequest.RequestFilter.PeriodStart:dd-MM-yyyy}";
        filename += $"_{fileRequest.RequestFilter.PeriodEnd:dd-MM-yyyy}";

        return filename;
    }
}
