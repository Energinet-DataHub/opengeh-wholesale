using Energinet.DataHub.Contracts.WholesaleProcess;

namespace Energinet.DataHub.Wholesale.Application;

public interface IBatchApplicationService
{
    Task CreateAsync(WholesaleProcessType processType, List<Guid> gridAreas);
}
