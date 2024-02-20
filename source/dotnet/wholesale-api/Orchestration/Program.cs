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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks;
using Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks.ServiceBus;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Orchestration.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Common
        services.AddApplicationInsightsForIsolatedWorker();
        services.AddHealthChecksForIsolatedWorker();

        // Shared by modules
        services.AddNodaTimeForApplication(context.Configuration);
        services.AddDatabricksJobsForApplication(context.Configuration);

        // Modules
        services.AddCalculationsModule(context.Configuration);
        services.AddCalculationResultsModule(context.Configuration);
        // => Sub-modules of Events
        services.AddEventsDatabase(context.Configuration);
        services.AddIntegrationEventPublishing(context.Configuration);

        var serviceBusOptions = context.Configuration.Get<ServiceBusOptions>()!;
        services.AddHealthChecks()
            .AddAzureServiceBusSubscriptionUsingWebSockets(
                serviceBusOptions.SERVICE_BUS_TRANCEIVER_CONNECTION_STRING,
                serviceBusOptions.INTEGRATIONEVENTS_TOPIC_NAME,
                serviceBusOptions.INTEGRATIONEVENTS_SUBSCRIPTION_NAME,
                name: HealthCheckNames.IntegrationEventsTopicSubscription);
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddLoggingConfigurationForIsolatedWorker(hostingContext);
    })
    .Build();

host.Run();
