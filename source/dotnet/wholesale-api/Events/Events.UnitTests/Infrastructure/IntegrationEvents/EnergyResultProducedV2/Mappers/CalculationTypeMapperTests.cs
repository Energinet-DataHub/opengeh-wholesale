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
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EnergyResultProducedV2.Mappers;
using FluentAssertions;
using Xunit;
using EnergyResultProduced = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV2;
using ProcessType = Energinet.DataHub.Wholesale.Common.Interfaces.Models.ProcessType;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.EnergyResultProducedV2.Mappers;

public class CalculationTypeMapperTests
{
    [Theory]
    [InlineAutoMoqData(ProcessType.Aggregation, Contracts.IntegrationEvents.EnergyResultProducedV2.Types.CalculationType.Aggregation)]
    [InlineAutoMoqData(ProcessType.BalanceFixing, EnergyResultProduced.Types.CalculationType.BalanceFixing)]
    [InlineAutoMoqData(ProcessType.WholesaleFixing, EnergyResultProduced.Types.CalculationType.WholesaleFixing)]
    [InlineAutoMoqData(ProcessType.FirstCorrectionSettlement, EnergyResultProduced.Types.CalculationType.FirstCorrectionSettlement)]
    [InlineAutoMoqData(ProcessType.SecondCorrectionSettlement, EnergyResultProduced.Types.CalculationType.SecondCorrectionSettlement)]
    [InlineAutoMoqData(ProcessType.ThirdCorrectionSettlement, EnergyResultProduced.Types.CalculationType.ThirdCorrectionSettlement)]
    public void MapCalculationType_WhenCalled_MapsCorrectly(ProcessType processType, EnergyResultProduced.Types.CalculationType expected)
    {
        // Arrange & Act
        var actual = CalculationTypeMapper.MapCalculationType(processType);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void MapCalculationType_MapsAnyValidValue()
    {
        foreach (var processType in Enum.GetValues(typeof(ProcessType)).Cast<ProcessType>())
        {
            // Act
            var actual = CalculationTypeMapper.MapCalculationType(processType);

            // Assert: Is defined (and implicitly that it didn't throw exception)
            Enum.IsDefined(typeof(EnergyResultProduced.Types.CalculationType), actual).Should().BeTrue();
        }
    }

    [Fact]
    public void MapCalculationType_WhenInvalidEnumNumberForCalculationType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (ProcessType)99;

        // Act
        var act = () => CalculationTypeMapper.MapCalculationType(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }
}
