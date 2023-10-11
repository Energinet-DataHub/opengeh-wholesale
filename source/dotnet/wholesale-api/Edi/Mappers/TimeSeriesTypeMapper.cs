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

using Energinet.DataHub.Wholesale.EDI.Models;

namespace Energinet.DataHub.Wholesale.EDI.Mappers;

public static class TimeSeriesTypeMapper
{
    public static TimeSeriesType MapTimeSeriesType(Energinet.DataHub.Edi.Requests.TimeSeriesType timeSeriesType, string meteringPointType, string? settlementMethod)
    {
        // TODO: Delete this, when EDI has updated their contract
        if (string.IsNullOrWhiteSpace(meteringPointType))
        {
            return timeSeriesType switch
            {
                Edi.Requests.TimeSeriesType.Production => TimeSeriesType.Production,
                Edi.Requests.TimeSeriesType.FlexConsumption => TimeSeriesType.FlexConsumption,
                Edi.Requests.TimeSeriesType.NonProfiledConsumption => TimeSeriesType
                    .NonProfiledConsumption,
                Edi.Requests.TimeSeriesType.NetExchangePerGa => TimeSeriesType.NetExchangePerGa,
                Edi.Requests.TimeSeriesType.NetExchangePerNeighboringGa => TimeSeriesType
                    .NetExchangePerNeighboringGa,
                Edi.Requests.TimeSeriesType.TotalConsumption => TimeSeriesType.TotalConsumption,
                Edi.Requests.TimeSeriesType.Unspecified => throw new InvalidOperationException(
                    "Unknown time series type"),
                _ => throw new InvalidOperationException("Unknown time series type"),
            };
        }

        return meteringPointType switch
        {
            "E18" => TimeSeriesType.Production,
            "E20" => TimeSeriesType.NetExchangePerGa,
            "E17" => settlementMethod switch
            {
                "E02" => TimeSeriesType.NonProfiledConsumption,
                "D01" => TimeSeriesType.FlexConsumption,
                var method when
                    string.IsNullOrWhiteSpace(method) => TimeSeriesType.TotalConsumption,
                _ => throw new InvalidOperationException("Unknown time series type"),
            },
            _ => throw new InvalidOperationException("Unknown time series type"),
        };
    }
}
