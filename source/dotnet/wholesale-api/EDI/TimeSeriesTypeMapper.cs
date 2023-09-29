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

using EDI.InboxEvents;

namespace EDI;

using CalculationTimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType;

public static class TimeSeriesTypeMapper
{
    public static CalculationTimeSeriesType MapTimeSerieType(TimeSeriesType timeSeriesType)
    {
        return timeSeriesType switch {
            TimeSeriesType.Production => CalculationTimeSeriesType.Production,
            TimeSeriesType.FlexConsumption => CalculationTimeSeriesType.FlexConsumption,
            TimeSeriesType.TotalConsumption => CalculationTimeSeriesType.TotalConsumption,
            TimeSeriesType.NetExchangePerGa => CalculationTimeSeriesType.NetExchangePerGa,
            TimeSeriesType.NonProfiledConsumption => CalculationTimeSeriesType.NonProfiledConsumption,
            _ => throw new InvalidOperationException($"Unknown time series type: {timeSeriesType}"),
        };
    }
}
