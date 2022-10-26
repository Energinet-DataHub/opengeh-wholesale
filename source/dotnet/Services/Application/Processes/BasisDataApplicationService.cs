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
using Energinet.DataHub.Wholesale.Application.Infrastructure;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;

namespace Energinet.DataHub.Wholesale.Application.Processes;

public class BasisDataApplicationService : IBasisDataApplicationService
{
    private readonly IBatchRepository _batchRepository;
    private readonly IBatchFileManager _batchFileManager;
    private readonly IUnitOfWork _unitOfWork;

    public BasisDataApplicationService(IBatchRepository batchRepository, IBatchFileManager batchFileManager, IUnitOfWork unitOfWork)
    {
        _batchRepository = batchRepository;
        _batchFileManager = batchFileManager;
        _unitOfWork = unitOfWork;
    }

    public async Task ZipBasisDataAsync(BatchCompletedEventDto batchCompletedEvent)
    {
        var batch = await _batchRepository.GetAsync(batchCompletedEvent.BatchId).ConfigureAwait(false);
        await _batchFileManager.CreateBasisDataZipAsync(batch).ConfigureAwait(false);
        batch.IsBasisDataDownloadAvailable = true;
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    public async Task<Stream> GetZippedBasisDataStreamAsync(Guid batchId)
    {
        var batch = await _batchRepository.GetAsync(batchId).ConfigureAwait(false);
        return await _batchFileManager.GetZippedBasisDataStreamAsync(batch).ConfigureAwait(false);
    }
}
