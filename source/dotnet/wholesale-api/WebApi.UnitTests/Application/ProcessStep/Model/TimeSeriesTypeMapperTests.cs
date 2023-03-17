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
    [InlineAutoMoqData(TimeSeriesType.Production, Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType.Production)]
    [InlineAutoMoqData(TimeSeriesType.FlexConsumption, Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType.FlexConsumption)]
    [InlineAutoMoqData(TimeSeriesType.NonProfiledConsumption, Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType.NonProfiledConsumption)]
    public void MapContractType_ReturnsExpectedDomainType(TimeSeriesType source, Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType expected)
    {
        var actual = TimeSeriesTypeMapper.Map(source);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineAutoMoqData(Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType.Production, TimeSeriesType.Production)]
    [InlineAutoMoqData(Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType.FlexConsumption, TimeSeriesType.FlexConsumption)]
    [InlineAutoMoqData(Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType.NonProfiledConsumption, TimeSeriesType.NonProfiledConsumption)]
    public void MapDomainType_ReturnsExpectedContractType(Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType source, TimeSeriesType expected)
    {
        var actual = TimeSeriesTypeMapper.Map(source);
        actual.Should().Be(expected);
    }
}
