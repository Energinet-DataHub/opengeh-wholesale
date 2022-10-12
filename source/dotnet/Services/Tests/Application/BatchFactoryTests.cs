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
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using FluentAssertions;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Application;

[UnitTest]
public class BatchFactoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public void Create_ReturnsBatchWithCorrectPeriod(BatchFactory sut)
    {
        // Arrange
        var batchRequestDto = CreateBatchRequestDto();

        // Act
        var batch = sut.Create(batchRequestDto);

        // Assert
        batch.PeriodStart.Should().Be(Instant.FromDateTimeOffset(batchRequestDto.StartDate));
        batch.PeriodEnd.Should().Be(Instant.FromDateTimeOffset(batchRequestDto.EndDate));
    }

    [Theory]
    [InlineAutoMoqData]
    public void Create_ReturnsBatchWithCorrectGridAreas(BatchFactory sut)
    {
        // Arrange
        var batchRequestDto = CreateBatchRequestDto();

        // Act
        var batch = sut.Create(batchRequestDto);

        // Assert
        batch.GridAreaCodes.Select(x => x.Code).Should().Contain(batchRequestDto.GridAreaCodes);
        batch.GridAreaCodes.Count.Should().Be(batchRequestDto.GridAreaCodes.Count());
    }

    private BatchRequestDto CreateBatchRequestDto()
    {
        var startDate = new DateTimeOffset(2022, 5, 1, 8, 6, 32, TimeSpan.Zero);
        var endDate = new DateTimeOffset(2022, 5, 5, 8, 6, 32, TimeSpan.Zero);
        var someGridAreasIds = new List<string> { "004", "805" };
        return new BatchRequestDto(WholesaleProcessType.BalanceFixing, someGridAreasIds, startDate, endDate);
    }
}
