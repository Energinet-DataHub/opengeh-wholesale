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

using Energinet.DataHub.Wholesale.IntegrationEventListener.MeteringPoints;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Application.MeteringPoints;

[UnitTest]
public class MeteringPointTypeTests
{
    [Theory]
    [InlineData(0, nameof(MeteringPointType.Unknown))]
    [InlineData(1, nameof(MeteringPointType.Consumption))]
    [InlineData(2, nameof(MeteringPointType.Production))]
    [InlineData(3, nameof(MeteringPointType.Exchange))]
    [InlineData(4, nameof(MeteringPointType.VeProduction))]
    [InlineData(5, nameof(MeteringPointType.Analysis))]
    [InlineData(6, nameof(MeteringPointType.SurplusProductionGroup))]
    [InlineData(7, nameof(MeteringPointType.NetProduction))]
    [InlineData(8, nameof(MeteringPointType.SupplyToGrid))]
    [InlineData(9, nameof(MeteringPointType.ConsumptionFromGrid))]
    [InlineData(10, nameof(MeteringPointType.WholesaleService))]
    [InlineData(11, nameof(MeteringPointType.OwnProduction))]
    [InlineData(12, nameof(MeteringPointType.NetFromGrid))]
    [InlineData(13, nameof(MeteringPointType.NetToGrid))]
    [InlineData(14, nameof(MeteringPointType.TotalConsumption))]
    [InlineData(15, nameof(MeteringPointType.GridLossCorrection))]
    [InlineData(16, nameof(MeteringPointType.ElectricalHeating))]
    [InlineData(17, nameof(MeteringPointType.NetConsumption))]
    [InlineData(18, nameof(MeteringPointType.OtherConsumption))]
    [InlineData(19, nameof(MeteringPointType.OtherProduction))]
    [InlineData(20, nameof(MeteringPointType.ExchangeReactiveEnergy))]
    [InlineData(21, nameof(MeteringPointType.InternalUse))]
    public void Value_IsDefinedCorrectly(int expected, string name)
    {
        // Arrange
        var actual = Enum.Parse<MeteringPointType>(name);

        // Assert
        actual.Should().Be((MeteringPointType)expected);
    }
}
