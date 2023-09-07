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

using Azure.Storage.Files.DataLake;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Wholesale.WebApi.HealthChecks.DataLake;

public static class DataLakeHealthCheckBuilderExtensions
{
    private const string Name = "DataLakeHealthCheck";

    public static IHealthChecksBuilder AddDataLakeHealthCheck(
        this IHealthChecksBuilder builder,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? Name,
            serviceProvider => new DataLakeHealthRegistration(serviceProvider.GetRequiredService<DataLakeFileSystemClient>()),
            failureStatus,
            tags,
            timeout));
    }
}
