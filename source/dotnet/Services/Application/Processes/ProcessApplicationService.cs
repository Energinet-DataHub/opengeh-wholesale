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
        await PublishCalculationResultCompletedForEnergySuppliersAsync(processCompletedEvent).ConfigureAwait(false);
        await PublishCalculationResultCompletedForTotalGridAreaAsync(processCompletedEvent).ConfigureAwait(false);
    }

    private async Task PublishCalculationResultCompletedForTotalGridAreaAsync(ProcessCompletedEventDto processCompletedEvent)
    {
        var productionForTotalGa = await _processStepResultRepository
            .GetAsync(
                processCompletedEvent.BatchId,
                new GridAreaCode(processCompletedEvent.GridAreaCode),
                TimeSeriesType.Production,
                null,
                null)
            .ConfigureAwait(false);

        await _integrationEventPublisher
            .PublishCalculationResultForTotalGridAreaAsync(productionForTotalGa, processCompletedEvent)
            .ConfigureAwait(false);
    }

    private async Task PublishCalculationResultCompletedForEnergySuppliersAsync(ProcessCompletedEventDto processCompletedEvent)
    {
        var timeSeriesTypes = Enum.GetValues(typeof(TimeSeriesType)).Cast<TimeSeriesType>();
        foreach (var timeSeriesType in timeSeriesTypes)
        {
            var actors = await _actorRepository.GetEnergySuppliersAsync(
                processCompletedEvent.BatchId,
                new GridAreaCode(processCompletedEvent.GridAreaCode),
                timeSeriesType).ConfigureAwait(false);

            foreach (var actor in actors)
            {
                var processStepResultDto = await _processStepResultRepository
                    .GetAsync(
                        processCompletedEvent.BatchId,
                        new GridAreaCode(processCompletedEvent.GridAreaCode),
                        timeSeriesType,
                        actor.Gln,
                        null)
                    .ConfigureAwait(false);

                await _integrationEventPublisher.PublishCalculationResultForEnergySupplierAsync(
                    processStepResultDto,
                    processCompletedEvent,
                    actor.Gln).ConfigureAwait(false);
            }
        }
    }
}
