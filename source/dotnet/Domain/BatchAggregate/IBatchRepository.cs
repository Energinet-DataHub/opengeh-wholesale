namespace Energinet.DataHub.Wholesale.Domain.BatchAggregate;

public interface IBatchRepository
{
    Task AddAsync(Batch batch);
}
