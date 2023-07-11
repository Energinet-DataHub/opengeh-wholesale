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
using Energinet.DataHub.Wholesale.WebApi.V3.Calculation;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.WebApi.V3;

public static class CalculationStateMapperTests
{
    [Theory]
    [InlineAutoMoqData(CalculationState.Failed, CalculationState.Failed)]
    [InlineAutoMoqData(CalculationState.Completed, CalculationState.Completed)]
    [InlineAutoMoqData(CalculationState.Executing, CalculationState.Executing)]
    [InlineAutoMoqData(CalculationState.Pending, CalculationState.Pending)]
    public static void Map_ReturnsExpectedTypeForWebApi(CalculationState source, CalculationState expected)
    {
        var actual = CalculationStateMapper.MapState(source);
        actual.Should().Be((Calculations.Interfaces.Models.CalculationState)expected);
    }

    [Theory]
    [InlineAutoMoqData(CalculationState.Failed, Calculations.Interfaces.Models.CalculationState.Failed)]
    [InlineAutoMoqData(CalculationState.Completed, Calculations.Interfaces.Models.CalculationState.Completed)]
    [InlineAutoMoqData(CalculationState.Executing, Calculations.Interfaces.Models.CalculationState.Executing)]
    [InlineAutoMoqData(CalculationState.Pending, Calculations.Interfaces.Models.CalculationState.Pending)]
    public static void Map_ReturnsExpectedTypeForCalculationModule(CalculationState source, Calculations.Interfaces.Models.CalculationState expected)
    {
        var actual = CalculationStateMapper.MapState(source);
        actual.Should().Be(expected);
    }
}
