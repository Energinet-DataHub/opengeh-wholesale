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
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Calculations.Application.UseCases;

public class StartCalculationHandler : IStartCalculationHandler
{
    private readonly ICalculationRepository _calculationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICalculationInfrastructureService _calculationInfrastructureService;
    private readonly ILogger _logger;

    public StartCalculationHandler(
        ICalculationInfrastructureService calculationInfrastructureService,
        IUnitOfWork unitOfWork,
        ICalculationRepository calculationRepository,
        ILogger<StartCalculationHandler> logger)
    {
        _calculationInfrastructureService = calculationInfrastructureService;
        _unitOfWork = unitOfWork;
        _calculationRepository = calculationRepository;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        var calculations = await _calculationRepository.GetCreatedAsync().ConfigureAwait(false);
        foreach (var batch in calculations)
        {
            await _calculationInfrastructureService.StartAsync(batch.Id).ConfigureAwait(false);
            await _unitOfWork.CommitAsync().ConfigureAwait(false);

            _logger.LogInformation("Calculation with id {calculation_id} started.", batch.Id);
        }
    }
}
