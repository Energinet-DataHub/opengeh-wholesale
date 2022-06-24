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

using Energinet.DataHub.Core.App.Common.Abstractions.IntegrationEventContext;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.Apps.Core;

public abstract class StartupBase
{
    public void Initialize(IConfiguration configuration, IServiceCollection services)
    {
        RequiredMiddleware(services);

        services.AddDbContexts(configuration);
        services.AddRepositories();
        services.AddApplicationServices();
        services.AddDomainServices();

        Configure(configuration, services);
    }

    protected abstract void Configure(IConfiguration configuration, IServiceCollection services);

    private static void RequiredMiddleware(IServiceCollection services)
    {
        services.AddScoped<ICorrelationContext, CorrelationContext>();
        services.AddScoped<CorrelationIdMiddleware>();
        services.AddScoped<FunctionTelemetryScopeMiddleware>();
        services.AddScoped<IIntegrationEventContext, IntegrationEventContext>();
        services.AddScoped<IntegrationEventMetadataMiddleware>();
    }
}
