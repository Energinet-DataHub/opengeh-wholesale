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

using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Apps.Core;
using Energinet.DataHub.Wholesale.Apps.Core.Configuration;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.ProcessManager.Monitor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HostSettings = Energinet.DataHub.Wholesale.ProcessManager.Configuration.Settings;

namespace Energinet.DataHub.Wholesale.ProcessManager;

public sealed class Startup : StartupBase
{
    protected override void Configure(IConfiguration configuration, IServiceCollection services)
    {
        Infrastructure(services);
        HealthCheck(configuration, services);
    }

    private static void Infrastructure(IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.AddSingleton<IJsonSerializer, JsonSerializer>();
    }

    private static void HealthCheck(IConfiguration configuration, IServiceCollection services)
    {
        services.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
        services.AddScoped<HealthCheckEndpoint>();

        var serviceBusConnectionString = configuration.GetSetting(HostSettings.ServiceBusManageConnectionString);
        var processCompletedTopicName = configuration.GetSetting(Settings.ProcessCompletedTopicName);

        services
            .AddHealthChecks()
            .AddLiveCheck()
            .AddDbContextCheck<DatabaseContext>(name: "SqlDatabaseContextCheck")
            .AddAzureServiceBusTopic(
                serviceBusConnectionString,
                processCompletedTopicName,
                name: "ProcessCompletedTopicExists");
    }
}
