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

using Energinet.DataHub.Wholesale.Edi.Exceptions;
using Energinet.DataHub.Wholesale.Edi.Models;
using CalculationTimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;
using TimeSeriesTypeContract = Energinet.DataHub.Edi.Responses.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Edi.Mappers;

public static class CalculationTimeSeriesTypeMapper
{
    public static CalculationTimeSeriesType MapTimeSeriesTypeFromEdi(TimeSeriesType timeSeriesType)
    {
        return timeSeriesType switch
        {
            TimeSeriesType.Production => CalculationTimeSeriesType.Production,
            TimeSeriesType.FlexConsumption => CalculationTimeSeriesType.FlexConsumption,
            TimeSeriesType.TotalConsumption => CalculationTimeSeriesType.TotalConsumption,
            TimeSeriesType.NetExchangePerGa => CalculationTimeSeriesType.NetExchangePerGa,
            TimeSeriesType.NonProfiledConsumption => CalculationTimeSeriesType.NonProfiledConsumption,

            _ => throw new ArgumentOutOfRangeException(
                nameof(timeSeriesType),
                actualValue: timeSeriesType,
                "Value cannot be mapped to calculation time series type."),
        };
    }

    public static TimeSeriesTypeContract MapTimeSeriesTypeFromCalculationsResult(CalculationTimeSeriesType timeSeriesType)
    {
        return timeSeriesType switch
        {
            CalculationTimeSeriesType.Production => TimeSeriesTypeContract.Production,
            CalculationTimeSeriesType.NonProfiledConsumption => TimeSeriesTypeContract.NonProfiledConsumption,
            CalculationTimeSeriesType.TotalConsumption => TimeSeriesTypeContract.TotalConsumption,
            CalculationTimeSeriesType.FlexConsumption => TimeSeriesTypeContract.FlexConsumption,
            CalculationTimeSeriesType.NetExchangePerGa => TimeSeriesTypeContract.NetExchangePerGa,
            CalculationTimeSeriesType.GridLoss => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),
            CalculationTimeSeriesType.TempProduction => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),
            CalculationTimeSeriesType.NegativeGridLoss => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),
            CalculationTimeSeriesType.PositiveGridLoss => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),
            CalculationTimeSeriesType.TempFlexConsumption => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),
            CalculationTimeSeriesType.NetExchangePerNeighboringGa => throw new NotSupportedTimeSeriesTypeException(
                $"{timeSeriesType} is not a supported TimeSeriesType For AggregatedTimeSeriesRequestAccepted response."),

            _ => throw new ArgumentOutOfRangeException(
                nameof(timeSeriesType),
                actualValue: timeSeriesType,
                "Value cannot be mapped to time service type contract."),
        };
    }
}
