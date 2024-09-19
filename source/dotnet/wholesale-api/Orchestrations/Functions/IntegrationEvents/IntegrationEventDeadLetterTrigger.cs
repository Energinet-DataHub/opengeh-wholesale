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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Energinet.DataHub.Core.Messaging.Communication.Subscriber;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.IntegrationEvents;

internal class IntegrationEventDeadLetterTrigger(
    IDeadLetterHandler deadLetterHandler)
{
    private readonly IDeadLetterHandler _deadLetterHandler = deadLetterHandler;

    /// <summary>
    /// Receives messages from the dead-letter queue of the integration event topic/subscription and processes them.
    /// </summary>
    /// <remarks>
    /// The dead-letter handler is responsible for managing the message, which is why 'AutoCompleteMessages' must be set 'false'.
    /// </remarks>
    [Function(nameof(IntegrationEventDeadLetterTrigger))]
    public async Task RunAsync(
        [ServiceBusTrigger(
            $"%{IntegrationEventsOptions.SectionName}:{nameof(IntegrationEventsOptions.TopicName)}%",
            $"%{IntegrationEventsOptions.SectionName}:{nameof(IntegrationEventsOptions.SubscriptionName)}%{DeadLetterConstants.DeadLetterQueueSuffix}",
            Connection = ServiceBusNamespaceOptions.SectionName,
            AutoCompleteMessages = false)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        await _deadLetterHandler
            .HandleAsync(deadLetterSource: "integration-events", message, messageActions)
            .ConfigureAwait(false);
    }
}
