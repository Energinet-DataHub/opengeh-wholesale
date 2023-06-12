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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.ProcessStepResultAggregate;

public class ProcessStepResultAggregateTests
{
    [Theory]
    [InlineAutoMoqData]
    public void Ctor_WhenNoPoints_ThrowsArgumentException(TimeSeriesType anyTimeSeriesType)
    {
        var emptyTimeSeriesPoints = new TimeSeriesPoint[] { };
        Assert.Throws<ArgumentException>(() => new CalculationResult(anyTimeSeriesType, emptyTimeSeriesPoints));
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void CalculatedProperties_ReturnsExpectedValue(List<TimeSeriesPoint> points, decimal expectedMin, decimal expectedMax, decimal expectedSum)
    {
        // Arrange
        var anyTimeSeriesType = TimeSeriesType.Production;

        // Act
        var sut = new CalculationResult(anyTimeSeriesType, points.ToArray());

        // Assert
        sut.Min.Should().Be(expectedMin);
        sut.Max.Should().Be(expectedMax);
        sut.Sum.Should().Be(expectedSum);
    }

    public static IEnumerable<object[]> Data =>
        new List<object[]>
        {
            new object[]
            {
                new List<TimeSeriesPoint>
                {
                    new(DateTimeOffset.Now, 1m, QuantityQuality.Missing),
                },
                1m,
                1m,
                1m,
            },
            new object[]
            {
                new List<TimeSeriesPoint>
                {
                    new(DateTimeOffset.Now, 7m, QuantityQuality.Missing),
                    new(DateTimeOffset.Now, 1m, QuantityQuality.Missing),
                },
                1m,
                7m,
                8m,
            },
        };
}
