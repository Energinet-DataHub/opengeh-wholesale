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

using Energinet.DataHub.Wholesale.Events.Application.Processes;
using Energinet.DataHub.Wholesale.Events.Application.Processes.Model;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Application.UseCases;

public class PublishCalculationResultsHandler : IPublishCalculationResultsHandler
{
    private readonly ICompletedBatchRepository _completedBatchRepository;
    private readonly IProcessApplicationService _processApplicationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public PublishCalculationResultsHandler(ICompletedBatchRepository completedBatchRepository, IProcessApplicationService processApplicationService, IUnitOfWork unitOfWork, IClock clock)
    {
        _completedBatchRepository = completedBatchRepository;
        _processApplicationService = processApplicationService;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task PublishCalculationResultsAsync()
    {
        do
        {
            var batch = await _completedBatchRepository.GetNextUnpublishedOrNullAsync().ConfigureAwait(false);
            if (batch == null) break;

            // TODO BJM: Reuse existing functionality of ProcessApplicationService and refactor in upcoming PR
            foreach (var gridAreaCode in batch.GridAreaCodes)
            {
                var @event = new ProcessCompletedEventDto(gridAreaCode, batch.Id, batch.ProcessType, batch.PeriodStart, batch.PeriodEnd);
                await _processApplicationService.PublishCalculationResultCompletedIntegrationEventsAsync(@event).ConfigureAwait(false);
            }

            batch.PublishedTime = _clock.GetCurrentInstant();
            await _unitOfWork.CommitAsync().ConfigureAwait(false);
        }
        while (true);
    }
}
