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

using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;

namespace Energinet.DataHub.Wholesale.Events.Application.UseCases;

public class RegisterCompletedCalculationsHandler : IRegisterCompletedCalculationsHandler
{
    private readonly ICalculationsClient _calculationsClient;
    private readonly ICompletedBatchRepository _completedBatchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompletedBatchFactory _completedBatchFactory;

    public RegisterCompletedCalculationsHandler(
        ICalculationsClient calculationsClient,
        ICompletedBatchRepository completedBatchRepository,
        IUnitOfWork unitOfWork,
        ICompletedBatchFactory completedBatchFactory)
    {
        _calculationsClient = calculationsClient;
        _completedBatchRepository = completedBatchRepository;
        _unitOfWork = unitOfWork;
        _completedBatchFactory = completedBatchFactory;
    }

    public async Task RegisterCompletedCalculationsAsync()
    {
        var newCompletedBatches = await GetNewCompletedBatchesAsync().ConfigureAwait(false);
        await _completedBatchRepository.AddAsync(newCompletedBatches).ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    private async Task<IEnumerable<CompletedBatch>> GetNewCompletedBatchesAsync()
    {
        var lastKnownCompletedBatch = await _completedBatchRepository.GetLastCompletedOrNullAsync().ConfigureAwait(false);
        var completedBatchDtos = await _calculationsClient.GetBatchesCompletedAfterAsync(lastKnownCompletedBatch?.CompletedTime).ConfigureAwait(false);
        return _completedBatchFactory.CreateFromBatches(completedBatchDtos);
    }
}
