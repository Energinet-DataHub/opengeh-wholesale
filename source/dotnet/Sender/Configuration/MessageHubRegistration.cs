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

using Energinet.DataHub.MessageHub.Client;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Client.Peek;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Dequeue;
using Energinet.DataHub.MessageHub.Model.Peek;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.Sender.Configuration;

public static class MessageHubRegistration
{
    /// <summary>
    /// Post office provides a NuGet package to handle the configuration, but it's for SimpleInjector
    /// and thus not applicable in this function host. See also
    /// https://github.com/Energinet-DataHub/geh-post-office/blob/main/source/PostOffice.Communicator.SimpleInjector/source/PostOffice.Communicator.SimpleInjector/ServiceCollectionExtensions.cs
    /// </summary>
    public static void AddMessageHub(
        this IServiceCollection serviceCollection,
        string serviceBusConnectionString,
        MessageHubConfig messageHubConfig,
        string storageServiceConnectionString,
        StorageConfig storageConfig)
    {
        if (serviceCollection == null)
            throw new ArgumentNullException(nameof(serviceCollection));

        if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
            throw new ArgumentNullException(nameof(serviceBusConnectionString));

        if (messageHubConfig == null)
            throw new ArgumentNullException(nameof(messageHubConfig));

        if (string.IsNullOrWhiteSpace(storageServiceConnectionString))
            throw new ArgumentNullException(nameof(storageServiceConnectionString));

        if (storageConfig == null)
            throw new ArgumentNullException(nameof(storageConfig));

        serviceCollection.AddSingleton(_ => messageHubConfig);
        serviceCollection.AddSingleton(_ => storageConfig);
        serviceCollection.AddServiceBus(serviceBusConnectionString);
        serviceCollection.AddApplicationServices();
        serviceCollection.AddStorageHandler(storageServiceConnectionString);
    }

    private static void AddServiceBus(this IServiceCollection serviceCollection, string serviceBusConnectionString)
    {
        serviceCollection.AddSingleton<IServiceBusClientFactory>(_ =>
        {
            if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
            {
                throw new InvalidOperationException(
                    "Please specify a valid ServiceBus in the appSettings.json file or your Azure Functions Settings.");
            }

            return new ServiceBusClientFactory(serviceBusConnectionString);
        });

        serviceCollection.AddSingleton<IMessageBusFactory>(provider =>
        {
            var serviceBusClientFactory = provider.GetRequiredService<IServiceBusClientFactory>();
            return new AzureServiceBusFactory(serviceBusClientFactory);
        });
    }

    private static void AddApplicationServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDataAvailableNotificationSender, DataAvailableNotificationSender>();
        serviceCollection.AddSingleton<IRequestBundleParser, RequestBundleParser>();
        serviceCollection.AddSingleton<IResponseBundleParser, ResponseBundleParser>();
        serviceCollection.AddSingleton<IDataBundleResponseSender, DataBundleResponseSender>();
        serviceCollection.AddSingleton<IDequeueNotificationParser, DequeueNotificationParser>();
    }

    private static void AddStorageHandler(this IServiceCollection serviceCollection, string storageServiceConnectionString)
    {
        serviceCollection.AddSingleton<IStorageServiceClientFactory>(_ =>
        {
            if (string.IsNullOrWhiteSpace(storageServiceConnectionString))
            {
                throw new InvalidOperationException(
                    "Please specify a valid BlobStorageConnectionString in the appSettings.json file or your Azure Functions Settings.");
            }

            return new StorageServiceClientFactory(storageServiceConnectionString);
        });

        serviceCollection.AddSingleton<IStorageHandler, StorageHandler>();
    }
}
