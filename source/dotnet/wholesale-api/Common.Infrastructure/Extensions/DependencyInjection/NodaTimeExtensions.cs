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

using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// that allow adding NodaTime services to an application.
/// </summary>
public static class NodaTimeExtensions
{
    /// <summary>
    /// Register NodaTime services commonly used by DH3 applications.
    /// </summary>
    public static IServiceCollection AddNodaTimeForApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<IClock>(_ => SystemClock.Instance);

        services.AddOptions<DateTimeOptions>().Bind(configuration);
        services.TryAddSingleton<DateTimeZone>(services =>
        {
            var options = services.GetRequiredService<IOptions<DateTimeOptions>>().Value;
            return DateTimeZoneProviders.Tzdb.GetZoneOrNull(options.TIME_ZONE)!;
        });

        return services;
    }
}
