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

using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.Batches.Model;
using Energinet.DataHub.Wholesale.Application.Processes.Model;

namespace Energinet.DataHub.Wholesale.Application.Processes;

public class ProcessApplicationService : IProcessApplicationService
{
    private readonly IProcessCompletedPublisher _processCompletedPublisher;
    private readonly IProcessCompletedIntegrationEventPublisher _processCompletedIntegrationEventPublisher;
    private readonly IProcessCompletedEventDtoFactory _processCompletedEventDtoFactory;

    public ProcessApplicationService(
        IProcessCompletedPublisher processCompletedPublisher,
        IProcessCompletedIntegrationEventPublisher processCompletedIntegrationEventPublisher,
        IProcessCompletedEventDtoFactory processCompletedEventDtoFactory)
    {
        _processCompletedPublisher = processCompletedPublisher;
        _processCompletedIntegrationEventPublisher = processCompletedIntegrationEventPublisher;
        _processCompletedEventDtoFactory = processCompletedEventDtoFactory;
    }

    public async Task PublishProcessCompletedEventsAsync(BatchCompletedEventDto batchCompletedEvent)
    {
        var processCompletedEvents = _processCompletedEventDtoFactory.CreateFromBatchCompletedEvent(batchCompletedEvent);
        await _processCompletedPublisher.PublishAsync(processCompletedEvents).ConfigureAwait(false);
    }

    public async Task PublishProcessCompletedIntegrationEventsAsync(ProcessCompletedEventDto processCompletedEvent)
    {
        await _processCompletedIntegrationEventPublisher.PublishAsync(processCompletedEvent).ConfigureAwait(false);
    }
}
