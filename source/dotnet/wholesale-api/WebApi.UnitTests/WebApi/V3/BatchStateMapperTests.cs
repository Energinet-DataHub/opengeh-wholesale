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
using Energinet.DataHub.Wholesale.WebApi.V3.Batch;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.WebApi.V3;

public static class BatchStateMapperTests
{
    [Theory]
    [InlineAutoMoqData(Calculations.Interfaces.Models.CalculationState.Failed, BatchState.Failed)]
    [InlineAutoMoqData(Calculations.Interfaces.Models.CalculationState.Completed, BatchState.Completed)]
    [InlineAutoMoqData(Calculations.Interfaces.Models.CalculationState.Executing, BatchState.Executing)]
    [InlineAutoMoqData(Calculations.Interfaces.Models.CalculationState.Pending, BatchState.Pending)]
    public static void Map_ReturnsExpectedTypeForWebApi(Calculations.Interfaces.Models.CalculationState source, BatchState expected)
    {
        var actual = BatchStateMapper.MapState(source);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineAutoMoqData(BatchState.Failed, Calculations.Interfaces.Models.CalculationState.Failed)]
    [InlineAutoMoqData(BatchState.Completed, Calculations.Interfaces.Models.CalculationState.Completed)]
    [InlineAutoMoqData(BatchState.Executing, Calculations.Interfaces.Models.CalculationState.Executing)]
    [InlineAutoMoqData(BatchState.Pending, Calculations.Interfaces.Models.CalculationState.Pending)]
    public static void Map_ReturnsExpectedTypeForBatchModule(BatchState source, Calculations.Interfaces.Models.CalculationState expected)
    {
        var actual = BatchStateMapper.MapState(source);
        actual.Should().Be(expected);
    }
}
