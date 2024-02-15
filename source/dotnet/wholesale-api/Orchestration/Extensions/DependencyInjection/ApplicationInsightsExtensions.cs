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
using Energinet.DataHub.Core.App.Common.Reflection;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Telemetry;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.Orchestration.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>
    /// that allow adding Application Insights services to Function App's.
    /// </summary>
    public static class ApplicationInsightsExtensions
    {
        /// <summary>
        /// Register services necessary for enabling an Azure Function App (isolated worker model)
        /// to log telemetry to Application Insights.
        /// Configuration of telemetry (initializers, properties etc.) within the isolated worker
        /// only affects logs emitted from the isolated worker and not those emitted from the host.
        /// </summary>
        public static IServiceCollection AddApplicationInsightsForIsolatedWorker(this IServiceCollection services)
        {
            // Telemetry initializers only adds information to logs emitted by the isolated worker; not logs emitted by the function host.
            services.AddSingleton<ITelemetryInitializer>(new SubsystemInitializer(TelemetryConstants.SubsystemName));

            // Configure isolated worker to emit logs directly to Application Insights.
            // See https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#application-insights
            services.AddApplicationInsightsTelemetryWorkerService(options =>
            {
                options.ApplicationVersion = Assembly
                    .GetEntryAssembly()!
                    .GetAssemblyInformationalVersionAttribute()!
                    .GetSourceVersionInformation()
                    .ToString();
            });
            services.ConfigureFunctionsApplicationInsights();

            return services;
        }
    }
}
