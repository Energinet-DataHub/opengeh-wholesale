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
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Application.MeteringPoints;
using Energinet.DataHub.Wholesale.Infrastructure.Core;
using Energinet.DataHub.Wholesale.IntegrationEventListener.Common;
using Energinet.DataHub.Wholesale.IntegrationEventListener.Contracts.External.MeteringPointCreated;
using Energinet.DataHub.Wholesale.IntegrationEventListener.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Wholesale.IntegrationEventListener
{
    public class Program
    {
        public static async Task Main()
        {
            using var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults(builder =>
                {
                    builder.UseMiddleware<IntegrationEventMetadataMiddleware>();
                })
                .ConfigureServices(ConfigureServices)
                .Build();

            await host.RunAsync().ConfigureAwait(false);
        }

        private static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging();
            serviceCollection.AddApplicationInsightsTelemetryWorkerService(
                EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.AppInsightsInstrumentationKey));

            serviceCollection.AddScoped<IIntegrationEventContext, IntegrationEventContext>();
            serviceCollection.AddScoped<IntegrationEventMetadataMiddleware>();

            serviceCollection.AddSingleton<IJsonSerializer, JsonSerializer>();
            serviceCollection.AddSingleton<MeteringPointCreatedInboundMapper>();

            serviceCollection.AddScoped<IMeteringPointCreatedDtoFactory, MeteringPointCreatedDtoFactory>();
        }
    }
}
