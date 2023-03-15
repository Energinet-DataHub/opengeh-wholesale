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

using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Domain.ActorAggregate;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;

namespace Energinet.DataHub.Wholesale.Application.Processes;

public class ProcessApplicationServiceV2 : IProcessApplicationServiceV2
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly IProcessCompletedEventDtoFactory _processCompletedEventDtoFactory;
    private readonly IDomainEventPublisher _domainEventPublisher;
    private readonly IProcessStepResultRepository _processStepResultRepository;
    private readonly IActorRepository _actorRepository;
    private readonly IOutboxService _outboxService;
    private readonly IOutboxMessageFactory _outboxMessageFactory;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessApplicationServiceV2(
        IIntegrationEventPublisher integrationEventPublisher,
        IProcessCompletedEventDtoFactory processCompletedEventDtoFactory,
        IDomainEventPublisher domainEventPublisher,
        IProcessStepResultRepository processStepResultRepository,
        IActorRepository actorRepository,
        IOutboxService outboxService,
        IOutboxMessageFactory outboxMessageFactory,
        IUnitOfWork unitOfWork)
    {
        _integrationEventPublisher = integrationEventPublisher;
        _processCompletedEventDtoFactory = processCompletedEventDtoFactory;
        _domainEventPublisher = domainEventPublisher;
        _processStepResultRepository = processStepResultRepository;
        _actorRepository = actorRepository;
        _outboxService = outboxService;
        _outboxMessageFactory = outboxMessageFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task PublishProcessCompletedEventsAsync(BatchCompletedEventDto batchCompletedEvent)
    {
        var processCompletedEvents = _processCompletedEventDtoFactory.CreateFromBatchCompletedEvent(batchCompletedEvent);
        await _domainEventPublisher.PublishAsync(processCompletedEvents).ConfigureAwait(false);
    }

    public async Task PublishProcessCompletedIntegrationEventsAsync(ProcessCompletedEventDto processCompletedEvent)
    {
        await _integrationEventPublisher.PublishAsync(processCompletedEvent).ConfigureAwait(false);
    }

    public async Task PublishCalculationResultReadyIntegrationEventsAsync(ProcessCompletedEventDto processCompletedEventDto)
    {
        // Publish events for energy suppliers
        await CreateOutboxMessageWithCalculationResultCompletedForEnergySuppliersAsync(processCompletedEventDto, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);

        // Publish events for total grid area - production
        await CreateOutboxMessageWithCalculationResultCompletedForTotalGridAreaAsync(processCompletedEventDto, TimeSeriesType.Production).ConfigureAwait(false);

        // Publish events for total grid area - non profile
        await CreateOutboxMessageWithCalculationResultCompletedForTotalGridAreaAsync(processCompletedEventDto, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);

        // TODO AJH
        // Publish events for balance responsible party
        await CreateOutboxMessageWithCalculationResultCompletedForBalanceResponsiblePartiesAsync(processCompletedEventDto, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);
    }

    private async Task CreateOutboxMessageWithCalculationResultCompletedForTotalGridAreaAsync(ProcessCompletedEventDto processCompletedEvent, TimeSeriesType timeSeriesType)
    {
            var productionForTotalGa = await _processStepResultRepository
                .GetAsync(
                    processCompletedEvent.BatchId,
                    new GridAreaCode(processCompletedEvent.GridAreaCode),
                    timeSeriesType,
                    null,
                    null)
                .ConfigureAwait(false);

            await _integrationEventPublisher
                .PublishCalculationResultForTotalGridAreaAsync(productionForTotalGa, processCompletedEvent)
                .ConfigureAwait(false);

            var outboxMessage = _outboxMessageFactory.CreateMessageCalculationResultForTotalGridArea(productionForTotalGa, processCompletedEvent);
            await _outboxService.AddAsync(outboxMessage).ConfigureAwait(false);
            await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    private async Task CreateOutboxMessageWithCalculationResultCompletedForEnergySuppliersAsync(ProcessCompletedEventDto processCompletedEvent, TimeSeriesType timeSeriesType)
    {
            var energySuppliers = await _actorRepository.GetEnergySuppliersAsync(
                processCompletedEvent.BatchId,
                new GridAreaCode(processCompletedEvent.GridAreaCode),
                timeSeriesType).ConfigureAwait(false);

            foreach (var energySupplier in energySuppliers)
            {
                var processStepResultDto = await _processStepResultRepository
                    .GetAsync(
                        processCompletedEvent.BatchId,
                        new GridAreaCode(processCompletedEvent.GridAreaCode),
                        timeSeriesType,
                        energySupplier.Gln,
                        null)
                    .ConfigureAwait(false);

                var outboxMessage = _outboxMessageFactory.CreateMessageCalculationResultForEnergySupplier(processStepResultDto, processCompletedEvent, energySupplier.Gln);
                await _outboxService.AddAsync(outboxMessage).ConfigureAwait(false);
            }

            await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    private async Task CreateOutboxMessageWithCalculationResultCompletedForBalanceResponsiblePartiesAsync(ProcessCompletedEventDto processCompletedEvent, TimeSeriesType timeSeriesType)
    {
        var balanceResponsibleParties = await _actorRepository.GetBalanceResponsiblePartiesAsync(
            processCompletedEvent.BatchId,
            new GridAreaCode(processCompletedEvent.GridAreaCode),
            timeSeriesType).ConfigureAwait(false);

        foreach (var balanceResponsibleParty in balanceResponsibleParties)
        {
            var processStepResultDto = await _processStepResultRepository
                .GetAsync(
                    processCompletedEvent.BatchId,
                    new GridAreaCode(processCompletedEvent.GridAreaCode),
                    timeSeriesType,
                    balanceResponsibleParty.Gln,
                    null)
                .ConfigureAwait(false);

            var outboxMessage = _outboxMessageFactory.CreateMessageForCalculationResultForBalanceResponsibleParty(processStepResultDto, processCompletedEvent, balanceResponsibleParty.Gln);
            await _outboxService.AddAsync(outboxMessage).ConfigureAwait(false);
        }

        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }
}
