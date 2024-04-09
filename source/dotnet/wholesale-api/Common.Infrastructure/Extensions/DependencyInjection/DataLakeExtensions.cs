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
using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks.DataLake;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// that allow adding DataLake services to an application.
/// </summary>
public static class DataLakeExtensions
{
    /// <summary>
    /// Register DataLake services commonly used by DH3 applications.
    /// </summary>
    public static IServiceCollection AddDataLakeClientForApplication(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<DataLakeOptions>().Bind(configuration);
        var options = configuration.Get<DataLakeOptions>()!;
        services.AddSingleton<DataLakeFileSystemClient>(_ =>
        {
            var dataLakeServiceClient = new DataLakeServiceClient(new Uri(options.STORAGE_ACCOUNT_URI), new DefaultAzureCredential());
            return dataLakeServiceClient.GetFileSystemClient(options.STORAGE_CONTAINER_NAME);
        });

        // Health checks
        services.AddHealthChecks()
            .AddDataLakeHealthCheck(
                _ => configuration.Get<DataLakeOptions>()!,
                name: "DataLake");

        return services;
    }
}
