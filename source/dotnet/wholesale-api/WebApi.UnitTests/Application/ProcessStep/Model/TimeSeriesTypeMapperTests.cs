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
using Energinet.DataHub.Wholesale.Application.ProcessStep.Model;
using Energinet.DataHub.Wholesale.Contracts;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Application.ProcessStep.Model;

public class TimeSeriesTypeMapperTests
{
    [Theory]
    [InlineAutoMoqData(TimeSeriesType.Production, CalculationResults.Interfaces.TimeSeriesType.Production)]
    [InlineAutoMoqData(TimeSeriesType.FlexConsumption, CalculationResults.Interfaces.TimeSeriesType.FlexConsumption)]
    [InlineAutoMoqData(TimeSeriesType.NonProfiledConsumption, CalculationResults.Interfaces.TimeSeriesType.NonProfiledConsumption)]
    public void MapContractType_ReturnsExpectedDomainType(TimeSeriesType source, CalculationResults.Interfaces.TimeSeriesType expected)
    {
        var actual = TimeSeriesTypeMapper.Map(source);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineAutoMoqData(CalculationResults.Interfaces.TimeSeriesType.Production, TimeSeriesType.Production)]
    [InlineAutoMoqData(CalculationResults.Interfaces.TimeSeriesType.FlexConsumption, TimeSeriesType.FlexConsumption)]
    [InlineAutoMoqData(CalculationResults.Interfaces.TimeSeriesType.NonProfiledConsumption, TimeSeriesType.NonProfiledConsumption)]
    public void MapDomainType_ReturnsExpectedContractType(CalculationResults.Interfaces.TimeSeriesType source, TimeSeriesType expected)
    {
        var actual = TimeSeriesTypeMapper.Map(source);
        actual.Should().Be(expected);
    }
}
