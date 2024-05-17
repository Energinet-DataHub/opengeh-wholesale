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

namespace Energinet.DataHub.Wholesale.WebApi.V3.Calculation;

public static class CalculationOrchestrationStateMapper
{
    public static CalculationOrchestrationState MapState(
        Common.Interfaces.Models.CalculationOrchestrationState state)
    {
        return state switch
        {
            Common.Interfaces.Models.CalculationOrchestrationState.Scheduled => CalculationOrchestrationState.Scheduled,
            Common.Interfaces.Models.CalculationOrchestrationState.Calculating => CalculationOrchestrationState.Calculating,
            Common.Interfaces.Models.CalculationOrchestrationState.Calculated => CalculationOrchestrationState.Calculated,
            Common.Interfaces.Models.CalculationOrchestrationState.CalculationFailed => CalculationOrchestrationState.CalculationFailed,
            Common.Interfaces.Models.CalculationOrchestrationState.MessagesEnqueuing => CalculationOrchestrationState.MessagesEnqueuing,
            Common.Interfaces.Models.CalculationOrchestrationState.MessagesEnqueued => CalculationOrchestrationState.MessagesEnqueued,
            Common.Interfaces.Models.CalculationOrchestrationState.MessagesEnqueuingFailed => CalculationOrchestrationState.MessagesEnqueuingFailed,
            Common.Interfaces.Models.CalculationOrchestrationState.Completed => CalculationOrchestrationState.Completed,

            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                actualValue: state,
                "Value cannot be mapped to CalculationOrchestrationState."),
        };
    }
}
