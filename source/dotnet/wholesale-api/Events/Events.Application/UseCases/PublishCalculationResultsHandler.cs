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

using Energinet.DataHub.Wholesale.Events.Application.CalculationResultPublishing;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Application.UseCases;

public class PublishCalculationResultsHandler : IPublishCalculationResultsHandler
{
    private readonly ICompletedBatchRepository _completedBatchRepository;
    private readonly ICalculationResultPublisher _calculationResultPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public PublishCalculationResultsHandler(ICompletedBatchRepository completedBatchRepository, ICalculationResultPublisher calculationResultPublisher, IUnitOfWork unitOfWork, IClock clock)
    {
        _completedBatchRepository = completedBatchRepository;
        _calculationResultPublisher = calculationResultPublisher;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task PublishCalculationResultsAsync()
    {
        do
        {
            var batch = await _completedBatchRepository.GetNextUnpublishedOrNullAsync().ConfigureAwait(false);
            if (batch == null) break;

            await _calculationResultPublisher.PublishForBatchAsync(batch).ConfigureAwait(false);

            batch.PublishedTime = _clock.GetCurrentInstant();
            await _unitOfWork.CommitAsync().ConfigureAwait(false);
        }
        while (true);
    }
}
