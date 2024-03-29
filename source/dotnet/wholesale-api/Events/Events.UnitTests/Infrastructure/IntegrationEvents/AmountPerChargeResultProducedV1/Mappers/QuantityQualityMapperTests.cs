﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.AmountPerChargeResultProducedV1.Mappers;
using FluentAssertions;
using Xunit;
using EventQuantityQuality = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.QuantityQuality;
using ModelQuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.AmountPerChargeResultProducedV1.Mappers;

public class QuantityQualityMapperTests
{
    [Theory]
    [InlineData(ModelQuantityQuality.Estimated, EventQuantityQuality.Estimated)]
    [InlineData(ModelQuantityQuality.Measured, EventQuantityQuality.Measured)]
    [InlineData(ModelQuantityQuality.Missing, EventQuantityQuality.Missing)]
    [InlineData(ModelQuantityQuality.Calculated, EventQuantityQuality.Calculated)]
    public void MapQuantityQuality_WhenCalled_MapsCorrectly(ModelQuantityQuality quantityQuality, EventQuantityQuality expected)
    {
        // Act & Assert
        QuantityQualityMapper.MapQuantityQuality(quantityQuality).Should().Be(expected);
    }

    [Fact]
    public void MapQuantityQuality_MapsAnyValidValue()
    {
        foreach (var quality in Enum.GetValues(typeof(ModelQuantityQuality)).Cast<ModelQuantityQuality>())
        {
            // Act
            var actual = QuantityQualityMapper.MapQuantityQuality(quality);

            // Assert: Is defined (and implicitly that it didn't throw exception)
            Enum.IsDefined(typeof(EventQuantityQuality), actual).Should().BeTrue();
        }
    }

    [Fact]
    public void MapQuantityQuality_WhenInvalidEnumNumber_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (ModelQuantityQuality)99;

        // Act
        var act = () => QuantityQualityMapper.MapQuantityQuality(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }
}
