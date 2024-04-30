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

using Azure.Identity;
using Azure.Storage.Blobs;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Extensions.Options;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// that allow adding DataLake services to an application.
/// </summary>
public static class StorageExtensions
{
    /// <summary>
    /// Register DataLake services commonly used by DH3 applications.
    /// </summary>
    public static IServiceCollection AddSettlementReportStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<SettlementReportStorageOptions>()
            .BindConfiguration(configSectionPath: string.Empty);

        services.AddScoped<ISettlementReportStorage, SettlementReportStorage>(d =>
        {
            var blobSettings = d.GetRequiredService<IOptions<SettlementReportStorageOptions>>().Value;

            var webJobsStorage = configuration.GetValue<string>("AzureWebJobsStorage");
            if (webJobsStorage == "UseDevelopmentStorage=true")
            {
                var containerClient = new BlobContainerClient(webJobsStorage, blobSettings.StorageContainerName);
                containerClient.CreateIfNotExists();
                return new SettlementReportStorage(containerClient);
            }

            var blobContainerUri = new Uri(blobSettings.StorageAccountUri, blobSettings.StorageContainerName);
            var blobContainerClient = new BlobContainerClient(blobContainerUri, new DefaultAzureCredential());

            return new SettlementReportStorage(blobContainerClient);
        });

        var blobStorageSettings = configuration.GetRequiredSection(nameof(SettlementReportStorageOptions)).Get<SettlementReportStorageOptions>();
        ArgumentNullException.ThrowIfNull(blobStorageSettings);

        // Health checks
        services
            .AddHealthChecks()
            .AddAzureBlobStorage(
                blobStorageSettings.StorageAccountUri,
                new DefaultAzureCredential(),
                blobStorageSettings.StorageContainerName);

        return services;
    }
}
