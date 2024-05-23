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
    Scheduled = 1, // Planlagt
    Calculating = 2, // Beregner
    Calculated = 3, // Beregnet
    CalculationFailed = 4, // Beregning fejlet
    ActorMessagesEnqueuing = 5, // Beskeder dannes
    ActorMessagesEnqueued = 6, // Beskeder dannet
    MessagesEnqueuingFailed = 7, // Besked dannelse fejlet
    Completed = 8, // Orchestration færdig
}
