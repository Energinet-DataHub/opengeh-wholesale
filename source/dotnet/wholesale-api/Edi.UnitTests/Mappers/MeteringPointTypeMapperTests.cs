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

using System.Reflection;
using Energinet.DataHub.Edi.Responses;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Edi.Mappers;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Mappers;

public class MeteringPointTypeMapperTests
{
    [Fact]
    public void Map_WhenValidMeteringPointType_ReturnsExpectedString()
    {
        // Arrange
        var meteringPointTypes = new List<(
            MeteringPointType GivenType,
            WholesaleServicesRequestSeries.Types.MeteringPointType ExpectedType)>
        {
            (MeteringPointType.Consumption, WholesaleServicesRequestSeries.Types.MeteringPointType.Consumption),
            (MeteringPointType.Production, WholesaleServicesRequestSeries.Types.MeteringPointType.Production),
            (MeteringPointType.VeProduction, WholesaleServicesRequestSeries.Types.MeteringPointType.VeProduction),
            (MeteringPointType.NetProduction, WholesaleServicesRequestSeries.Types.MeteringPointType.NetProduction),
            (MeteringPointType.SupplyToGrid, WholesaleServicesRequestSeries.Types.MeteringPointType.SupplyToGrid),
            (MeteringPointType.ConsumptionFromGrid, WholesaleServicesRequestSeries.Types.MeteringPointType.ConsumptionFromGrid),
            (MeteringPointType.WholesaleServicesInformation, WholesaleServicesRequestSeries.Types.MeteringPointType.WholesaleServicesInformation),
            (MeteringPointType.OwnProduction, WholesaleServicesRequestSeries.Types.MeteringPointType.OwnProduction),
            (MeteringPointType.NetFromGrid, WholesaleServicesRequestSeries.Types.MeteringPointType.NetFromGrid),
            (MeteringPointType.NetToGrid, WholesaleServicesRequestSeries.Types.MeteringPointType.NetToGrid),
            (MeteringPointType.TotalConsumption, WholesaleServicesRequestSeries.Types.MeteringPointType.TotalConsumption),
            (MeteringPointType.ElectricalHeating, WholesaleServicesRequestSeries.Types.MeteringPointType.ElectricalHeating),
            (MeteringPointType.NetConsumption, WholesaleServicesRequestSeries.Types.MeteringPointType.NetConsumption),
            (MeteringPointType.EffectSettlement, WholesaleServicesRequestSeries.Types.MeteringPointType.EffectSettlement),
        };

        // Act
        using var scope = new AssertionScope();
        foreach (var test in meteringPointTypes)
        {
            MeteringPointTypeMapper.Map(test.GivenType).Should().Be(test.ExpectedType);
        }

        // Ensure that we have tested all MeteringPointTypes
        meteringPointTypes.Select(c => c.GivenType)
            .Should().BeEquivalentTo(GetAllMeteringPointTypesExceptExchange());
    }

    [Fact]
    public void Map_WhenMeteringPointTypeIsExchange_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var meteringPointType = MeteringPointType.Exchange;

        // Act
        var act = () => MeteringPointTypeMapper.Map(meteringPointType);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>().And.ActualValue.Should().Be(meteringPointType);
    }

    private static IEnumerable<MeteringPointType> GetAllMeteringPointTypesExceptExchange()
    {
        var fields =
            typeof(MeteringPointType).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        return fields.Select(f => f.GetValue(null)).Cast<MeteringPointType>()
            .Where(meteringPointType => meteringPointType is not MeteringPointType.Exchange);
    }
}
