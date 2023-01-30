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

using Energinet.DataHub.Wholesale.Domain.Actor;
using Energinet.DataHub.Wholesale.Infrastructure.BatchActor;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Infrastructure.BatchActor;

[UnitTest]
public class MarketRoleMapperTests
{
    [Theory]
    [InlineData(MarketRole.EnergySupplier, "energy_supplier")]
    public void WhenMapIsCalled_ThenCorrectStringIsReturned(MarketRole role, string expected)
    {
        var marketRoles = Enum.GetValues(typeof(MarketRole)).Cast<MarketRole>();
        foreach (var marketRole in marketRoles)
        {
            var actual = MarketRoleMapper.Map(marketRole);
            if (marketRole == role)
                actual.Should().Be(expected);
        }
    }
}
