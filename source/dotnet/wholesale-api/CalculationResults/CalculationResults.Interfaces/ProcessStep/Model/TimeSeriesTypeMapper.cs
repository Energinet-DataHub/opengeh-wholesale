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

namespace Energinet.DataHub.Wholesale.CalculationResults.Interfaces.ProcessStep.Model;

public static class TimeSeriesTypeMapper
{
    public static CalculationResultClient.TimeSeriesType Map(TimeSeriesType timeSeriesType)
    {
        return timeSeriesType switch
        {
            TimeSeriesType.NonProfiledConsumption => CalculationResultClient.TimeSeriesType.NonProfiledConsumption,
            TimeSeriesType.FlexConsumption => CalculationResultClient.TimeSeriesType.FlexConsumption,
            TimeSeriesType.Production => CalculationResultClient.TimeSeriesType.Production,
            _ => throw new ArgumentOutOfRangeException(nameof(timeSeriesType), timeSeriesType, null),
        };
    }

    public static TimeSeriesType Map(CalculationResultClient.TimeSeriesType timeSeriesType)
    {
        return timeSeriesType switch
        {
            CalculationResultClient.TimeSeriesType.NonProfiledConsumption => TimeSeriesType.NonProfiledConsumption,
            CalculationResultClient.TimeSeriesType.FlexConsumption => TimeSeriesType.FlexConsumption,
            CalculationResultClient.TimeSeriesType.Production => TimeSeriesType.Production,
            _ => throw new ArgumentOutOfRangeException(nameof(timeSeriesType), timeSeriesType, null),
        };
    }
}
