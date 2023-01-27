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

using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.BatchActor;
using Energinet.DataHub.Wholesale.Infrastructure.Processes;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Infrastructure.Processes;

[UnitTest]
public class TimeSeriesTypeMapperTests
{
    [Theory]
    [InlineData(TimeSeriesType.FlexConsumption, "consumption")]
    [InlineData(TimeSeriesType.Production, "production")]
    [InlineData(TimeSeriesType.NonProfiledConsumption, "non_profiled_consumption")]
    public void WhenMapIsCalled_ThenCorrectStringIsReturned(TimeSeriesType type, string expected)
    {
        var marketRoleTypes = Enum.GetValues(typeof(TimeSeriesType)).Cast<TimeSeriesType>();
        foreach (var marketRoleType in marketRoleTypes)
        {
            var actual = TimeSeriesTypeMapper.Map(marketRoleType);
            if (marketRoleType == type)
                actual.Should().Be(expected);
        }
    }
}
