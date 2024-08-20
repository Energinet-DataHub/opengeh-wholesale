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

namespace Energinet.DataHub.Wholesale.Common.Interfaces.Models;

/// <summary>
/// Used to represent the state of a calculation orchestration.
/// IMPORTANT: Do not change numeric values as it is persisted in the database.
/// </summary>
public enum CalculationOrchestrationState
{
    /// <summary>
    /// Calculation is scheduled to run at a later time (and not started yet).
    /// </summary>
    Scheduled = 1,

    /// <summary>
    /// The calculation orchestration is started (after the schedule has been met)
    /// </summary>
    Started = 11,

    /// <summary>
    /// The calculation results are being calculated by the calculation engine.
    /// </summary>
    Calculating = 2,

    /// <summary>
    /// The calculation has finished running in the calculation engine and the calculation results are ready.
    /// </summary>
    Calculated = 3,

    /// <summary>
    /// The calculation failed while calculating calculation results in the calculation engine.
    /// </summary>
    CalculationFailed = 4,

    /// <summary>
    /// Actor messages are being enqueued in the EDI subsystem based on the calculation results.
    /// </summary>
    ActorMessagesEnqueuing = 5,

    /// <summary>
    /// All actor messages has been enqueued in the EDI subsystem, and the actor messages are ready to be consumed by actors.
    /// </summary>
    ActorMessagesEnqueued = 6,

    /// <summary>
    /// Atleast one actor message failed to be enqueued in the EDI subsystem.
    /// </summary>
    ActorMessagesEnqueuingFailed = 7,

    /// <summary>
    /// The calculation orchestration is completed
    /// </summary>
    Completed = 8,

    /// <summary>
    /// The calculation orchestration is canceled (it can only be canceled if it is scheduled and not started yet)
    /// </summary>
    Canceled = 9,
}
