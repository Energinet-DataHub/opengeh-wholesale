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
using NodaTime;

namespace Energinet.DataHub.Wholesale.WebApi.HealthChecks.Databricks;

public class DatabricksSqlStatementsApiHealthRegistration : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IClock _clock;
    private readonly DatabricksOptions _options;

    public DatabricksSqlStatementsApiHealthRegistration(IHttpClientFactory httpClientFactory, IClock clock, DatabricksOptions options)
    {
        _httpClientFactory = httpClientFactory;
        _clock = clock;
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        var currentHour = _clock.GetCurrentInstant().ToDateTimeUtc().Hour;
        if (_options.DATABRICKS_HEALTH_CHECK_START_HOUR.Hour <= currentHour && currentHour <= _options.DATABRICKS_HEALTH_CHECK_END_HOUR.Hour)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_options.DATABRICKS_WORKSPACE_URL);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.DATABRICKS_WORKSPACE_TOKEN);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.BaseAddress = new Uri(_options.DATABRICKS_WORKSPACE_URL);
            var url = $"{_options.DATABRICKS_WORKSPACE_URL}/api/2.0/sql/warehouses/{_options.DATABRICKS_WAREHOUSE_ID}";
            var response = await httpClient
                .GetAsync(url, cancellationToken)
                .ConfigureAwait(false);

            return response.IsSuccessStatusCode ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        }

        return HealthCheckResult.Healthy();
    }
}
