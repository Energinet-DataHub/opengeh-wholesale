//  Copyright 2020 Energinet DataHub A/S
//
//  Licensed under the Apache License, Version 2.0 (the "License2");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using Azure.Storage.Blobs;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence.Processes;

namespace Energinet.DataHub.Wholesale.Sender.Infrastructure.Services.ResultSender;

public class ResultReader : IResultReader
{
    private readonly BlobContainerClient _blobClient;

    public ResultReader(BlobContainerClient blobClient)
    {
        _blobClient = blobClient;
    }

    public Task<BalanceFixingResultDto> GetResultAsync(Process process)
    {
        var resultsPath = EnvironmentSettingNames.ResultsPath + $"/batch_id={process.BatchId}/grid_area={process.GridAreaCode}";
        var client = _blobClient.GetBlobClient(resultsPath);
    }
}
