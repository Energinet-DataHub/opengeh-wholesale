using Microsoft.Extensions.Hosting;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Azure.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.SqlServer;
using Microsoft.Data.SqlClient;

var startup = new Startup();
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        startup.Initialize(context.Configuration, services);

        var keyvaultname = Environment.GetEnvironmentVariable("SHARED_KEYVAULT_NAME");
        var keyvaultUri = new Uri($"https://{keyvaultname}.vault.azure.net");

        var blobStorageName = Environment.GetEnvironmentVariable("SHARED_DATALAKE_NAME");
        var blobStorageUri = new Uri($"https://{blobStorageName}.blob.core.windows.net");

        var servicebusConnectionString = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTION_STRING");
        var servicebusTopicName = Environment.GetEnvironmentVariable("SERVICEBUS_TOPIC_NAME");

        services.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
        services.AddHealthChecks()
             .AddLiveCheck()
                .AddAzureKeyVault(
                    keyvaultUri,
                    new DefaultAzureCredential(),
                    options =>
                    {
                        options.AddSecret("b2c-tenant-id");
                    },
                    name: $"Keyvault {keyvaultname} b2c-tenant-id access")
                .AddAzureBlobStorage(
                    blobStorageUri,
                    new DefaultAzureCredential(),
                    name: $"Storage account {blobStorageName} access")
                .AddAzureServiceBusTopic(
                    servicebusConnectionString,
                    servicebusTopicName,
                    name: $"Servicebus topic {servicebusTopicName} access");


    }).Build();

await host.RunAsync().ConfigureAwait(false);
