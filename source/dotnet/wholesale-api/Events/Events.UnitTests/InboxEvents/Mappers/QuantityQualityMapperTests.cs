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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Events.Infrastructure.InboxEvents.Mappers;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.InboxEvents.Mappers;

public class QuantityQualityMapperTests
{
    [Theory]
    [InlineData(QuantityQuality.Calculated, Edi.Responses.QuantityQuality.Calculated)]
    [InlineData(QuantityQuality.Missing, Edi.Responses.QuantityQuality.Missing)]
    [InlineData(QuantityQuality.Measured, Edi.Responses.QuantityQuality.Measured)]
    [InlineData(QuantityQuality.Estimated, Edi.Responses.QuantityQuality.Estimated)]
    [InlineData(QuantityQuality.Incomplete, Edi.Responses.QuantityQuality.Incomplete)]
    public void MapQuantityQuality_WhenCalled_MapsCorrectly(
        QuantityQuality quantityQuality,
        Edi.Responses.QuantityQuality expected)
    {
        // Act & Assert
        QuantityQualityMapper.MapQuantityQuality(quantityQuality).Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(QuantityQualitys))]
    public void MapQuantityQuality_WhenCalled_HandlesExpectedType(QuantityQuality quantityQuality)
    {
        // Act
        var actual = () => QuantityQualityMapper.MapQuantityQuality(quantityQuality);

        // Assert
        actual.Should().NotThrow();
    }

    public static IEnumerable<object[]> QuantityQualitys()
    {
        foreach (var quantityQuality in Enum.GetValues(typeof(QuantityQuality)))
        {
            yield return new[] { quantityQuality };
        }
    }
}
