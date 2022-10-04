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

using Microsoft.Azure.Databricks.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Wholesale.ProcessManager.Monitor
{
    public static class DatabricksHealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder AddDatabricksCheck(this IHealthChecksBuilder builder, string url, string token)
        {
            return builder.AddAsyncCheck("Databricks", async () =>
            {
                var client = DatabricksClient.CreateClient(url, token);
                try
                {
                    await client.Jobs.List().ConfigureAwait(false);
                    return HealthCheckResult.Healthy();
                }
                catch (Exception)
                {
                    return HealthCheckResult.Unhealthy();
                }
            });
        }
    }
}
