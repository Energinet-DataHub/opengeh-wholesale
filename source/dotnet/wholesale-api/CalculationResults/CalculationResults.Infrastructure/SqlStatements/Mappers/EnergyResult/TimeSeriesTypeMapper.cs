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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.EnergyResult;

public static class TimeSeriesTypeMapper
{
    public static TimeSeriesType FromDeltaTableValue(string timeSeriesType) =>
        timeSeriesType switch
        {
            DeltaTableTimeSeriesType.Production => TimeSeriesType.Production,
            DeltaTableTimeSeriesType.FlexConsumption => TimeSeriesType.FlexConsumption,
            DeltaTableTimeSeriesType.NonProfiledConsumption => TimeSeriesType.NonProfiledConsumption,
            DeltaTableTimeSeriesType.NetExchangePerGridArea => TimeSeriesType.NetExchangePerGa,
            DeltaTableTimeSeriesType.NetExchangePerNeighboringGridArea => TimeSeriesType.NetExchangePerNeighboringGa,
            DeltaTableTimeSeriesType.GridLoss => TimeSeriesType.GridLoss,
            DeltaTableTimeSeriesType.NegativeGridLoss => TimeSeriesType.NegativeGridLoss,
            DeltaTableTimeSeriesType.PositiveGridLoss => TimeSeriesType.PositiveGridLoss,
            DeltaTableTimeSeriesType.TotalConsumption => TimeSeriesType.TotalConsumption,
            DeltaTableTimeSeriesType.TempFlexConsumption => TimeSeriesType.TempFlexConsumption,
            DeltaTableTimeSeriesType.TempProduction => TimeSeriesType.TempProduction,

            _ => throw new ArgumentOutOfRangeException(
                nameof(timeSeriesType),
                actualValue: timeSeriesType,
                "Value does not contain a valid string representation of a time series type."),
        };

    public static string ToDeltaTableValue(TimeSeriesType timeSeriesType) =>
        timeSeriesType switch
        {
            TimeSeriesType.NonProfiledConsumption => DeltaTableTimeSeriesType.NonProfiledConsumption,
            TimeSeriesType.Production => DeltaTableTimeSeriesType.Production,
            TimeSeriesType.FlexConsumption => DeltaTableTimeSeriesType.FlexConsumption,
            TimeSeriesType.NetExchangePerGa => DeltaTableTimeSeriesType.NetExchangePerGridArea,
            TimeSeriesType.NetExchangePerNeighboringGa => DeltaTableTimeSeriesType.NetExchangePerNeighboringGridArea,
            TimeSeriesType.GridLoss => DeltaTableTimeSeriesType.GridLoss,
            TimeSeriesType.NegativeGridLoss => DeltaTableTimeSeriesType.NegativeGridLoss,
            TimeSeriesType.PositiveGridLoss => DeltaTableTimeSeriesType.PositiveGridLoss,
            TimeSeriesType.TotalConsumption => DeltaTableTimeSeriesType.TotalConsumption,
            TimeSeriesType.TempFlexConsumption => DeltaTableTimeSeriesType.TempFlexConsumption,
            TimeSeriesType.TempProduction => DeltaTableTimeSeriesType.TempProduction,

            _ => throw new ArgumentOutOfRangeException(
                nameof(timeSeriesType),
                actualValue: timeSeriesType,
                "Value cannot be mapped to a string representation of a time series type."),
        };
}
