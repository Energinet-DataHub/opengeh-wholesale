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

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportFileBlobStorage : ISettlementReportFileRepository
{
    private readonly BlobContainerClient _blobContainerClient;

    public SettlementReportFileBlobStorage(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public Task<Stream> OpenForReadingAsync(SettlementReportRequestId reportRequestId, string fileName)
    {
        var blobName = GetBlobName(reportRequestId, fileName);
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        return blobClient.OpenReadAsync();
    }

    public Task<Stream> OpenForWritingAsync(SettlementReportRequestId reportRequestId, string fileName)
    {
        var blobName = GetBlobName(reportRequestId, fileName);
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        return blobClient.OpenWriteAsync(true);
    }

    public Task DeleteAsync(SettlementReportRequestId reportRequestId, string fileName)
    {
        var blobName = GetBlobName(reportRequestId, fileName);
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        return blobClient.DeleteIfExistsAsync();
    }

    private static string GetBlobName(SettlementReportRequestId reportRequestId, string fileName)
    {
        return Path.Combine("settlement-reports", reportRequestId.Id, fileName);
    }
}
