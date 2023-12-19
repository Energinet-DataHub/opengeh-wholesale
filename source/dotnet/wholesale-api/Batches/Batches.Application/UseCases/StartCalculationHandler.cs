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
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Batches.Application.UseCases;

public class StartCalculationHandler : IStartCalculationHandler
{
    private readonly IBatchRepository _batchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICalculationInfrastructureService _calculationInfrastructureService;
    private readonly ILogger _logger;

    public StartCalculationHandler(
        ICalculationInfrastructureService calculationInfrastructureService,
        IUnitOfWork unitOfWork,
        IBatchRepository batchRepository,
        ILogger<StartCalculationHandler> logger)
    {
        _calculationInfrastructureService = calculationInfrastructureService;
        _unitOfWork = unitOfWork;
        _batchRepository = batchRepository;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        var batches = await _batchRepository.GetCreatedAsync().ConfigureAwait(false);
        foreach (var batch in batches)
        {
            await _calculationInfrastructureService.StartAsync(batch.Id).ConfigureAwait(false);
            await _unitOfWork.CommitAsync().ConfigureAwait(false);

            _logger.LogInformation("Calculation with id {calculation_id} started.", batch.Id);
        }
    }
}
