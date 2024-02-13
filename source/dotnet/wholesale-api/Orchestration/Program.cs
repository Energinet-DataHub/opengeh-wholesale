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

using System.Reflection;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.Common.Reflection;
using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Telemetry;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Application Insights (Telemetry)
        //  - Telemetry initializers only add information to logs emitted by the isolated worker; not the function host.
        services.AddSingleton<ITelemetryInitializer>(new SubsystemInitializer(TelemetryConstants.SubsystemName));
        services.AddApplicationInsightsTelemetryWorkerService(options =>
        {
            options.ApplicationVersion = Assembly
                .GetEntryAssembly()!
                .GetAssemblyInformationalVersionAttribute()!
                .GetSourceVersionInformation()
                .ToString();
        });
        services.ConfigureFunctionsApplicationInsights();

        // Health check
        services.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
        services.AddHealthChecks()
            .AddLiveCheck();
    })
    .Build();

host.Run();
