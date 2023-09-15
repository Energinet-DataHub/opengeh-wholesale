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

using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Wholesale.WebApi.HealthChecks.Databricks;

public static class DatabricksSqlStatementsApiHealthCheckBuilderExtensions
{
    private const string Name = "DatabricksSqlStatementsApiHealthCheck";

    public static IHealthChecksBuilder AddDatabricksSqlStatementsApiHealthCheck(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, DatabricksOptions> options,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? Name,
            serviceProvider => new DatabricksSqlStatementsApiHealthRegistration(
                serviceProvider.GetRequiredService<IHttpClientFactory>(),
                options(serviceProvider)),
            failureStatus,
            tags,
            timeout));
    }
}
