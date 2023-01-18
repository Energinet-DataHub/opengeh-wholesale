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

using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessActorResultAggregate;

namespace Energinet.DataHub.Wholesale.Application.ProcessResult;

/// <summary>
/// This class provides the ability to retrieve a calculated result for a given step for a batch.
/// </summary>
public class ProcessActorResultApplicationService : IProcessActorResultApplicationService
{
    private readonly IProcessActorResultRepository _processActorResultRepository;
    private readonly IProcessActorResultMapper _processActorResultMapper;

    public ProcessActorResultApplicationService(IProcessActorResultRepository processActorResultRepository, IProcessActorResultMapper processActorResultMapper)
    {
        _processActorResultRepository = processActorResultRepository;
        _processActorResultMapper = processActorResultMapper;
    }

    public async Task<ProcessStepResultDto> GetResultAsync(ProcessStepResultRequestDto processStepResultRequestDto)
    {
        var processActorResult = await _processActorResultRepository.GetAsync(
                processStepResultRequestDto.BatchId,
                new GridAreaCode(processStepResultRequestDto.GridAreaCode))
            .ConfigureAwait(false);

        return _processActorResultMapper.MapToDto(processActorResult);
    }
}
