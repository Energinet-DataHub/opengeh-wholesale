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
using Energinet.DataHub.Wholesale.Application.ProcessStep.Model;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using FluentAssertions;
using Test.Core;
using Xunit;
using TimeSeriesType = Energinet.DataHub.Wholesale.Contracts.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Application.ProcessStep.Model;

public class ProcessStepMapperTests
{
    [Theory]
    [InlineAutoMoqData]
    public void MapToDto_WhenNull_ThrowsArgumentNullException(ProcessStepResultMapper sut)
    {
        Assert.Throws<ArgumentNullException>(() => sut.MapToDto(null!));
    }

    [Theory]
    [InlineAutoMoqData]
    public void MapToDto_ReturnsDto(ProcessStepResultMapper sut, ProcessStepResult processStepResult)
    {
        // Arrange
        processStepResult.SetPrivateProperty(p => p.TimeSeriesType, Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType.Production);
        var expected = new ProcessStepResultDto(
            TimeSeriesType.Production,
            processStepResult.Sum,
            processStepResult.Min,
            processStepResult.Max,
            processStepResult.TimeSeriesPoints.Select(point => new TimeSeriesPointDto(point.Time, point.Quantity, point.Quality.ToString())).ToArray());

        // Act
        var actual = sut.MapToDto(processStepResult);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }
}
