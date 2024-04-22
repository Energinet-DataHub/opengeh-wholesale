﻿// Copyright 2020 Energinet DataHub A/S
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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Subscriber;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.Events.Application.Workers;

public class ReceiveIntegrationEventServiceBusWorker : ServiceBusWorker
{
    private readonly IServiceProvider _serviceProvider;

    public ReceiveIntegrationEventServiceBusWorker(
        ILogger<ReceiveIntegrationEventServiceBusWorker> logger,
        IOptions<IntegrationEventsOptions> options,
        ServiceBusClient serviceBusClient,
        IServiceProvider serviceProvider)
        : base(
            serviceBusClient.CreateProcessor(
                options.Value.TopicName,
                options.Value.SubscriptionName),
            logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ProcessAsync(ProcessMessageEventArgs arg)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ISubscriber>();

        var integrationEventMessage = IntegrationEventServiceBusMessage.Create(arg.Message);

        await handler.HandleAsync(integrationEventMessage).ConfigureAwait(true);
    }
}
