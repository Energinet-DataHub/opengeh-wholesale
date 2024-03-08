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

using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Wholesale.WebApi.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// that allow adding HealthChecks services to an ASP.NET Core app.
/// </summary>
public static class HealthChecksExtensions
{
    /// <summary>
    /// Register services necessary for using Health Checks in an ASP.NET Core app.
    /// </summary>
    public static IServiceCollection AddHealthChecksForWebApp(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddLiveCheck();

        return services;
    }
}
