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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.Events.Application.Workers;

/// <summary>
/// Receives messages from the WholesaleInbox service bus queue (typically sent from the EDI subsystem)
/// </summary>
public class WholesaleInboxServiceBusWorker : ServiceBusWorker
{
    private readonly IServiceProvider _serviceProvider;

    public WholesaleInboxServiceBusWorker(
        IServiceProvider serviceProvider,
        ILogger<WholesaleInboxServiceBusWorker> logger,
        IOptions<WholesaleInboxQueueOptions> options,
        ServiceBusClient serviceBusClient)
    : base(
        serviceBusClient.CreateProcessor(options.Value.QueueName),
        logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ProcessAsync(ProcessMessageEventArgs arg)
    {
        if (arg.Message.ApplicationProperties.TryGetValue("ReferenceId", out var referenceIdPropertyValue)
            && referenceIdPropertyValue is string referenceId)
        {
            Logger.LogInformation("Processing message with reference id {reference_id}.", referenceId);
            using var scope = _serviceProvider.CreateScope();
            var requestHandlers = scope.ServiceProvider.GetServices<IWholesaleInboxRequestHandler>();
            var requestHandler = requestHandlers.Single(h => h.CanHandle(arg.Message.Subject));
            await requestHandler.ProcessAsync(arg.Message, referenceId, arg.CancellationToken).ConfigureAwait(true);
        }
        else
        {
            Logger.LogError("Missing reference id for Service Bus Message.");
        }
    }
}
