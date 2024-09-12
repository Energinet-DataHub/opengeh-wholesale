using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Energinet.DataHub.HealthCheckApp.Monitor;
using Azure.Identity;

namespace Energinet.DataHub.HealthCheckApp.Extensions.DependencyInjections;

public static class HealthCheckAppExtensions
{
    public static IServiceCollection AddHealthCheckAppModule(this IServiceCollection services, HostBuilderContext context)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(context);

        var keyvaultname = Environment.GetEnvironmentVariable("SHARED_KEYVAULT_NAME");
        ArgumentNullException.ThrowIfNull(keyvaultname);
        var keyvaultUri = new Uri($"https://{keyvaultname}.vault.azure.net");

        var blobStorageName = Environment.GetEnvironmentVariable("SHARED_DATALAKE_NAME");
        ArgumentNullException.ThrowIfNull(blobStorageName);
        var blobStorageUri = new Uri($"https://{blobStorageName}.blob.core.windows.net");

        var servicebusEndpoint = Environment.GetEnvironmentVariable("SERVICEBUS_ENDPOINT");
        ArgumentNullException.ThrowIfNull(servicebusEndpoint);
        // format of URL is https://<namespace>.servicebus.windows.net:443/
        // It only accepts the format of <namespace>.servicebus.windows.net
        servicebusEndpoint = servicebusEndpoint.Replace("https://", "").Replace(":443/", "");

        var servicebusTopicName = Environment.GetEnvironmentVariable("SERVICEBUS_TOPIC_NAME");
        ArgumentNullException.ThrowIfNull(servicebusTopicName);


        services.AddScoped<HealthCheckEndpoint>();

        services.AddHealthChecks()
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
                    servicebusEndpoint,
                    servicebusTopicName,
                    new DefaultAzureCredential(),
                    name: $"Servicebus topic {servicebusTopicName} access");

        return services;
    }

}
