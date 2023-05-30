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
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Infrastructure.Integration;
using FluentAssertions;
using Xunit;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient.Model.QuantityQuality;

namespace Energinet.DataHub.Wholesale.IntegrationEventPublishing.UnitTests.Integration;

public class QuantityQualityMapperTests
{
    [Theory]
    [InlineAutoMoqData(QuantityQuality.Estimated, Wholesale.Contracts.Events.QuantityQuality.Estimated)]
    [InlineAutoMoqData(QuantityQuality.Incomplete, Wholesale.Contracts.Events.QuantityQuality.Incomplete)]
    [InlineAutoMoqData(QuantityQuality.Measured, Wholesale.Contracts.Events.QuantityQuality.Measured)]
    [InlineAutoMoqData(QuantityQuality.Missing, Wholesale.Contracts.Events.QuantityQuality.Missing)]
    public void MapQuantityQuality_WhenCalled_MapsCorrectly(QuantityQuality quantityQuality, Wholesale.Contracts.Events.QuantityQuality expected)
    {
        // Act & Assert
        QuantityQualityMapper.MapQuantityQuality(quantityQuality).Should().Be(expected);
    }

    [Fact]
    public void MapQuantityQuality_WhenCalledWithCalculated_ExceptionIsThrown()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => QuantityQualityMapper.MapQuantityQuality(QuantityQuality.Calculated));
    }
}
