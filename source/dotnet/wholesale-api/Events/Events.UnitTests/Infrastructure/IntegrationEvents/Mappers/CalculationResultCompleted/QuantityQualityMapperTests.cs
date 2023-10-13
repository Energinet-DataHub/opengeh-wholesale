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
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers.CalculationResultCompleted;
using FluentAssertions;
using Xunit;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Mappers.CalculationResultCompleted;

public class QuantityQualityMapperTests
{
    [Theory]
    [InlineAutoMoqData(QuantityQuality.Estimated, Contracts.Events.QuantityQuality.Estimated)]
    [InlineAutoMoqData(QuantityQuality.Incomplete, Contracts.Events.QuantityQuality.Incomplete)]
    [InlineAutoMoqData(QuantityQuality.Measured, Contracts.Events.QuantityQuality.Measured)]
    [InlineAutoMoqData(QuantityQuality.Missing, Contracts.Events.QuantityQuality.Missing)]
    [InlineAutoMoqData(QuantityQuality.Calculated, Contracts.Events.QuantityQuality.Calculated)]
    public void MapQuantityQuality_WhenCalled_MapsCorrectly(QuantityQuality quantityQuality, Contracts.Events.QuantityQuality expected)
    {
        // Act & Assert
        QuantityQualityMapper.MapQuantityQuality(quantityQuality).Should().Be(expected);
    }

    [Fact]
    public void MapQuantityQuality_MapsAnyValidValue()
    {
        foreach (var quality in Enum.GetValues(typeof(QuantityQuality)).Cast<QuantityQuality>())
        {
            // Act
            var actual = QuantityQualityMapper.MapQuantityQuality(quality);

            // Assert: Is defined (and implicitly that it didn't throw exception)
            Enum.IsDefined(typeof(Contracts.Events.QuantityQuality), actual).Should().BeTrue();
        }
    }

    [Fact]
    public void MapQuantityQuality_WhenInvalidEnumNumberForQuantityQuality_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (QuantityQuality)99;

        // Act
        var act = () => QuantityQualityMapper.MapQuantityQuality(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }
}
