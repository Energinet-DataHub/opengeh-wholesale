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

using Energinet.DataHub.Wholesale.EDI.Mappers;
using Energinet.DataHub.Wholesale.EDI.Models;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Mappers;

public class TimeSeriesTypeMapperTests
{
    [Theory]
    [InlineData("E17", "", TimeSeriesType.TotalConsumption)]
    [InlineData("E17", null, TimeSeriesType.TotalConsumption)]
    [InlineData("E17", "E02", TimeSeriesType.NonProfiledConsumption)]
    [InlineData("E17", "D01", TimeSeriesType.FlexConsumption)]
    [InlineData("E18", null, TimeSeriesType.Production)]
    [InlineData("E20", null, TimeSeriesType.NetExchangePerGa)]
    public void MapTimeSeriesType_WithSettlementMethodAndMeteringPointType_returnsExpectedType(
        string meteringPointType,
        string? settlementMethod,
        TimeSeriesType expectedType)
    {
        // Edi.Requests.TimeSeriesType.Production is unused, kept for backwards compatibility
        var timeSeriesType = TimeSeriesTypeMapper.MapTimeSeriesType(
            Edi.Requests.TimeSeriesType.Production,
            meteringPointType,
            settlementMethod);

        // Assert
        Assert.Equal(expectedType, timeSeriesType);
    }

    [Theory]
    [InlineData(Edi.Requests.TimeSeriesType.TotalConsumption, TimeSeriesType.TotalConsumption)]
    [InlineData(Edi.Requests.TimeSeriesType.NonProfiledConsumption, TimeSeriesType.NonProfiledConsumption)]
    [InlineData(Edi.Requests.TimeSeriesType.FlexConsumption, TimeSeriesType.FlexConsumption)]
    [InlineData(Edi.Requests.TimeSeriesType.Production, TimeSeriesType.Production)]
    [InlineData(Edi.Requests.TimeSeriesType.NetExchangePerGa, TimeSeriesType.NetExchangePerGa)]
    public void MapTimeSeriesType_FromTimeSeriesType_returnsExpectedType(
        Edi.Requests.TimeSeriesType actualTimeSeriesType,
        TimeSeriesType expectedType)
    {
        var mappedTimeSeriesType = TimeSeriesTypeMapper.MapTimeSeriesType(
            actualTimeSeriesType,
            string.Empty,
            string.Empty);

        // Assert
        Assert.Equal(expectedType, mappedTimeSeriesType);
    }
}
