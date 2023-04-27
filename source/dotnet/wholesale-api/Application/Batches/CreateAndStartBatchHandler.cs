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

using Energinet.DataHub.Wholesale.Application.Base;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.CalculationDomainService;

namespace Energinet.DataHub.Wholesale.Application.Batches;

public class CreateAndStartBatchHandler : ICommandHandler<CreateAndStartBatchCommand, Guid>
{
    private readonly IBatchFactory _batchFactory;
    private readonly IBatchRepository _batchRepository;
    private readonly IProcessTypeMapper _processTypeMapper;
    private readonly ICalculationDomainService _calculationDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAndStartBatchHandler(
        IBatchFactory batchFactory,
        IBatchRepository batchRepository,
        IProcessTypeMapper processTypeMapper,
        ICalculationDomainService calculationDomainService,
        IUnitOfWork unitOfWork)
    {
        _batchFactory = batchFactory;
        _batchRepository = batchRepository;
        _processTypeMapper = processTypeMapper;
        _calculationDomainService = calculationDomainService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateAndStartBatchCommand command, CancellationToken cancellationToken)
    {
        var processType = _processTypeMapper.MapFrom(command.ProcessType);
        var batch = _batchFactory.Create(processType, command.GridAreaCodes, command.StartDate, command.EndDate);
        await _batchRepository.AddAsync(batch).ConfigureAwait(false);
        await _calculationDomainService.StartAsync(batch.Id).ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
        return batch.Id;
    }
}
