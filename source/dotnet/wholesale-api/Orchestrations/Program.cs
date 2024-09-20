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

using Azure.Storage.Blobs;
using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.Builder;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Security;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Telemetry;
using Energinet.DataHub.Wholesale.Common.Interfaces.Security;
using Energinet.DataHub.Wholesale.Edi.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Orchestrations.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Orchestrations.Extensions.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        // Http => Authorization
        builder.UseFunctionsAuthorization();
        // Http => Authentication
        builder.UseUserMiddlewareForIsolatedWorker<FrontendUser>();
    })
    .ConfigureServices((context, services) =>
    {
        // Common
        services.AddApplicationInsightsForIsolatedWorker(TelemetryConstants.SubsystemName);
        services.AddHealthChecksForIsolatedWorker();

        // Http => Authentication
        services
            .AddJwtBearerAuthenticationForIsolatedWorker(context.Configuration)
            .AddUserAuthenticationForIsolatedWorker<FrontendUser, FrontendUserProvider>();

        // Shared by modules
        services.AddNodaTimeForApplication();
        services.AddServiceBusClientForApplication(context.Configuration);
        services
            .AddOptions<CalculationOrchestrationMonitorOptions>()
            .BindConfiguration(CalculationOrchestrationMonitorOptions.SectionName);

        // ServiceBus channels
        services.AddIntegrationEventsPublishing(context.Configuration);
        services
            .AddInboxSubscription()
            .AddCalculationOrchestrationInboxRequestHandler();
        // => Dead-letter logging
        services.AddDeadLetterHandlerForIsolatedWorker(context.Configuration);
        services
            .AddHealthChecks()
            .AddAzureBlobStorage(
                clientFactory: sp =>
                {
                    var options = sp.GetRequiredService<IOptions<BlobDeadLetterLoggerOptions>>();
                    var clientFactory = sp.GetRequiredService<IAzureClientFactory<BlobServiceClient>>();
                    return clientFactory.CreateClient(options.Value.ContainerName);
                },
                configureOptions: (_, _) => { }, // Necessary to hint compiler of which method overload to use
                name: "dead-letter-logging");

        // Calculation scheduler
        services.AddCalculationScheduler();

        services.AddOutboxProcessing();

        // Modules
        services.AddEdiModule(context.Configuration); // Edi module has Wholesale inbox handlers for requests from EDI; and a client to send messages to EDI inbox
        services.AddCalculationsModule(context.Configuration);
        services.AddCalculationEngineModule(context.Configuration);
        services.AddCalculationResultsModule(context.Configuration);
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddLoggingConfigurationForIsolatedWorker(hostingContext);
    })
    .Build();

host.Run();
