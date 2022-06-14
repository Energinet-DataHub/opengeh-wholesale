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
