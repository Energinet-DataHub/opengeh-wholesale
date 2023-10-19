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

using TimeSeriesType = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProduced.V2.EnergyResultProduced.Types.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EnergyResultProduced.V2.Mappers;

public static class TimeSeriesTypeMapper
{
    public static TimeSeriesType MapTimeSeriesType(CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType timeSeriesType)
    {
        return timeSeriesType switch
        {
            CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType.Production => TimeSeriesType.Production,
            CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType.FlexConsumption => TimeSeriesType.FlexConsumption,
            CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType.NonProfiledConsumption => TimeSeriesType.NonProfiledConsumption,
            CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType.NetExchangePerGa => TimeSeriesType.NetExchangePerGa,
            CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType.NetExchangePerNeighboringGa => TimeSeriesType.NetExchangePerNeighboringGa,
            CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType.GridLoss => TimeSeriesType.GridLoss,
            CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType.NegativeGridLoss => TimeSeriesType.NegativeGridLoss,
            CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType.PositiveGridLoss => TimeSeriesType.PositiveGridLoss,
            CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType.TotalConsumption => TimeSeriesType.TotalConsumption,
            CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType.TempFlexConsumption => TimeSeriesType.TempFlexConsumption,
            CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType.TempProduction => TimeSeriesType.TempProduction,

            _ => throw new ArgumentOutOfRangeException(
                nameof(timeSeriesType),
                actualValue: timeSeriesType,
                "Value cannot be mapped to a time series type."),
        };
    }
}
