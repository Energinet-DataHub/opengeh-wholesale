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
using Energinet.DataHub.Wholesale.Common.DatabricksClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NodaTime;

namespace Energinet.DataHub.Wholesale.WebApi.HealthChecks.Databricks;

public class DatabricksJobsApiHealthRegistration : IHealthCheck
{
    private readonly IJobsApiClient _jobsApiClient;
    private readonly IClock _clock;
    private readonly DatabricksOptions _options;

    public DatabricksJobsApiHealthRegistration(IJobsApiClient jobsApiClient, IClock clock, DatabricksOptions options)
    {
        _jobsApiClient = jobsApiClient;
        _clock = clock;
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        var currentHour = _clock.GetCurrentInstant().ToDateTimeUtc().Hour;
        if (_options.DATABRICKS_HEALTH_CHECK_START_HOUR.Hour <= currentHour && currentHour <= _options.DATABRICKS_HEALTH_CHECK_END_HOUR.Hour)
        {
            await _jobsApiClient.Jobs.List(1, 0, null, false, cancellationToken).ConfigureAwait(false);
        }

        return HealthCheckResult.Healthy();
    }
}
