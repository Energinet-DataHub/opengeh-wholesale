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

using System.Collections.ObjectModel;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Subscriber;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Energinet.DataHub.Wholesale.Events.Application.Workers;

public class ReceiveIntegrationEventServiceBusWorker : ServiceBusWorker<ReceiveIntegrationEventServiceBusWorker>
{
    private readonly IServiceProvider _serviceProvider;

    public ReceiveIntegrationEventServiceBusWorker(
        ILogger<ReceiveIntegrationEventServiceBusWorker> logger,
        IOptions<ServiceBusOptions> options,
        ServiceBusClient serviceBusClient,
        IServiceProvider serviceProvider)
        : base(
            logger,
            serviceBusClient.CreateProcessor(
                options.Value.INTEGRATIONEVENTS_TOPIC_NAME,
                options.Value.INTEGRATIONEVENTS_SUBSCRIPTION_NAME))
    {
        _serviceProvider = serviceProvider;
    }

    protected override Task ProcessAsync(ProcessMessageEventArgs arg, string referenceId)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ISubscriber>();

        var servicebusMessageBytes = arg.Message.Body.ToArray();
        var bindingData = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
        {
            { "MessageId", arg.Message.MessageId },
            { "Subject", arg.Message.Subject },
            { "ApplicationProperties", JsonConvert.ToString(arg.Message.ApplicationProperties) },
        });

        var integrationEventMessage = IntegrationEventServiceBusMessage.Create(servicebusMessageBytes, bindingData);

        return handler.HandleAsync(integrationEventMessage);
    }
}
