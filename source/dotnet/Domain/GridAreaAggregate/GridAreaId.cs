namespace Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;

public sealed record GridAreaId(Guid Id)
{
    public GridAreaId() : this(Guid.NewGuid())
    {
    }
}
