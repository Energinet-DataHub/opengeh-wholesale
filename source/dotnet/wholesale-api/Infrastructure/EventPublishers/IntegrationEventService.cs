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
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Infrastructure.EventPublishers;

public class IntegrationEventService : IIntegrationEventService
{
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventDispatcher _integrationEventDispatcher;

    public IntegrationEventService(
        IOutboxMessageRepository outboxMessageRepository,
        IClock clock,
        IUnitOfWork unitOfWork,
        IIntegrationEventDispatcher integrationEventDispatcher)
    {
        _outboxMessageRepository = outboxMessageRepository;
        _clock = clock;
        _unitOfWork = unitOfWork;
        _integrationEventDispatcher = integrationEventDispatcher;
    }

    public async Task AddAsync(IntegrationEventDto integrationEventDto)
    {
        var outboxMessage = new OutboxMessage(integrationEventDto.EventData, integrationEventDto.MessageType, integrationEventDto.CreationDate);
        await _outboxMessageRepository.AddAsync(outboxMessage).ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    public async Task DeleteProcessedOlderThanAsync(int daysOld)
    {
        var instant = _clock.GetCurrentInstant();
        instant = instant.Minus(Duration.FromDays(daysOld));
        _outboxMessageRepository.DeleteProcessedOlderThan(instant);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    public async Task DispatchIntegrationEventsAsync()
    {
        await _integrationEventDispatcher.DispatchIntegrationEventsAsync().ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }
}
