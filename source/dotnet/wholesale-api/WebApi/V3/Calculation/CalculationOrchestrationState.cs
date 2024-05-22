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

public enum CalculationOrchestrationState
{
    /// <summary>
    /// // The calculation is created but not yet started
    /// </summary>
    Scheduled,

    /// <summary>
    /// The data calculation is running and the result is not yet available
    /// </summary>
    Calculating,

    /// <summary>
    /// The data calculation is completed and data is ready to be consumed
    /// </summary>
    Calculated,

    /// <summary>
    /// The data calculation failed during calculation
    /// </summary>
    CalculationFailed,

    /// <summary>
    /// The completed calculation is sent for enqueuing as actor messages
    /// </summary>
    ActorMessagesEnqueuing,

    /// <summary>
    /// The actor messages for the completed calculation are enqueued and ready to be consumed by actors
    /// </summary>
    ActorMessagesEnqueued,

    /// <summary>
    /// Enqueuing the completed calculation as actor messages has failed
    /// </summary>
    ActorMessagesEnqueuingFailed,

    /// <summary>
    /// The calculation orchestration is completed (the calculation is completed and actor messages are enqueued)
    /// </summary>
    Completed,
}
