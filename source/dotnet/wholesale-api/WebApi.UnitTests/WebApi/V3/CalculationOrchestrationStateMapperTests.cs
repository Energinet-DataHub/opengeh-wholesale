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

using Energinet.DataHub.Wholesale.WebApi.V3.Calculation;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.WebApi.V3;

public class CalculationOrchestrationStateMapperTests
{
    public static IEnumerable<object[]> AllCalculationOrchestrationStateInputs()
    {
        var allValues = Enum
            .GetValues<Common.Interfaces.Models.CalculationOrchestrationState>()
            .Select(s => new object[] { s })
            .ToArray();

        return allValues;
    }

    public static IReadOnlyCollection<CalculationOrchestrationState> AllCalculationOrchestrationStateOutputs()
    {
        var allValues = Enum
            .GetValues<CalculationOrchestrationState>()
            .ToList();

        return allValues;
    }

    [Theory]
    [MemberData(nameof(AllCalculationOrchestrationStateInputs))]
    public void MapState_WhenCalledWithAllPossibleValues_ReturnsValidValue(Common.Interfaces.Models.CalculationOrchestrationState input)
    {
        // Arrange
        var expectedValues = AllCalculationOrchestrationStateOutputs();

        // Act
        var act = () => CalculationOrchestrationStateMapper.MapState(input);

        // Assert
        var actualValue = act.Should().NotThrow().Subject;

        actualValue.Should().BeOneOf(expectedValues);
    }
}
