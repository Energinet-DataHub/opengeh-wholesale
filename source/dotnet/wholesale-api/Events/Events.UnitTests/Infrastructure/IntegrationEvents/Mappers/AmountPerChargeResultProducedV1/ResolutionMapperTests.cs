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
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers.AmountPerChargeResultProducedV1;
using FluentAssertions;
using Xunit;
using EventResolution = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.Resolution;
using ModelResolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.ChargeResolution;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Mappers.AmountPerChargeResultProducedV1;

public class ResolutionMapperTests
{
    [Theory]
    [InlineAutoMoqData(ModelResolution.Day, EventResolution.Day)]
    [InlineAutoMoqData(ModelResolution.Hour, EventResolution.Hour)]
    public void MapResolution_WhenCalled_MapsCorrectly(ModelResolution resolution, EventResolution expected)
    {
        // Act & Assert
        ResolutionMapper.MapResolution(resolution).Should().Be(expected);
    }

    [Fact]
    public void MapResolution_WhenResolutionIsNotMonth_MapsAnyValidValue()
    {
        foreach (var resolution in Enum.GetValues(typeof(ModelResolution)).Cast<ModelResolution>())
        {
            // Arrange
            if (resolution == ModelResolution.Month)
                continue;

            // Act
            var actual = ResolutionMapper.MapResolution(resolution);

            // Assert: Is defined (and implicitly that it didn't throw exception)
            Enum.IsDefined(typeof(EventResolution), actual).Should().BeTrue();
        }
    }

    [Fact]
    public void MapResolution_WhenInvalidEnum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (ModelResolution)99;

        // Act
        var act = () => ResolutionMapper.MapResolution(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }
}
