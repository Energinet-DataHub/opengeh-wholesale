using Energinet.DataHub.Contracts.WholesaleProcess;

namespace Energinet.DataHub.Wholesale.Domain.BatchAggregate;

public class Batch
{
    public WholesaleProcessType ProcessType { get; }
    public List<Guid> GridAreas { get; }

    public Batch(WholesaleProcessType processType, List<Guid> gridAreas)
    {
        ProcessType = processType;
        GridAreas = gridAreas;
    }
}
