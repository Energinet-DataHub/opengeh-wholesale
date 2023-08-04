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

using Energinet.DataHub.Core.Messaging.Communication.Internal;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Wholesale.WebApi.HealthChecks;

public class HostedServicesHealthCheck : IHealthCheck
{
    private readonly IHostedServiceReadinessMonitor _hostedServiceReadinessMonitor;

    public HostedServicesHealthCheck(IHostedServiceReadinessMonitor hostedServiceReadinessMonitor)
    {
        _hostedServiceReadinessMonitor = hostedServiceReadinessMonitor;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // TODO BJM: Must check all hosted services + in case of unhealthy, return a list of unhealthy services
        var isHealthy = _hostedServiceReadinessMonitor.IsReady<OutboxSenderTrigger>();

        if (!isHealthy)
            return Task.FromResult(HealthCheckResult.Unhealthy("Hosted services health check failed."));

        return Task.FromResult(HealthCheckResult.Healthy("Hosted services health check is healthy."));
    }
}
