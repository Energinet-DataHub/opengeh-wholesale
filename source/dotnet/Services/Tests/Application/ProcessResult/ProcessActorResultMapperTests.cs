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
using Energinet.DataHub.Wholesale.Application.ProcessResult;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.ProcessOutput;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Tests.Application.ProcessResult;

public class ProcessActorResultMapperTests
{
    [Theory]
    [InlineAutoMoqData]
    public void MapToDto_WhenNull_ThrowsArgumentNullException(ProcessActorResultMapper sut)
    {
        Assert.Throws<ArgumentNullException>(() => sut.MapToDto(null!));
    }

    [Theory]
    [InlineAutoMoqData]
    public void MapToDto_ReturnsDto(ProcessActorResultMapper sut, ProcessActorResult processActorResult)
    {
        var expected = new ProcessStepResultDto(
            ProcessStepMeteringPointType.Production,
            processActorResult.Sum,
            processActorResult.Min,
            processActorResult.Max,
            processActorResult.TimeSeriesPoints.Select(point => new TimeSeriesPointDto(point.Time, point.Quantity, point.Quality)).ToArray());
        var actual = sut.MapToDto(processActorResult);
        actual.Should().BeEquivalentTo(expected);
    }
}
