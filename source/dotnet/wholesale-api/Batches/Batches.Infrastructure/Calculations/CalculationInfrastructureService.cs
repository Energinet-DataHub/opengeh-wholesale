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

using Energinet.DataHub.Wholesale.Batches.Application;
using Energinet.DataHub.Wholesale.Batches.Application.Model;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;

namespace Energinet.DataHub.Wholesale.Batches.Infrastructure.Calculations;

public class CalculationInfrastructureService : ICalculationInfrastructureService
{
    private readonly IBatchRepository _batchRepository;
    private readonly ICalculationEngineClient _calculationEngineClient;

    public CalculationInfrastructureService(
        IBatchRepository batchRepository,
        ICalculationEngineClient calculationEngineClient)
    {
        _batchRepository = batchRepository;
        _calculationEngineClient = calculationEngineClient;
    }

    public async Task<CalculationState> GetStatusAsync(CalculationId calculationId)
    {
        return await _calculationEngineClient.GetStatusAsync(calculationId).ConfigureAwait(false);
    }

    public async Task StartAsync(Guid batchId)
    {
        var batch = await _batchRepository.GetAsync(batchId).ConfigureAwait(false);
        var calculationId = await _calculationEngineClient.StartAsync(batch).ConfigureAwait(false);
        batch.MarkAsSubmitted(calculationId);
    }
}
