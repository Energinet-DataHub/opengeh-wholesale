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

using Energinet.DataHub.Wholesale.Calculations.Interfaces;

namespace Energinet.DataHub.Wholesale.Calculations.Application.UseCases;

public class UpdateCalculationStateHandler : IUpdateCalculationStateHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICalculationStateInfrastructureService _calculationStateInfrastructureService;

    public UpdateCalculationStateHandler(
        IUnitOfWork unitOfWork,
        ICalculationStateInfrastructureService calculationStateInfrastructureService)
    {
        _unitOfWork = unitOfWork;
        _calculationStateInfrastructureService = calculationStateInfrastructureService;
    }

    public async Task UpdateStateAsync()
    {
        await _calculationStateInfrastructureService.UpdateStateAsync().ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }
}
