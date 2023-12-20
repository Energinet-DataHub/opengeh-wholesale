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

using Energinet.DataHub.Wholesale.Batches.Application.Model.Batches;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Batches.Application.UseCases;

public class CreateBatchHandler : ICreateBatchHandler
{
    private readonly ICalculationFactory _calculationFactory;
    private readonly IBatchRepository _batchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateBatchHandler(
        ICalculationFactory calculationFactory,
        IBatchRepository batchRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateBatchHandler> logger)
    {
        _calculationFactory = calculationFactory;
        _batchRepository = batchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> HandleAsync(CreateBatchCommand command)
    {
        var batch = _calculationFactory.Create(command.ProcessType, command.GridAreaCodes, command.StartDate, command.EndDate, command.CreatedByUserId);
        await _batchRepository.AddAsync(batch).ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);

        _logger.LogInformation("Calculation created with id {calculation_id}", batch.Id);
        return batch.Id;
    }
}
