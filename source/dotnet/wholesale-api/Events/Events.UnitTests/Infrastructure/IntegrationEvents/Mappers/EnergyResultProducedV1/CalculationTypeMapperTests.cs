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
using ProcessType = Energinet.DataHub.Wholesale.Common.Models.ProcessType;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Mappers.EnergyResultProducedV1;

public class CalculationTypeMapperTests
{
    [Theory]
    [InlineAutoMoqData(ProcessType.Aggregation,  Contracts.IntegrationEvents.EnergyResultProducedV1.Types.CalculationType.Aggregation)]
    [InlineAutoMoqData(ProcessType.BalanceFixing,  Contracts.IntegrationEvents.EnergyResultProducedV1.Types.CalculationType.BalanceFixing)]
    [InlineAutoMoqData(ProcessType.WholesaleFixing,  Contracts.IntegrationEvents.EnergyResultProducedV1.Types.CalculationType.WholesaleFixing)]
    [InlineAutoMoqData(ProcessType.FirstCorrectionSettlement,  Contracts.IntegrationEvents.EnergyResultProducedV1.Types.CalculationType.FirstCorrectionSettlement)]
    [InlineAutoMoqData(ProcessType.SecondCorrectionSettlement,  Contracts.IntegrationEvents.EnergyResultProducedV1.Types.CalculationType.SecondCorrectionSettlement)]
    [InlineAutoMoqData(ProcessType.ThirdCorrectionSettlement,  Contracts.IntegrationEvents.EnergyResultProducedV1.Types.CalculationType.ThirdCorrectionSettlement)]
    public void MapCalculationType_WhenCalled_MapsCorrectly(ProcessType processType, Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV1.Types.CalculationType expected)
    {
        // Act & Assert
        CalculationTypeMapper.MapCalculationType(processType).Should().Be(expected);
    }

    [Fact]
    public void MapCalculationType_MapsAnyValidValue()
    {
        foreach (var processType in Enum.GetValues(typeof(ProcessType)).Cast<ProcessType>())
        {
            // Act
            var actual = CalculationTypeMapper.MapCalculationType(processType);

            // Assert: Is defined (and implicitly that it didn't throw exception)
            Enum.IsDefined(typeof(Contracts.IntegrationEvents.EnergyResultProducedV1.Types.CalculationType), actual).Should().BeTrue();
        }
    }
}
