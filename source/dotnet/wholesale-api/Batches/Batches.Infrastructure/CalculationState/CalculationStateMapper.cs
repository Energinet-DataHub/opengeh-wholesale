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

using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;

namespace Energinet.DataHub.Wholesale.Batches.Infrastructure.CalculationState;

public class CalculationStateMapper
{
    public static CalculationExecutionState MapState(Application.Model.CalculationState calculationState)
    {
        return calculationState switch
        {
            Application.Model.CalculationState.Pending => CalculationExecutionState.Pending,
            Application.Model.CalculationState.Running => CalculationExecutionState.Executing,
            Application.Model.CalculationState.Completed => CalculationExecutionState.Completed,
            Application.Model.CalculationState.Canceled => CalculationExecutionState.Canceled,
            Application.Model.CalculationState.Failed => CalculationExecutionState.Failed,

            _ => throw new ArgumentOutOfRangeException(
                nameof(calculationState),
                actualValue: calculationState,
                "Value cannot be mapped to a batch execution state."),
        };
    }
}
