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

using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;

namespace Energinet.DataHub.Wholesale.Application.Processes;

public class BasisDataService : IBasisDataService
{
    private readonly IBatchRepository _batchRepository;
    private readonly IBatchFileManager _batchFileManager;

    public BasisDataService(IBatchRepository batchRepository, IBatchFileManager batchFileManager)
    {
        _batchRepository = batchRepository;
        _batchFileManager = batchFileManager;
    }

    public async Task ZipBasisDataAsync(ProcessCompletedEventDto completedProcessEvent)
    {
        var batch = await _batchRepository.GetAsync(completedProcessEvent.BatchId).ConfigureAwait(false);

        // This will often happen as we are listening to process completed event as we have no batch completed event
        if (await _batchFileManager.ExistsBasisDataZipAsync(batch).ConfigureAwait(false))
            return;

        await _batchFileManager.CreateBasisDataZipAsync(batch).ConfigureAwait(false);
    }
}
