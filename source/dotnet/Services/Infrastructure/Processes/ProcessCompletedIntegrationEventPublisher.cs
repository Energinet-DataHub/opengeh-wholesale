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
using Energinet.DataHub.Wholesale.Application.Infrastructure;
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Infrastructure.Integration;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;

namespace Energinet.DataHub.Wholesale.Infrastructure.Processes;

public class ProcessCompletedIntegrationEventPublisher : IProcessCompletedIntegrationEventPublisher
{
    private readonly ServiceBusSender _serviceBusSender;
    private readonly IServiceBusMessageFactory _serviceBusMessageFactory;

    public ProcessCompletedIntegrationEventPublisher(
        ServiceBusSender serviceBusSender,
        IServiceBusMessageFactory serviceBusMessageFactory)
    {
        _serviceBusSender = serviceBusSender;
        _serviceBusMessageFactory = serviceBusMessageFactory;
    }

    public async Task PublishAsync(ProcessCompletedEventDto processCompletedEvent)
    {
        var integrationEvent = Map(processCompletedEvent);
        var message = _serviceBusMessageFactory.Create(integrationEvent);
        await _serviceBusSender.SendMessageAsync(message, CancellationToken.None).ConfigureAwait(false);
    }

    private static ProcessCompleted Map(ProcessCompletedEventDto processCompletedEvent)
    {
        return new ProcessCompleted
        {
            BatchId = processCompletedEvent.BatchId.ToString(),
            ProcessType = ProcessCompleted.Types.ProcessType.PtBalancefixing, // Will be made dynamic in upcoming PR
            GridAreaCode = processCompletedEvent.GridAreaCode,
        };
    }
}
