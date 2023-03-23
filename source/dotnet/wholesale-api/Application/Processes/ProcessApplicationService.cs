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

using Energinet.DataHub.Wholesale.Application.IntegrationEventsManagement;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Domain.ActorAggregate;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;

namespace Energinet.DataHub.Wholesale.Application.Processes;

public class ProcessApplicationService : IProcessApplicationService
{
    private readonly IProcessCompletedEventDtoFactory _processCompletedEventDtoFactory;
    private readonly IDomainEventPublisher _domainEventPublisher;
    private readonly IProcessStepResultRepository _processStepResultRepository;
    private readonly IActorRepository _actorRepository;
    private readonly ICalculationResultCompletedFactory _calculationResultCompletedFactory;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessApplicationService(
        IProcessCompletedEventDtoFactory processCompletedEventDtoFactory,
        IDomainEventPublisher domainEventPublisher,
        IProcessStepResultRepository processStepResultRepository,
        IActorRepository actorRepository,
        ICalculationResultCompletedFactory integrationEventFactory,
        IIntegrationEventPublisher integrationEventPublisher,
        IUnitOfWork unitOfWork)
    {
        _processCompletedEventDtoFactory = processCompletedEventDtoFactory;
        _domainEventPublisher = domainEventPublisher;
        _processStepResultRepository = processStepResultRepository;
        _actorRepository = actorRepository;
        _calculationResultCompletedFactory = integrationEventFactory;
        _integrationEventPublisher = integrationEventPublisher;
        _unitOfWork = unitOfWork;
    }

    public async Task PublishProcessCompletedEventsAsync(BatchCompletedEventDto batchCompletedEvent)
    {
        var processCompletedEvents = _processCompletedEventDtoFactory.CreateFromBatchCompletedEvent(batchCompletedEvent);
        await _domainEventPublisher.PublishAsync(processCompletedEvents).ConfigureAwait(false);
    }

    public async Task PublishCalculationResultCompletedIntegrationEventsAsync(ProcessCompletedEventDto processCompletedEvent)
    {
        // Publish events for energy suppliers
        await PublishCalculationResultCompletedForEnergySuppliersAsync(processCompletedEvent, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);

        // Publish events for total grid area - production
        await PublishCalculationResultCompletedForTotalGridAreaAsync(processCompletedEvent, TimeSeriesType.Production).ConfigureAwait(false);

        // Publish events for total grid area - non profiled
        await PublishCalculationResultCompletedForTotalGridAreaAsync(processCompletedEvent, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);

        // Publish events for balance responsible party
        await PublishCalculationResultCompletedForBalanceResponsiblePartiesAsync(processCompletedEvent, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);

        // Publish events for energy suppliers results for balance responsible parties
        await PublishCalculationResultCompletedForEnergySupplierBalanceResponsiblePartiesAsync(processCompletedEvent, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);

        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    private async Task PublishCalculationResultCompletedForEnergySupplierBalanceResponsiblePartiesAsync(ProcessCompletedEventDto processCompletedEvent, TimeSeriesType timeSeriesType)
    {
        var brps = await _actorRepository
            .GetBalanceResponsiblePartiesAsync(
                processCompletedEvent.BatchId,
                new GridAreaCode(processCompletedEvent.GridAreaCode),
                timeSeriesType).ConfigureAwait(false);
        foreach (var brp in brps)
        {
            var energySuppliersByBalanceResponsibleParty = await _actorRepository
                .GetEnergySuppliersByBalanceResponsiblePartyAsync(
                    processCompletedEvent.BatchId,
                    new GridAreaCode(processCompletedEvent.GridAreaCode),
                    timeSeriesType,
                    brp.Gln).ConfigureAwait(false);

            foreach (var energySupplier in energySuppliersByBalanceResponsibleParty)
            {
                var result = await _processStepResultRepository.GetAsync(
                        processCompletedEvent.BatchId,
                        new GridAreaCode(processCompletedEvent.GridAreaCode),
                        timeSeriesType,
                        energySupplier.Gln,
                        brp.Gln)
                    .ConfigureAwait(false);

                var integrationEvent = _calculationResultCompletedFactory.CreateForEnergySupplierByBalanceResponsibleParty(result, processCompletedEvent, energySupplier.Gln, brp.Gln);
                await _integrationEventPublisher.PublishAsync(integrationEvent).ConfigureAwait(false);
            }
        }
    }

    private async Task PublishCalculationResultCompletedForTotalGridAreaAsync(ProcessCompletedEventDto processCompletedEvent, TimeSeriesType timeSeriesType)
    {
            var productionForTotalGa = await _processStepResultRepository
                .GetAsync(
                    processCompletedEvent.BatchId,
                    new GridAreaCode(processCompletedEvent.GridAreaCode),
                    timeSeriesType,
                    null,
                    null)
                .ConfigureAwait(false);

            var integrationEvent = _calculationResultCompletedFactory.CreateForTotalGridArea(productionForTotalGa, processCompletedEvent);
            await _integrationEventPublisher.PublishAsync(integrationEvent).ConfigureAwait(false);
    }

    private async Task PublishCalculationResultCompletedForEnergySuppliersAsync(ProcessCompletedEventDto processCompletedEvent, TimeSeriesType timeSeriesType)
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

                var integrationEvent = _calculationResultCompletedFactory.CreateForEnergySupplier(processStepResultDto, processCompletedEvent, energySupplier.Gln);
                await _integrationEventPublisher.PublishAsync(integrationEvent).ConfigureAwait(false);
            }
    }

    private async Task PublishCalculationResultCompletedForBalanceResponsiblePartiesAsync(ProcessCompletedEventDto processCompletedEvent, TimeSeriesType timeSeriesType)
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
                    null,
                    balanceResponsibleParty.Gln)
                .ConfigureAwait(false);

            var integrationEvent = _calculationResultCompletedFactory.CreateForBalanceResponsibleParty(processStepResultDto, processCompletedEvent, balanceResponsibleParty.Gln);
            await _integrationEventPublisher.PublishAsync(integrationEvent).ConfigureAwait(false);
        }
    }
}
