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

using Energinet.DataHub.Wholesale.Domain.BatchAggregate;

namespace Energinet.DataHub.Wholesale.Domain.CalculationDomainService;

public class CalculationDomainService : ICalculationDomainService
{
    private readonly IBatchRepository _batchRepository;
    private readonly IDatabricksCalculationInfrastructureService _databricksCalculationInfrastructureService;

    public CalculationDomainService(
        IBatchRepository batchRepository,
        IDatabricksCalculationInfrastructureService databricksCalculationInfrastructureService)
    {
        _batchRepository = batchRepository;
        _databricksCalculationInfrastructureService = databricksCalculationInfrastructureService;
    }

    public async Task<CalculationState> GetStatusAsync(JobRunId jobRunId)
    {
        return await _databricksCalculationInfrastructureService.GetStatusAsync(jobRunId).ConfigureAwait(false);
    }

    public async Task StartAsync(Guid batchId)
    {
        var batch = await _batchRepository.GetAsync(batchId).ConfigureAwait(false);
        var jobRunId = await _databricksCalculationInfrastructureService.StartAsync(batch).ConfigureAwait(false);
        batch.MarkAsSubmitted(jobRunId);
    }
}
