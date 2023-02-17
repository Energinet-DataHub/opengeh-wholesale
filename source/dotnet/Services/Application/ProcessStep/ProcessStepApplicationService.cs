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

using Energinet.DataHub.Wholesale.Application.ProcessStep.Model;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.ActorAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using MarketRole = Energinet.DataHub.Wholesale.Contracts.MarketRole;
using TimeSeriesType = Energinet.DataHub.Wholesale.Contracts.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Application.ProcessStep;

/// <summary>
/// This class provides the ability to retrieve a calculated result for a given step for a batch.
/// </summary>
public class ProcessStepApplicationService : IProcessStepApplicationService
{
    private readonly IProcessStepResultRepository _processStepResultRepository;
    private readonly IProcessStepResultMapper _processStepResultMapper;
    private readonly IActorRepository _actorRepository;

    public ProcessStepApplicationService(
        IProcessStepResultRepository processStepResultRepository,
        IProcessStepResultMapper processStepResultMapper,
        IActorRepository actorRepository)
    {
        _processStepResultRepository = processStepResultRepository;
        _processStepResultMapper = processStepResultMapper;
        _actorRepository = actorRepository;
    }

    public async Task<WholesaleActorDto[]> GetActorsAsync(ProcessStepActorsRequest processStepActorsRequest)
    {
        var actors = await _actorRepository.GetAsync(
            processStepActorsRequest.BatchId,
            new GridAreaCode(processStepActorsRequest.GridAreaCode),
            TimeSeriesTypeMapper.Map(processStepActorsRequest.Type),
            MarketRoleMapper.Map(processStepActorsRequest.MarketRole)).ConfigureAwait(false);

        return actors.Select(batchActor => new WholesaleActorDto(batchActor.Gln)).ToArray();
    }

    public async Task<ProcessStepResultDto> GetResultAsync(
        Guid batchId,
        string gridAreaCode,
        TimeSeriesType timeSeriesType,
        string gln,
        MarketRole marketRole)
    {
        var processActorResult = await _processStepResultRepository.GetAsync(
                batchId,
                new GridAreaCode(gridAreaCode),
                TimeSeriesTypeMapper.Map(timeSeriesType),
                gln,
                MarketRoleMapper.Map(marketRole))
            .ConfigureAwait(false);

        return _processStepResultMapper.MapToDto(processActorResult);
    }
}
