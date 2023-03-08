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

using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Integration;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using Google.Protobuf;
using ProcessType = Energinet.DataHub.Wholesale.Contracts.ProcessType;

namespace Energinet.DataHub.Wholesale.Infrastructure.EventPublishers;

public class IntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IIntegrationEventTopicServiceBusSender _serviceBusSender;
    private readonly IServiceBusMessageFactory _serviceBusMessageFactory;
    private readonly IProcessCompletedIntegrationEventMapper _processCompletedIntegrationEventMapper;
    private readonly ICalculationResultReadyIntegrationEventMapper _calculationResultReadyIntegrationEventMapper;

    public IntegrationEventPublisher(
        IIntegrationEventTopicServiceBusSender serviceBusSender,
        IServiceBusMessageFactory serviceBusMessageFactory,
        IProcessCompletedIntegrationEventMapper processCompletedIntegrationEventMapper,
        ICalculationResultReadyIntegrationEventMapper calculationResultReadyIntegrationEventMapper)
    {
        _serviceBusSender = serviceBusSender;
        _serviceBusMessageFactory = serviceBusMessageFactory;
        _processCompletedIntegrationEventMapper = processCompletedIntegrationEventMapper;
        _calculationResultReadyIntegrationEventMapper = calculationResultReadyIntegrationEventMapper;
    }

    public async Task PublishAsync(ProcessCompletedEventDto processCompletedEvent)
    {
        var integrationEvent = _processCompletedIntegrationEventMapper.MapFrom(processCompletedEvent);
        var messageType = GetMessageType(processCompletedEvent.ProcessType);
        var message = _serviceBusMessageFactory.CreateProcessCompleted(integrationEvent.ToByteArray(), messageType);
        await _serviceBusSender.SendMessageAsync(message, CancellationToken.None).ConfigureAwait(false);
    }

    public Task PublishAsync(ProcessStepResult processStepResultDto, ProcessCompletedEventDto processCompletedEventDto)
    {
        throw new NotImplementedException();
        // var integrationEvent =
        //     _calculationResultReadyIntegrationEventMapper.MapFrom(processStepResultDto, processCompletedEventDto);
        // var messageType = "CalculationResultReady";
        // var message = _serviceBusMessageFactory.CreateProcessCompleted(integrationEvent.ToByteArray(), messageType);
        // await _serviceBusSender.SendMessageAsync(message, CancellationToken.None).ConfigureAwait(false);
    }

    private string GetMessageType(ProcessType processType) =>
        processType switch
        {
            ProcessType.BalanceFixing => ProcessCompleted.BalanceFixingProcessType,
            _ => throw new NotImplementedException($"Process type '{processType}' not implemented"),
        };
}
