﻿// Copyright 2020 Energinet DataHub A/S
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Extensions.DependencyInjection;

public static class StorageExtensions
{
    public static IServiceCollection AddSettlementReportBlobStorage(this IServiceCollection services)
    {
        services
            .AddOptions<SettlementReportStorageOptions>()
            .BindConfiguration(SettlementReportStorageOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddScoped<ISettlementReportFileRepository, SettlementReportFileBlobStorage>(serviceProvider =>
        {
            var blobSettings = serviceProvider.GetRequiredService<IOptions<SettlementReportStorageOptions>>().Value;

            var blobContainerUri = new Uri(blobSettings.StorageAccountUri, blobSettings.StorageContainerName);
            var blobContainerClient = new BlobContainerClient(blobContainerUri, new DefaultAzureCredential());

            return new SettlementReportFileBlobStorage(blobContainerClient);
        });

        // Health checks
        services.AddHealthChecks().AddAzureBlobStorage(
        serviceProvider =>
        {
            var blobSettings = serviceProvider.GetRequiredService<IOptions<SettlementReportStorageOptions>>().Value;
            return new BlobServiceClient(blobSettings.StorageAccountUri, new DefaultAzureCredential());
        },
        (serviceProvider, options) =>
        {
            var blobSettings = serviceProvider.GetRequiredService<IOptions<SettlementReportStorageOptions>>().Value;
            options.ContainerName = blobSettings.StorageContainerName;
        });

        return services;
    }
}
