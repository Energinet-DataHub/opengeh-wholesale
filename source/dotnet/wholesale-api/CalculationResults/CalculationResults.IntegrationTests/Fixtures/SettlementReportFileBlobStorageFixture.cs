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
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;

public sealed class SettlementReportFileBlobStorageFixture : IAsyncLifetime
{
    public SettlementReportFileBlobStorageFixture()
    {
        AzuriteManager = new AzuriteManager(useOAuth: true);
    }

    private AzuriteManager AzuriteManager { get; }

    public Task InitializeAsync()
    {
        AzuriteManager.StartAzurite();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        AzuriteManager.Dispose();
        return Task.CompletedTask;
    }

    public BlobContainerClient CreateBlobContainerClient()
    {
        var blobContainerClient = new BlobContainerClient(AzuriteManager.FullConnectionString, "settlement-report-container");
        blobContainerClient.CreateIfNotExists();
        return blobContainerClient;
    }
}
