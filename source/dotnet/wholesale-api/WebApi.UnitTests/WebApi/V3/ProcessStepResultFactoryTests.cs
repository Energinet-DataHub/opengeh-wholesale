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
using Energinet.DataHub.Wholesale.WebApi.V3.Batch;
using Energinet.DataHub.Wholesale.WebApi.V3.ProcessStepResult;
using FluentAssertions;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.WebApi.V3;

public class ProcessStepResultFactoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public void Create_ReturnsExpectedStepResult(
        CalculationResult result,
        BatchDto batchDto)
    {
        // Arrange
        var point = result.TimeSeriesPoints.First();
        result.SetPrivateProperty(r => r.TimeSeriesPoints, new[] { point });
        var expected = new ProcessStepResultDto(
            result.Sum,
            result.Min,
            result.Max,
            batchDto.PeriodStart,
            batchDto.PeriodEnd,
            batchDto.Resolution,
            batchDto.Unit,
            new TimeSeriesPointDto[]
            {
                new(point.Time, point.Quantity, point.Quality.ToString()),
            },
            batchDto.ProcessType,
            result.TimeSeriesType);

        // Act
        var actual = ProcessStepResultFactory.Create(result, batchDto);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }
}
