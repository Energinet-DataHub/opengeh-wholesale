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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.ProcessStep.Model;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Application.ProcessStep.Model;

public class TimeSeriesTypeMapperTests
{
    [Theory]
    [InlineAutoMoqData(TimeSeriesType.Production, Interfaces.CalculationResultClient.TimeSeriesType.Production)]
    [InlineAutoMoqData(TimeSeriesType.FlexConsumption, Interfaces.CalculationResultClient.TimeSeriesType.FlexConsumption)]
    [InlineAutoMoqData(TimeSeriesType.NonProfiledConsumption, Interfaces.CalculationResultClient.TimeSeriesType.NonProfiledConsumption)]
    [InlineAutoMoqData(TimeSeriesType.NetExchangePerGridArea, Interfaces.CalculationResultClient.TimeSeriesType.NetExchangePerGridArea)]
    public void MapContractType_ReturnsExpectedDomainType(TimeSeriesType source, Interfaces.CalculationResultClient.TimeSeriesType expected)
    {
        var actual = TimeSeriesTypeMapper.Map(source);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineAutoMoqData(Interfaces.CalculationResultClient.TimeSeriesType.Production, TimeSeriesType.Production)]
    [InlineAutoMoqData(Interfaces.CalculationResultClient.TimeSeriesType.FlexConsumption, TimeSeriesType.FlexConsumption)]
    [InlineAutoMoqData(Interfaces.CalculationResultClient.TimeSeriesType.NonProfiledConsumption, TimeSeriesType.NonProfiledConsumption)]
    [InlineAutoMoqData(Interfaces.CalculationResultClient.TimeSeriesType.NetExchangePerGridArea, TimeSeriesType.NetExchangePerGridArea)]
    public void MapDomainType_ReturnsExpectedContractType(Interfaces.CalculationResultClient.TimeSeriesType source, TimeSeriesType expected)
    {
        var actual = TimeSeriesTypeMapper.Map(source);
        actual.Should().Be(expected);
    }
}
