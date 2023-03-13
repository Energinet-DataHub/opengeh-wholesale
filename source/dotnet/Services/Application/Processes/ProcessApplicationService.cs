﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;

namespace Energinet.DataHub.Wholesale.Application.Processes;

public class ProcessApplicationService : IProcessApplicationService
{
    private readonly IProcessCompletedIntegrationEventPublisher _processCompletedIntegrationEventPublisher;
    private readonly IProcessCompletedEventDtoFactory _processCompletedEventDtoFactory;
    private readonly IDomainEventPublisher _domainEventPublisher;
    private readonly IOutboxService _outboxService;
    private readonly IOutboxMessageFactory _outboxMessageFactory;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessApplicationService(
        IProcessCompletedIntegrationEventPublisher processCompletedIntegrationEventPublisher,
        IProcessCompletedEventDtoFactory processCompletedEventDtoFactory,
        IDomainEventPublisher domainEventPublisher,
        IOutboxService outboxService,
        IOutboxMessageFactory outboxMessageFactory,
        IUnitOfWork unitOfWork)
    {
        _processCompletedIntegrationEventPublisher = processCompletedIntegrationEventPublisher;
        _processCompletedEventDtoFactory = processCompletedEventDtoFactory;
        _domainEventPublisher = domainEventPublisher;
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
        var outboxMessage = _outboxMessageFactory.CreateFrom(processCompletedEvent);
        await _outboxService.AddAsync(outboxMessage).ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }
}
