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

using Energinet.DataHub.Wholesale.Batches.Application.BatchAggregate;
using Energinet.DataHub.Wholesale.Batches.Interfaces;

namespace Energinet.DataHub.Wholesale.Batches.Application;

public class CreateBatchHandler : ICreateBatchHandler
{
    private readonly IBatchFactory _batchFactory;
    private readonly IBatchRepository _batchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBatchHandler(
        IBatchFactory batchFactory,
        IBatchRepository batchRepository,
        IUnitOfWork unitOfWork)
    {
        _batchFactory = batchFactory;
        _batchRepository = batchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(CreateBatchCommand command)
    {
        var batch = _batchFactory.Create(command.ProcessType, command.GridAreaCodes, command.StartDate, command.EndDate, command.CreatedByUserId);
        await _batchRepository.AddAsync(batch).ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
        return batch.Id;
    }
}
