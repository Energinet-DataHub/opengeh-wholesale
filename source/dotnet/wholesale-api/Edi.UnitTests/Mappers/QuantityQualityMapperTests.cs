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

using Energinet.DataHub.Wholesale.Edi.Mappers;
using FluentAssertions;
using Xunit;
using EdiModel = Energinet.DataHub.Edi.Responses;
using WholesaleModel = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Mappers;

public class QuantityQualityMapperTests
{
    [Theory]
    [InlineData(WholesaleModel.QuantityQuality.Estimated, EdiModel.QuantityQuality.Estimated)]
    [InlineData(WholesaleModel.QuantityQuality.Measured, EdiModel.QuantityQuality.Measured)]
    [InlineData(WholesaleModel.QuantityQuality.Calculated, EdiModel.QuantityQuality.Calculated)]
    [InlineData(WholesaleModel.QuantityQuality.Missing, EdiModel.QuantityQuality.Missing)]
    public void ToDeltaTableValue_ReturnsExpectedString(
        WholesaleModel.QuantityQuality type,
        EdiModel.QuantityQuality expectedType)
    {
        // Act
        var actualType = QuantityQualityMapper.MapQuantityQuality(type);

        // Assert
        actualType.Should().Be(expectedType);
    }

    [Fact]
    public void MapQuantityQuality_WhenInvalidEnumNumberForQuantityQuality_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (WholesaleModel.QuantityQuality)99;

        // Act
        var act = () => QuantityQualityMapper.MapQuantityQuality(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }
}
