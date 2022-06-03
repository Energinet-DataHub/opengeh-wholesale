using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Core.Messaging.Transport;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Infrastructure.Core.MessagingExtensions.Registration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Wholesale.IntegrationEventListener
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(ConfigureServices)
                .Build();

            host.Run();
        }

        private static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IJsonSerializer, JsonSerializer>();
            serviceCollection.AddScoped<MessageExtractor>();
            serviceCollection.ConfigureProtobufReception();
            serviceCollection.ReceiveProtobufMessage<MeteringPointCreated>(
                configuration => configuration.WithParser(() => MeteringPointCreated.Parser));
        }
    }
}