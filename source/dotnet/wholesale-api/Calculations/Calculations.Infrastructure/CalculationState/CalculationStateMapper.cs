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

using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;

namespace Energinet.DataHub.Wholesale.Calculations.Infrastructure.CalculationState;

public class CalculationStateMapper
{
    public static CalculationOrchestrationState MapState(Application.Model.CalculationState calculationState)
    {
        return calculationState switch
        {
            Application.Model.CalculationState.Pending => CalculationOrchestrationState.Scheduled,
            Application.Model.CalculationState.Running => CalculationOrchestrationState.Calculating,
            Application.Model.CalculationState.Completed => CalculationOrchestrationState.Calculated,
            Application.Model.CalculationState.Failed => CalculationOrchestrationState.CalculationFailed,

            // Application.Model.CalculationState.Canceled cannot be mapped, since it is a databricks thing unrelated to domain
            _ => throw new ArgumentOutOfRangeException(
                nameof(calculationState),
                actualValue: calculationState,
                "Value cannot be mapped to a calculation orchestration state."),
        };
    }
}
