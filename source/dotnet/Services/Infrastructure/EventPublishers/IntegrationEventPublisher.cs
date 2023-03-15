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

using Energinet.DataHub.Wholesale.Application;
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
    private readonly ICalculationResultReadyIntegrationEventFactory _calculationResultReadyIntegrationEventFactory;

    public IntegrationEventPublisher(
        IIntegrationEventTopicServiceBusSender serviceBusSender,
        IServiceBusMessageFactory serviceBusMessageFactory,
        IProcessCompletedIntegrationEventMapper processCompletedIntegrationEventMapper,
        ICalculationResultReadyIntegrationEventFactory calculationResultReadyIntegrationEventFactory)
    {
        _serviceBusSender = serviceBusSender;
        _serviceBusMessageFactory = serviceBusMessageFactory;
        _processCompletedIntegrationEventMapper = processCompletedIntegrationEventMapper;
        _calculationResultReadyIntegrationEventFactory = calculationResultReadyIntegrationEventFactory;
    }

    public async Task PublishAsync(ProcessCompletedEventDto processCompletedEvent)
    {
        var integrationEvent = _processCompletedIntegrationEventMapper.MapFrom(processCompletedEvent);
        var messageType = GetMessageTypeForProcessCompletedEvent(processCompletedEvent.ProcessType);
        var message = _serviceBusMessageFactory.CreateProcessCompleted(integrationEvent.ToByteArray(), messageType);
        await _serviceBusSender.SendMessageAsync(message, CancellationToken.None).ConfigureAwait(false);
    }

    public async Task PublishCalculationResultForTotalGridAreaAsync(
        ProcessStepResult processStepResultDto,
        ProcessCompletedEventDto processCompletedEventDto)
    {
        var integrationEvent =
            _calculationResultReadyIntegrationEventFactory.CreateCalculationResultCompletedForGridArea(processStepResultDto, processCompletedEventDto);
        await PublishCalculationResultCompletedAsync(integrationEvent).ConfigureAwait(false);
    }

    public async Task PublishCalculationResultForEnergySupplierAsync(
        ProcessStepResult processStepResultDto,
        ProcessCompletedEventDto processCompletedEventDto,
        string energySupplierGln)
    {
        var integrationEvent =
            _calculationResultReadyIntegrationEventFactory.CreateCalculationResultCompletedForEnergySupplier(processStepResultDto, processCompletedEventDto, energySupplierGln);
        await PublishCalculationResultCompletedAsync(integrationEvent).ConfigureAwait(false);
    }

    public async Task PublishCalculationResultForBalanceResponsiblePartyAsync(
        ProcessStepResult processStepResultDto,
        ProcessCompletedEventDto processCompletedEventDto,
        string balanceResponsiblePartyGln)
    {
        var integrationEvent =
            _calculationResultReadyIntegrationEventFactory.CreateCalculationResultCompletedForBalanceResponsibleParty(processStepResultDto, processCompletedEventDto, balanceResponsiblePartyGln);
        await PublishCalculationResultCompletedAsync(integrationEvent).ConfigureAwait(false);
    }

    private async Task PublishCalculationResultCompletedAsync(
        CalculationResultCompleted integrationEvent)
    {
        var message = _serviceBusMessageFactory.CreateProcessCompleted(integrationEvent.ToByteArray(), CalculationResultCompleted.BalanceFixingEventName);
        await _serviceBusSender.SendMessageAsync(message, CancellationToken.None).ConfigureAwait(false);
    }

    private string GetMessageTypeForProcessCompletedEvent(ProcessType processType) =>
        processType switch
        {
            ProcessType.BalanceFixing => ProcessCompleted.BalanceFixingProcessType,
            ProcessType.Aggregation => ProcessCompleted.AggregationProcessType,
            _ => throw new NotImplementedException($"Process type '{processType}' not implemented"),
        };
}
