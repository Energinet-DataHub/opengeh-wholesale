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
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.Options;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Energinet.DataHub.Wholesale.Orchestrations.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions;

internal class WholesaleInboxTrigger(ILogger<WholesaleInboxTrigger> logger, WholesaleInboxHandler wholesaleInboxHandler)
{
    [Function(nameof(WholesaleInboxTrigger))]
    public async Task ReceiveWholesaleInboxMessageAsync(
        [ServiceBusTrigger(
            $"%{WholesaleInboxQueueOptions.SectionName}:{nameof(WholesaleInboxQueueOptions.QueueName)}%",
            Connection = $"{ServiceBusNamespaceOptions.SectionName}:{nameof(ServiceBusNamespaceOptions.ConnectionString)}")]
        ServiceBusReceivedMessage inboxMessage,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Started receiving Wholesale inbox message (Subject: \"{Subject}\", MessageId: \"{EventId}\") from queue",
            inboxMessage.Subject,
            inboxMessage.MessageId);

        await wholesaleInboxHandler.ProcessAsync(inboxMessage, cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation(
            "Finished receiving Wholesale inbox message (Subject: \"{Subject}\", MessageId: \"{EventId}\") from queue",
            inboxMessage.Subject,
            inboxMessage.MessageId);
    }
}
