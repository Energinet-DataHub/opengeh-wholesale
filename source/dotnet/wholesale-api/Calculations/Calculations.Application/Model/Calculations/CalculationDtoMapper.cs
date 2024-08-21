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

using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;

namespace Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;

public class CalculationDtoMapper : ICalculationDtoMapper
{
    public CalculationDto Map(Calculation calculation)
    {
        return new CalculationDto(
            calculation.CalculationJobId?.Id,
            calculation.Id,
            calculation.PeriodStart.ToDateTimeOffset(),
            calculation.PeriodEnd.ToDateTimeOffset(),
            calculation.GetResolution(),
            calculation.GetQuantityUnit().ToString(),
            calculation.ScheduledAt.ToDateTimeOffset(),
            calculation.ExecutionTimeStart?.ToDateTimeOffset(),
            calculation.ExecutionTimeEnd?.ToDateTimeOffset() ?? null,
            MapState(calculation.ExecutionState),
            calculation.AreSettlementReportsCreated,
            MapGridAreaCodes(calculation.GridAreaCodes),
            calculation.CalculationType,
            calculation.CreatedByUserId,
            calculation.Version,
            calculation.OrchestrationState,
            calculation.CompletedTime?.ToDateTimeOffset() ?? null);
    }

    private static Interfaces.Models.CalculationState MapState(CalculationExecutionState state)
    {
        return state switch
        {
            CalculationExecutionState.Created => Interfaces.Models.CalculationState.Pending,
            CalculationExecutionState.Submitted => Interfaces.Models.CalculationState.Pending,
            CalculationExecutionState.Pending => Interfaces.Models.CalculationState.Pending,
            CalculationExecutionState.Executing => Interfaces.Models.CalculationState.Executing,
            CalculationExecutionState.Completed => Interfaces.Models.CalculationState.Completed,
            CalculationExecutionState.Failed => Interfaces.Models.CalculationState.Failed,

            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                actualValue: state,
                "Value cannot be mapped to a calculation state."),
        };
    }

    private static string[] MapGridAreaCodes(IReadOnlyCollection<GridAreaCode> gridAreaCodes)
    {
        return gridAreaCodes.Select(gridArea => gridArea.Code).ToArray();
    }
}
