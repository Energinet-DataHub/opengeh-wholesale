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

#pragma warning disable SA1300
namespace Energinet.DataHub.Wholesale.SubsystemTests.Clients.v3;
#pragma warning restore SA1300

public static class CalculationOrchestrationStateExtensions
{
    public static readonly List<CalculationOrchestrationState> CalculationJobCompletedStates =
    [
        CalculationOrchestrationState.Calculated,
        CalculationOrchestrationState.ActorMessagesEnqueuing,
        CalculationOrchestrationState.ActorMessagesEnqueued,
        CalculationOrchestrationState.ActorMessagesEnqueuingFailed,
        CalculationOrchestrationState.Completed
    ];

    public static bool IsCalculationJobCompleted(this CalculationOrchestrationState state)
    {
        return CalculationJobCompletedStates.Contains(state);
    }
}
