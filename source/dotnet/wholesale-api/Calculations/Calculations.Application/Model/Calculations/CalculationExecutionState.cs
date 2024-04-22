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

namespace Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;

/// <summary>
/// IMPORTANT: Do not change numeric values as it'll affect persistence or communication.
/// </summary>
public enum CalculationExecutionState
{
    /// <summary>
    /// The calculation is created.
    /// </summary>
    Created = -2,

    /// <summary>
    /// The calculation is submitted.
    /// </summary>
    Submitted = -1,

    /// <summary>
    /// The calculation is pending.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// The calculation is currently executing.
    /// </summary>
    Executing = 1,

    /// <summary>
    /// The calculation has (successfully) completed.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// The calculation has (Failed) failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// The calculation has been cancelled.
    /// </summary>
    Canceled,
}
