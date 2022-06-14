using Energinet.DataHub.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Tests.Domain.BatchAggregate;

public class BatchTests
{
    [Fact]
    public void Ctor_CreatesImmutableGridAreaIds()
    {
        // Arrange
        var gridAreaIds = new List<GridAreaId> { new(), new() };
        var sut = new Batch(WholesaleProcessType.BalanceFixing, gridAreaIds);

        // Act
        var unexpectedGridAreaId = new GridAreaId();
        gridAreaIds.Add(unexpectedGridAreaId);

        // Assert
        sut.GridAreaIds.Should().NotContain(unexpectedGridAreaId);
    }
}
