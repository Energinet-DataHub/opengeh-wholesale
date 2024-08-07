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
using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks.DataLake;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// that allow adding DataLake services to an application.
/// </summary>
public static class DataLakeExtensions
{
    /// <summary>
    /// Register DataLake services commonly used by DH3 applications.
    /// </summary>
    public static IServiceCollection AddDataLakeClientForApplication(this IServiceCollection services)
    {
        services
            .AddOptions<DataLakeOptions>()
            .BindConfiguration(configSectionPath: string.Empty);

        services.AddSingleton(sp =>
        {
            var dataLakeOptions = sp.GetRequiredService<IOptions<DataLakeOptions>>().Value;

            var dataLakeServiceClient = new DataLakeServiceClient(new Uri(dataLakeOptions.STORAGE_ACCOUNT_URI), new DefaultAzureCredential());
            return dataLakeServiceClient.GetFileSystemClient(dataLakeOptions.STORAGE_CONTAINER_NAME);
        });

        // Health checks
        services.AddHealthChecks()
            .AddDataLakeHealthCheck(
                sp => sp.GetRequiredService<IOptions<DataLakeOptions>>().Value,
                name: "DataLake");

        return services;
    }
}
