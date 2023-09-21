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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Events.Infrastructure.InboxEvents.Exceptions;
using TimeSeriesTypeContract = Energinet.DataHub.Edi.Responses.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.InboxEvents.Mappers;

public static class TimeSeriesTypeMapper
{
    public static TimeSeriesTypeContract MapTimeSeriesType(TimeSeriesType timeSeriesType)
    {
        return timeSeriesType switch
        {
            TimeSeriesType.Production => TimeSeriesTypeContract.Production,
            TimeSeriesType.NonProfiledConsumption => TimeSeriesTypeContract.NonProfiledConsumption,
            TimeSeriesType.TotalConsumption => TimeSeriesTypeContract.TotalConsumption,
            TimeSeriesType.FlexConsumption => TimeSeriesTypeContract.FlexConsumption,
            TimeSeriesType.NetExchangePerGa => TimeSeriesTypeContract.NetExchangePerGa,
            TimeSeriesType.GridLoss => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),
            TimeSeriesType.TempProduction => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),
            TimeSeriesType.NegativeGridLoss => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),
            TimeSeriesType.PositiveGridLoss => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),
            TimeSeriesType.TempFlexConsumption => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),
            TimeSeriesType.NetExchangePerNeighboringGa => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),
            _ => throw new ArgumentOutOfRangeException($"Unknown time series type {nameof(timeSeriesType)}"),
        };
    }
}
