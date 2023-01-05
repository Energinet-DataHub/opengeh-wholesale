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

using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using FluentAssertions;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Domain.BatchAggregate;

[UnitTest]
public class BatchFactoryTests
{
    private readonly DateTimeOffset _startDate = DateTimeOffset.Parse("2021-12-31T23:00Z");
    private readonly DateTimeOffset _endDate = DateTimeOffset.Parse("2022-01-31T22:59:59.999Z");
    private readonly List<string> _someGridAreasIds = new() { "004", "805" };

    [Theory]
    [InlineAutoMoqData]
    public void Create_ReturnsBatchWithCorrectPeriod(BatchFactory sut)
    {
        // Act
        var batch = sut.Create(ProcessType.BalanceFixing, _someGridAreasIds, _startDate, _endDate);

        // Assert
        batch.PeriodStart.Should().Be(Instant.FromDateTimeOffset(_startDate));
        batch.PeriodEnd.Should().Be(Instant.FromDateTimeOffset(_endDate));
    }

    [Theory]
    [InlineAutoMoqData]
    public void Create_ReturnsBatchWithCorrectGridAreas(BatchFactory sut)
    {
        // Act
        var batch = sut.Create(ProcessType.BalanceFixing, _someGridAreasIds, _startDate, _endDate);

        // Assert
        batch.GridAreaCodes.Select(x => x.Code).Should().Contain(_someGridAreasIds);
        batch.GridAreaCodes.Count.Should().Be(_someGridAreasIds.Count);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Create_ReturnsBatchWithExpectedExecutionTimeStart([Frozen] Mock<IClock> clockMock, BatchFactory sut)
    {
        // Arrange
        var expected = SystemClock.Instance.GetCurrentInstant();
        clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(expected);

        // Act
        var batch = sut.Create(ProcessType.BalanceFixing, _someGridAreasIds, _startDate, _endDate);

        // Assert
        batch.ExecutionTimeStart.Should().Be(expected);
    }
}
