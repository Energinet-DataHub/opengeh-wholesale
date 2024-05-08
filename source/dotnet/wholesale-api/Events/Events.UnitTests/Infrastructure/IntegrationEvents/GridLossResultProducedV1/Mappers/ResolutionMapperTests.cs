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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using FluentAssertions;
using Xunit;
using EnergyResultProduced = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.GridLossResultProducedV1;
using ResolutionMapper = Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Mappers.ResolutionMapper;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Mappers;

public class ResolutionMapperTests
{
    [Theory]
    [InlineAutoMoqData(Resolution.Quarter, EnergyResultProduced.Types.Resolution.Quarter)]
    [InlineAutoMoqData(Resolution.Hour, EnergyResultProduced.Types.Resolution.Hour)]
    public void MapResolution_WhenCalled_MapsCorrectly(Resolution resolution, EnergyResultProduced.Types.Resolution expected)
    {
        // Act & Assert
        var actual = ResolutionMapper.MapResolution(resolution);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void MapResolution_WhenInvalidEnumNumberForResolution_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (Resolution)99;

        // Act
        var act = () => ResolutionMapper.MapResolution(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }
}
