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

using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Tests.Domain.ProcessStepResultAggregate;

public class ProcessStepResultAggregateTests
{
    [Fact]
    public void Ctor_WhenNoPoints_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ProcessStepResult(new TimeSeriesPoint[] { }));
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void CalculatedProperties_ReturnsExpectedValue(List<TimeSeriesPoint> points, decimal expectedMin, decimal expectedMax, decimal expectedSum)
    {
        var sut = new ProcessStepResult(points.ToArray());
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
                    new(DateTimeOffset.Now, 1m, string.Empty),
                },
                1m,
                1m,
                1m,
            },
            new object[]
            {
                new List<TimeSeriesPoint>
                {
                    new(DateTimeOffset.Now, 7m, string.Empty),
                    new(DateTimeOffset.Now, 1m, string.Empty),
                },
                1m,
                7m,
                8m,
            },
        };
}
