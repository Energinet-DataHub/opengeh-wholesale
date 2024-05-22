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
using Energinet.DataHub.Wholesale.Edi;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;

namespace Energinet.DataHub.Wholesale.WebApi.EventListeners;

public class InboxEventListener(
    IReadOnlyCollection<IWholesaleInboxRequestHandler> wholesaleInboxRequestHandlers,
    ILogger<InboxEventListener> logger)
{
    private readonly IReadOnlyCollection<IWholesaleInboxRequestHandler> _wholesaleInboxRequestHandlers = wholesaleInboxRequestHandlers;
    private readonly ILogger<InboxEventListener> _logger = logger;

    [Function(nameof(InboxEventListener))]
    public async Task RunAsync(
        [ServiceBusTrigger(
            $"%{WholesaleInboxQueueOptions.SectionName}:{nameof(WholesaleInboxQueueOptions.QueueName)}%",
            Connection = $"{ServiceBusNamespaceOptions.SectionName}:{nameof(ServiceBusNamespaceOptions.ConnectionString)}")]
        ServiceBusReceivedMessage message,
        FunctionContext context,
        CancellationToken hostCancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.BindingContext.BindingData.TryGetValue("ReferenceId", out var referenceIdPropertyValue)
            && referenceIdPropertyValue is string referenceId
            && context.BindingContext.BindingData.TryGetValue("Subject", out var subjectPropertyValue)
            && subjectPropertyValue is string subject)
        {
            _logger.LogInformation("Processing message with reference id {reference_id}.", referenceId);
            var requestHandler = _wholesaleInboxRequestHandlers.Single(h => h.CanHandle(subject));
            await requestHandler.ProcessAsync(message, referenceId, hostCancellationToken).ConfigureAwait(true);
        }
        else
        {
            _logger.LogError("Missing reference id for Service Bus Message.");
        }
    }
}
