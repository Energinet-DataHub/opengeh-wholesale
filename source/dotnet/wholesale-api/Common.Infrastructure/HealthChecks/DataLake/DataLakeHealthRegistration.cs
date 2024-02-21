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

using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks.DataLake;

public class DataLakeHealthRegistration : IHealthCheck
{
    private readonly DataLakeFileSystemClient _dataLakeFileSystemClient;
    private readonly IClock _clock;
    private readonly DataLakeOptions _options;

    public DataLakeHealthRegistration(DataLakeFileSystemClient dataLakeFileSystemClient, IClock clock, DataLakeOptions options)
    {
        _dataLakeFileSystemClient = dataLakeFileSystemClient;
        _clock = clock;
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        var currentHour = _clock.GetCurrentInstant().ToDateTimeUtc().Hour;
        if (_options.DATALAKE_HEALTH_CHECK_START.Hour <= currentHour && currentHour <= _options.DATALAKE_HEALTH_CHECK_END.Hour)
        {
            return await _dataLakeFileSystemClient.ExistsAsync(cancellationToken).ConfigureAwait(false)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy();
        }

        return HealthCheckResult.Healthy();
    }
}
