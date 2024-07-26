using Energinet.DataHub.Core.App.FunctionApp.Extensions.Builder;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Energinet.DataHub.HealthCheckApp.Extensions.DependencyInjections;

// Create and configure the host
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Configure health checks
        services.AddHealthChecksForIsolatedWorker();
        services.AddHealthCheckAppModule(context);
    })
    .Build();

// Run the host
host.Run();
