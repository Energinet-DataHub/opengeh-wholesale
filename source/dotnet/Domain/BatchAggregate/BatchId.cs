namespace Energinet.DataHub.Wholesale.Domain.BatchAggregate;

public sealed record BatchId(Guid Id)
{
    public BatchId() : this(Guid.NewGuid())
    {
    }
}
