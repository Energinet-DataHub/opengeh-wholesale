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

using Energinet.DataHub.Core.TestCommon.FluentAssertionsExtensions;
using Energinet.DataHub.Wholesale.Application.Infrastructure;
using Energinet.DataHub.Wholesale.Application.ProcessResult;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Application;

[UnitTest]
public class ProcessResultApplicationServiceTests
{
    private const string GridAreaCode = "805";

    [Fact]
    public async Task GetResultAsync_WhenCalled_ReturnsProcessResultDtoFromJson()
    {
        // Arrange
        var sut = ProcessResultApplicationService();

        // Act
        var actual = await sut.GetResultAsync(
            new ProcessStepResultRequestDto(
                Guid.NewGuid(),
                GridAreaCode,
                ProcessStepType.AggregateProductionPerGridArea));

        actual.Should().NotContainNullsOrEmptyEnumerables();
    }

    [Fact]
    public async Task GetResultAsync_Max_IsMaxQuantity()
    {
        // Arrange
        var sut = ProcessResultApplicationService();

        // Act
        var actual = await sut.GetResultAsync(
            new ProcessStepResultRequestDto(
                Guid.NewGuid(),
                GridAreaCode,
                ProcessStepType.AggregateProductionPerGridArea));

        actual.Should().NotBeNull();

        actual.Max.Should().Be(actual.TimeSeriesPoints.Max(x => x.Quantity));
    }

    [Fact]
    public async Task GetResultAsync_Min_IsMinQuantity()
    {
        // Arrange
        var sut = ProcessResultApplicationService();

        // Act
        var actual = await sut.GetResultAsync(
            new ProcessStepResultRequestDto(
                Guid.NewGuid(),
                GridAreaCode,
                ProcessStepType.AggregateProductionPerGridArea));

        actual.Should().NotBeNull();

        actual.Min.Should().Be(actual.TimeSeriesPoints.Min(x => x.Quantity));
    }

    [Fact]
    public async Task GetResultAsync_Sum_IsSumQuantity()
    {
        // Arrange
        var sut = ProcessResultApplicationService();

        // Act
        var actual = await sut.GetResultAsync(
            new ProcessStepResultRequestDto(
                Guid.NewGuid(),
                GridAreaCode,
                ProcessStepType.AggregateProductionPerGridArea));

        actual.Should().NotBeNull();

        actual.Sum.Should().Be(actual.TimeSeriesPoints.Sum(x => x.Quantity));
    }

    private static ProcessStepResultApplicationService ProcessResultApplicationService()
    {
        var mock = new Mock<IBatchFileManager>();
        var stream = EmbeddedResources.GetStream("Application.ProcessResult.json");
        mock.Setup(x => x.GetResultFileStreamAsync(It.IsAny<Guid>(), It.IsAny<GridAreaCode>()))
            .ReturnsAsync(stream);

        var sut = new ProcessStepResultApplicationService(mock.Object);
        return sut;
    }
}
