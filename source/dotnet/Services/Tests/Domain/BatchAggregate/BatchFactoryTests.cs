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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using FluentAssertions;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Domain.BatchAggregate;

[UnitTest]
public class BatchFactoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public void Create_ReturnsBatchWithCorrectPeriod(BatchFactory sut)
    {
        // Arrange
        var startDate = new DateTimeOffset(2022, 5, 1, 8, 6, 32, TimeSpan.Zero);
        var endDate = new DateTimeOffset(2022, 5, 5, 8, 6, 32, TimeSpan.Zero);
        var someGridAreasIds = new List<string> { "004", "805" };

        // Act
        var batch = sut.Create(ProcessType.BalanceFixing, someGridAreasIds, startDate, endDate);

        // Assert
        batch.PeriodStart.Should().Be(Instant.FromDateTimeOffset(startDate));
        batch.PeriodEnd.Should().Be(Instant.FromDateTimeOffset(endDate));
    }

    [Theory]
    [InlineAutoMoqData]
    public void Create_ReturnsBatchWithCorrectGridAreas(BatchFactory sut)
    {
        // Arrange
        var startDate = new DateTimeOffset(2022, 5, 1, 8, 6, 32, TimeSpan.Zero);
        var endDate = new DateTimeOffset(2022, 5, 5, 8, 6, 32, TimeSpan.Zero);
        var someGridAreasIds = new List<string> { "004", "805" };

        // Act
        var batch = sut.Create(ProcessType.BalanceFixing, someGridAreasIds, startDate, endDate);

        // Assert
        batch.GridAreaCodes.Select(x => x.Code).Should().Contain(someGridAreasIds);
        batch.GridAreaCodes.Count.Should().Be(someGridAreasIds.Count);
    }
}
