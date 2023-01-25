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

using Energinet.DataHub.Wholesale.Application.ProcessResult.Model;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.BatchActor;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using MarketRoleType = Energinet.DataHub.Wholesale.Contracts.MarketRoleType;

namespace Energinet.DataHub.Wholesale.Application.BatchActor;

public class BatchActorApplicationService : IBatchActorApplicationService
{
    private readonly IBatchActorRepository _batchActorRepository;

    public BatchActorApplicationService(IBatchActorRepository batchActorRepository)
    {
        _batchActorRepository = batchActorRepository;
    }

    public async Task<BatchActorDto[]> GetAsync(BatchActorRequestDto batchActorRequestDto)
    {
        var actors = await _batchActorRepository.GetAsync(
            batchActorRequestDto.BatchId,
            new GridAreaCode(batchActorRequestDto.GridAreaCode),
            TimeSeriesTypeMapper.Map(batchActorRequestDto.Type),
            Map(batchActorRequestDto.MarketRoleType)).ConfigureAwait(false);

        return actors.Select(x => new BatchActorDto(x.Gln)).ToArray();
    }

    private Domain.BatchActor.MarketRoleType Map(MarketRoleType marketRoleType)
    {
        switch (marketRoleType)
        {
            case MarketRoleType.EnergySupplier:
                return Domain.BatchActor.MarketRoleType.EnergySupplier;
            default:
                throw new ArgumentOutOfRangeException(nameof(marketRoleType), marketRoleType, null);
        }
    }
}
