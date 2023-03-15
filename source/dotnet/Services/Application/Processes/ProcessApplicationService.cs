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

public class ProcessApplicationService : IProcessApplicationService
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly IProcessCompletedEventDtoFactory _processCompletedEventDtoFactory;
    private readonly IDomainEventPublisher _domainEventPublisher;
    private readonly IProcessStepResultRepository _processStepResultRepository;
    private readonly IActorRepository _actorRepository;

    public ProcessApplicationService(
        IIntegrationEventPublisher integrationEventPublisher,
        IProcessCompletedEventDtoFactory processCompletedEventDtoFactory,
        IDomainEventPublisher domainEventPublisher,
        IProcessStepResultRepository processStepResultRepository,
        IActorRepository actorRepository)
    {
        _integrationEventPublisher = integrationEventPublisher;
        _processCompletedEventDtoFactory = processCompletedEventDtoFactory;
        _domainEventPublisher = domainEventPublisher;
        _processStepResultRepository = processStepResultRepository;
        _actorRepository = actorRepository;
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

    public async Task PublishCalculationResultReadyIntegrationEventsAsync(ProcessCompletedEventDto processCompletedEvent)
    {
        // Publish events for energy suppliers
        await PublishCalculationResultCompletedForEnergySuppliersAsync(processCompletedEvent, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);

        // Publish events for total grid area
        await PublishCalculationResultCompletedForTotalGridAreaAsync(processCompletedEvent, TimeSeriesType.Production).ConfigureAwait(false);
        await PublishCalculationResultCompletedForTotalGridAreaAsync(processCompletedEvent, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);

        // Publish events for balance responsible party
        await PublishCalculationResultCompletedForBalanceResponsiblePartiesAsync(processCompletedEvent, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);

        // Publish events for energy suppliers results for balance responsible parties
        await PublishCalculationResultCompletedForEnergySupplierBalanceResponsiblePartiesAsync(processCompletedEvent, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);
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

                await _integrationEventPublisher
                    .PublishCalculationResultForEnergySupplierByBalanceResponsiblePartyAsync(
                        result,
                        processCompletedEvent,
                        energySupplier.Gln,
                        brp.Gln).ConfigureAwait(false);
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

            await _integrationEventPublisher
                .PublishCalculationResultForTotalGridAreaAsync(productionForTotalGa, processCompletedEvent)
                .ConfigureAwait(false);
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

                await _integrationEventPublisher.PublishCalculationResultForEnergySupplierAsync(
                    processStepResultDto,
                    processCompletedEvent,
                    energySupplier.Gln).ConfigureAwait(false);
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
                    balanceResponsibleParty.Gln,
                    null)
                .ConfigureAwait(false);

            await _integrationEventPublisher.PublishCalculationResultForBalanceResponsiblePartyAsync(
                processStepResultDto,
                processCompletedEvent,
                balanceResponsibleParty.Gln).ConfigureAwait(false);
        }
    }
}
