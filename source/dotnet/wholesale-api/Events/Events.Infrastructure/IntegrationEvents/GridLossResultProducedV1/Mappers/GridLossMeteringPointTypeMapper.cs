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

using MeteringPointType = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.GridLossResultProducedV1.Types.MeteringPointType;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Mappers;

public static class GridLossMeteringPointTypeMapper
{
    public static MeteringPointType MapFromTimeSeriesType(CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType timeSeriesType)
    {
        return timeSeriesType switch
        {
            CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType.NegativeGridLoss => MeteringPointType.Production,
            CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType.PositiveGridLoss => MeteringPointType.Consumption,
            _ => throw new ArgumentOutOfRangeException(
                nameof(timeSeriesType),
                actualValue: timeSeriesType,
                "Value cannot be mapped to a metering point type for grid loss."),
        };
    }
}
