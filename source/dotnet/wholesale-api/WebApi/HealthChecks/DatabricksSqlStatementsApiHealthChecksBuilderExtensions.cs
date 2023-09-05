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

using System.Net.Http.Headers;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Wholesale.WebApi.HealthChecks
{
    public static class DatabricksSqlStatementsApiHealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder AddDatabricksSqlStatementsApiCheck(this IHealthChecksBuilder builder, DatabricksOptions options, string name)
        {
            return builder.AddAsyncCheck(name, async () =>
            {
                try
                {
                    using var httpClient = new HttpClient();
                    httpClient.BaseAddress = new Uri(options.DATABRICKS_WORKSPACE_URL);
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.DATABRICKS_WORKSPACE_TOKEN);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.BaseAddress = new Uri(options.DATABRICKS_WORKSPACE_URL);
                    var url = $"{options.DATABRICKS_WORKSPACE_URL}/api/2.0/sql/warehouses/{options.DATABRICKS_WAREHOUSE_ID}";
                    var response = await httpClient
                        .GetAsync(url)
                        .ConfigureAwait(false);

                    return response.IsSuccessStatusCode ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
                }
                catch (Exception)
                {
                    return HealthCheckResult.Unhealthy();
                }
            });
        }
    }
}
