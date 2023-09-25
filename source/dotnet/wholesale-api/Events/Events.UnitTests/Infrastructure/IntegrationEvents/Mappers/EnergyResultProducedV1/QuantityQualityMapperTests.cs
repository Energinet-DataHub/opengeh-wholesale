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
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers.EnergyResultProducedV1;
using FluentAssertions;
using Xunit;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Mappers.EnergyResultProducedV1;

public class QuantityQualityMapperTests
{
    [Theory]
    [InlineAutoMoqData(QuantityQuality.Estimated, Contracts.IntegrationEvents.EnergyResultProducedV1.Types.QuantityQuality.Estimated)]
    [InlineAutoMoqData(QuantityQuality.Incomplete, Contracts.IntegrationEvents.EnergyResultProducedV1.Types.QuantityQuality.Incomplete)]
    [InlineAutoMoqData(QuantityQuality.Measured, Contracts.IntegrationEvents.EnergyResultProducedV1.Types.QuantityQuality.Measured)]
    [InlineAutoMoqData(QuantityQuality.Missing, Contracts.IntegrationEvents.EnergyResultProducedV1.Types.QuantityQuality.Missing)]
    [InlineAutoMoqData(QuantityQuality.Calculated, Contracts.IntegrationEvents.EnergyResultProducedV1.Types.QuantityQuality.Calculated)]
    public void MapQuantityQuality_WhenCalled_MapsCorrectly(QuantityQuality quantityQuality, Contracts.IntegrationEvents.EnergyResultProducedV1.Types.QuantityQuality expected)
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
            Enum.IsDefined(typeof(Contracts.IntegrationEvents.EnergyResultProducedV1.Types.QuantityQuality), actual).Should().BeTrue();
        }
    }
}
