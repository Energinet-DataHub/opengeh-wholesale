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

using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.Builder;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Security;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Telemetry;
using Energinet.DataHub.Wholesale.Edi.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Orchestrations.Extensions;
using Energinet.DataHub.Wholesale.Orchestrations.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Orchestrations.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker =>
    {
        // Http => Authentication
        worker.UseUserMiddlewareForIsolatedWorker<FrontendUser>();
    })
    .ConfigureServices((context, services) =>
    {
        // Common
        services.AddApplicationInsightsForIsolatedWorker(TelemetryConstants.SubsystemName);
        services.AddHealthChecksForIsolatedWorker();

        // Http => Authentication
        services.AddUserAuthenticationForIsolatedWorker<FrontendUser, FrontendUserProvider>();

        // Shared by modules
        services.AddNodaTimeForApplication();
        services.AddDatabricksJobsForApplication(context.Configuration);
        services
            .AddOptions<CalculationJobStatusMonitorOptions>()
            .BindConfiguration(CalculationJobStatusMonitorOptions.SectionName);

        // Handle Wholesale inbox messages
        services.AddWholesaleInboxHandling(context.Configuration);

        // Modules
        services.AddCalculationsModule(context.Configuration);
        services.AddCalculationResultsV2Module(context.Configuration);

        // => Sub-modules of Events
        services.AddEventsDatabase(context.Configuration);
        services.AddIntegrationEventsPublishing(context.Configuration);
        services.AddCompletedCalculationsHandling();
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddLoggingConfigurationForIsolatedWorker(hostingContext);
    })
    .Build();

host.Run();
