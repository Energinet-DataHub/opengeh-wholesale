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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResultClient.Mappers;

[UnitTest]
public class MeteringPointTypeMapperTests
{
    [Theory]
    [InlineData("flex_consumption", MeteringPointType.Consumption)]
    [InlineData("production", MeteringPointType.Production)]
    [InlineData("non_profiled_consumption", MeteringPointType.Consumption)]
    public void ToDeltaTable_ReturnsExpectedString(string timeSeriesType, MeteringPointType expected)
    {
        // Act
        var actual = MeteringPointTypeMapper.FromDeltaTable(timeSeriesType);

        // Assert
        actual.Should().Be(expected);
    }
}
