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
using Energinet.DataHub.Wholesale.Domain.Actor;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;

namespace Energinet.DataHub.Wholesale.Application.BatchActor;

public class ActorApplicationService : IActorApplicationService
{
    private readonly IActorRepository _actorRepository;

    public ActorApplicationService(IActorRepository actorRepository)
    {
        _actorRepository = actorRepository;
    }

    public async Task<BatchActorDto[]> GetAsync(ProcessStepActorsRequest processStepActorsRequest)
    {
        var actors = await _actorRepository.GetAsync(
            processStepActorsRequest.BatchId,
            new GridAreaCode(processStepActorsRequest.GridAreaCode),
            TimeSeriesTypeMapper.Map(processStepActorsRequest.Type),
            MarketRoleTypeMapper.Map(processStepActorsRequest.MarketRoleType)).ConfigureAwait(false);

        return actors.Select(batchActor => new BatchActorDto(batchActor.Gln)).ToArray();
    }
}
