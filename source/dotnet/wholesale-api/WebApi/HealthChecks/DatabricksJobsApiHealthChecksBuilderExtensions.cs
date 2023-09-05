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

using Energinet.DataHub.Wholesale.Common.Databricks;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.WebApi.HealthChecks
{
    public static class DatabricksJobsApiHealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder AddDatabricksJobsApiCheck(this IHealthChecksBuilder builder, DatabricksOptions options, string name)
        {
            return builder.AddAsyncCheck(name, async () =>
            {
                try
                {
                    var client = new JobsApiClient(Options.Create(options));
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
