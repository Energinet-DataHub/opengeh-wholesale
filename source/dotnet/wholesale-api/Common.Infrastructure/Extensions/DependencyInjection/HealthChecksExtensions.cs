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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// that allow adding named HealthChecks services in an idempotent way.
/// </summary>
public static class HealthChecksExtensions
{
    /// <summary>
    /// This operation is idempotent with respect to the given <paramref name="registrationKey"/>.
    /// Multiple invocations using the same key will only result in a single call to the <paramref name="builderDelegate"/>.
    ///
    /// Verifies if the given registration key has already been used to add health checks.
    /// If that is the case the <paramref name="builderDelegate"/> is never called.
    /// Otherwise the <paramref name="builderDelegate"/> is called and health checks will
    /// be added in the usual fashion.
    /// </summary>
    public static IServiceCollection TryAddHealthChecks(
        this IServiceCollection services,
        string registrationKey,
        Action<string, IHealthChecksBuilder> builderDelegate)
    {
        ArgumentNullException.ThrowIfNull(registrationKey);
        ArgumentNullException.ThrowIfNull(builderDelegate);

        if (!IsHealthCheckAdded(services, registrationKey))
        {
            services.AddSingleton(new HealthCheckRegistrationGuard(registrationKey));
            builderDelegate(registrationKey, services.AddHealthChecks());
        }

        return services;
    }

    /// <summary>
    /// Determines if a <see cref="HealthCheckRegistrationGuard"/> has been registered using the given key.
    /// </summary>
    private static bool IsHealthCheckAdded(IServiceCollection services, string registrationKey)
    {
        return services.Any(
            service =>
                service.ServiceType == typeof(HealthCheckRegistrationGuard)
                && service.ImplementationInstance is HealthCheckRegistrationGuard registrationGuard
                && registrationGuard.Key == registrationKey);
    }

    /// <summary>
    /// Used for adding to the service collection to indicate that a
    /// health check registration using the given key has been performed.
    /// </summary>
    private sealed record HealthCheckRegistrationGuard(string Key);
}
