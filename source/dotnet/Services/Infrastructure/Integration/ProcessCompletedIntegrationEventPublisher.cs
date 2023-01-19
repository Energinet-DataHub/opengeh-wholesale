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
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using Google.Protobuf;

namespace Energinet.DataHub.Wholesale.Infrastructure.Integration;

public class ProcessCompletedIntegrationEventPublisher : IProcessCompletedIntegrationEventPublisher
{
    private readonly ServiceBusSender _serviceBusSender;
    private readonly IServiceBusMessageFactory _serviceBusMessageFactory;
    private readonly IProcessCompletedIntegrationEventMapper _processCompletedIntegrationEventMapper;

    public ProcessCompletedIntegrationEventPublisher(
        ServiceBusSender serviceBusSender,
        IServiceBusMessageFactory serviceBusMessageFactory,
        IProcessCompletedIntegrationEventMapper processCompletedIntegrationEventMapper)
    {
        _serviceBusSender = serviceBusSender;
        _serviceBusMessageFactory = serviceBusMessageFactory;
        _processCompletedIntegrationEventMapper = processCompletedIntegrationEventMapper;
    }

    public async Task PublishAsync(ProcessCompletedEventDto processCompletedEvent)
    {
        var integrationEvent = _processCompletedIntegrationEventMapper.MapFrom(processCompletedEvent);
        var messageType = GetMessageType(processCompletedEvent.ProcessType);
        var message = _serviceBusMessageFactory.Create(integrationEvent.ToByteArray(), messageType);
        await _serviceBusSender.SendMessageAsync(message, CancellationToken.None).ConfigureAwait(false);
    }

    private string GetMessageType(ProcessType processType) =>
        processType switch
        {
            ProcessType.BalanceFixing => ProcessCompleted.BalanceFixingProcessType,
            _ => throw new NotImplementedException($"Process type '{processType}' not implemented"),
        };
}
