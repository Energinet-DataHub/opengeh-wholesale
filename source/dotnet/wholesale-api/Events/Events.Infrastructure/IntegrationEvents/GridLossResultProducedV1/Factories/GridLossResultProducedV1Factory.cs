﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.Common;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Common;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Mappers;
using Google.Protobuf.WellKnownTypes;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Factories;

public class GridLossResultProducedV1Factory : IGridLossResultProducedV1Factory
{
    public bool CanCreate(EnergyResult result) =>
        result.TimeSeriesType is TimeSeriesType.NegativeGridLoss or TimeSeriesType.PositiveGridLoss;

    public Contracts.IntegrationEvents.GridLossResultProducedV1 Create(EnergyResult result)
    {
        if (!CanCreate(result))
            throw new ArgumentException($"Cannot create '{nameof(GridLossResultProducedV1)}' from energy result.", nameof(result));

        var gridLossResultProduced = new Contracts.IntegrationEvents.GridLossResultProducedV1
        {
            CalculationId = result.CalculationId.ToString(),
            MeteringPointId = result.MeteringPointId,
            MeteringPointType = GridLossMeteringPointTypeMapper.MapFromTimeSeriesType(result.TimeSeriesType),
            Resolution = ResolutionMapper.MapResolution(result.Resolution),
            QuantityUnit = Contracts.IntegrationEvents.GridLossResultProducedV1.Types.QuantityUnit.Kwh,
            PeriodStartUtc = result.PeriodStart.ToTimestamp(),
            PeriodEndUtc = result.PeriodEnd.ToTimestamp(),
        };

        gridLossResultProduced.TimeSeriesPoints
            .AddRange(result.TimeSeriesPoints
                .Select(timeSeriesPoint =>
                {
                    var mappedTimeSeriesPoint = new Contracts.IntegrationEvents.GridLossResultProducedV1.Types.TimeSeriesPoint
                    {
                        Quantity = new DecimalValue(timeSeriesPoint.Quantity),
                        Time = timeSeriesPoint.Time.ToTimestamp(),
                    };
                    return mappedTimeSeriesPoint;
                }));
        return gridLossResultProduced;
    }
}
