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

using Energinet.DataHub.Wholesale.EDI.Exceptions;
using Energinet.DataHub.Wholesale.EDI.Models;
using TimeSeriesTypeContract = Energinet.DataHub.Edi.Responses.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.EDI.Mappers;

using CalculationTimeSeriesType = CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType;

public static class TimeSeriesTypeMapper
{
    public static CalculationTimeSeriesType MapTimeSeriesTypeFromEdi(TimeSeriesType timeSeriesType)
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

    public static TimeSeriesTypeContract MapTimeSeriesTypeFromCalculationsResult(Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType timeSeriesType)
    {
        return timeSeriesType switch
        {
            CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.Production => TimeSeriesTypeContract.Production,
            CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.NonProfiledConsumption => TimeSeriesTypeContract.NonProfiledConsumption,
            CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.TotalConsumption => TimeSeriesTypeContract.TotalConsumption,
            CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.FlexConsumption => TimeSeriesTypeContract.FlexConsumption,
            CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.NetExchangePerGa => TimeSeriesTypeContract.NetExchangePerGa,
            CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.GridLoss => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),
            CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.TempProduction => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),
            CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.NegativeGridLoss => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),
            CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.PositiveGridLoss => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),
            CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.TempFlexConsumption => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),
            CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.NetExchangePerNeighboringGa => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),
            _ => throw new ArgumentOutOfRangeException($"Unknown time series type {nameof(timeSeriesType)}"),
        };
    }
}
