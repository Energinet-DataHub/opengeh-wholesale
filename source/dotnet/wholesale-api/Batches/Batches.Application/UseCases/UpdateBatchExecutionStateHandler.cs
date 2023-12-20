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

namespace Energinet.DataHub.Wholesale.Batches.Application.UseCases;

public class UpdateBatchExecutionStateHandler : IUpdateBatchExecutionStateHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICalculationExecutionStateInfrastructureService _calculationExecutionStateInfrastructureService;

    public UpdateBatchExecutionStateHandler(IUnitOfWork unitOfWork, ICalculationExecutionStateInfrastructureService calculationExecutionStateInfrastructureService)
    {
        _unitOfWork = unitOfWork;
        _calculationExecutionStateInfrastructureService = calculationExecutionStateInfrastructureService;
    }

    public async Task UpdateAsync()
    {
        await _calculationExecutionStateInfrastructureService.UpdateExecutionStateAsync().ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }
}
