using Energinet.DataHub.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;

namespace Energinet.DataHub.Wholesale.Application;

public class BatchApplicationService : IBatchApplicationService
{
    private readonly IBatchRepository _batchRepository;

    public BatchApplicationService(IBatchRepository batchRepository)
    {
        _batchRepository = batchRepository;
    }

    public async Task CreateAsync(WholesaleProcessType processType, List<Guid> gridAreas)
    {
        var batch = new Batch(processType, gridAreas);
        await _batchRepository.AddAsync(batch);
    }
}
