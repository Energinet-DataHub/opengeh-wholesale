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

using Energinet.DataHub.Wholesale.Edi.Models;
using Energinet.DataHub.Wholesale.EDI.Models;

namespace Energinet.DataHub.Wholesale.EDI.Mappers;

public static class TimeSeriesTypeMapper
{
    public static TimeSeriesType MapTimeSeriesType(string meteringPointType, string? settlementMethod)
    {
        return meteringPointType switch
        {
            MeteringPointType.Production => TimeSeriesType.Production,
            MeteringPointType.Exchange => TimeSeriesType.NetExchangePerGa,
            MeteringPointType.Consumption => settlementMethod switch
            {
                SettlementMethod.NonProfiled => TimeSeriesType.NonProfiledConsumption,
                SettlementMethod.Flex => TimeSeriesType.FlexConsumption,
                var method when
                    string.IsNullOrWhiteSpace(method) => TimeSeriesType.TotalConsumption,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(settlementMethod),
                    actualValue: settlementMethod,
                    "Value does not contain a valid string representation of a settlement method."),
            },

            _ => throw new ArgumentOutOfRangeException(
                nameof(meteringPointType),
                actualValue: meteringPointType,
                "Value does not contain a valid string representation of a metering point type."),
        };
    }
}
