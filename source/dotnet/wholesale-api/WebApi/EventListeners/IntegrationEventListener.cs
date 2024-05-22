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

using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Subscriber;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;

namespace Energinet.DataHub.Wholesale.WebApi.EventListeners;

public class IntegrationEventListener(ILogger<IntegrationEventListener> logger, ISubscriber subscriber)
{
    private readonly ILogger<IntegrationEventListener> _logger = logger;
    private readonly ISubscriber _subscriber = subscriber;

    [Function(nameof(IntegrationEventListener))]
    public async Task RunAsync(
        [ServiceBusTrigger(
            $"%{IntegrationEventsOptions.SectionName}:{nameof(IntegrationEventsOptions.TopicName)}%",
            $"%{IntegrationEventsOptions.SectionName}:{nameof(IntegrationEventsOptions.SubscriptionName)}%",
            Connection = $"{ServiceBusNamespaceOptions.SectionName}:{nameof(ServiceBusNamespaceOptions.ConnectionString)}")]
        byte[] eventData,
        FunctionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        await _subscriber
            .HandleAsync(IntegrationEventServiceBusMessage.Create(eventData, context.BindingContext.BindingData!))
            .ConfigureAwait(false);
    }
}
