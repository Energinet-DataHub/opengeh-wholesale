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

using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Wholesale.Batches.Application.IntegrationEvents;
using Energinet.DataHub.Wholesale.Batches.Application.IntegrationEvents.Handlers;
using IIntegrationEventHandler = Energinet.DataHub.Core.Messaging.Communication.Subscriber.IIntegrationEventHandler;

namespace Energinet.DataHub.Wholesale.Batches.Application.UseCases;

// ReSharper disable once ClassNeverInstantiated.Global - instantiated by DI container
public class ReceivedIntegrationEventHandler : IIntegrationEventHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReceivedIntegrationEventRepository _receivedIntegrationEventRepository;
    private readonly IntegrationEventHandlerFactory _integrationEventHandlerFactory;

    public ReceivedIntegrationEventHandler(IUnitOfWork unitOfWork, IReceivedIntegrationEventRepository receivedIntegrationEventRepository, IntegrationEventHandlerFactory integrationEventHandlerFactory)
    {
        _unitOfWork = unitOfWork;
        _receivedIntegrationEventRepository = receivedIntegrationEventRepository;
        _integrationEventHandlerFactory = integrationEventHandlerFactory;
    }

    public async Task HandleAsync(IntegrationEvent integrationEvent)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var eventAlreadyHandled = await _receivedIntegrationEventRepository.ExistsAsync(integrationEvent.EventIdentification).ConfigureAwait(false);
        if (eventAlreadyHandled)
            return;

        await _receivedIntegrationEventRepository.CreateAsync(integrationEvent.EventIdentification, integrationEvent.EventName).ConfigureAwait(false);

        // Commit immediately to ensure that the event is registered, so we don't handle the same event twice.
        await _unitOfWork.CommitAsync().ConfigureAwait(false);

        var handler = _integrationEventHandlerFactory.GetHandler(integrationEvent.EventName);

        await handler.HandleAsync(integrationEvent).ConfigureAwait(false);

        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }
}
