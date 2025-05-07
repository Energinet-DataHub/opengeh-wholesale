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

using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation;

namespace Energinet.DataHub.Wholesale.Orchestrations.Extensions.Options;

/// <summary>
/// Options for the calculation orchestration monitor loops implemented as part of the
/// </summary>
public class CalculationOrchestrationMonitorOptions
{
    public const string SectionName = "CalculationOrchestrationMonitor";

    /// <summary>
    /// Time between each call to get the calculation job status.
    /// </summary>
    public int CalculationJobStatusPollingIntervalInSeconds { get; set; } = 60; // 1 minute

    /// <summary>
    /// Expiry time of the calculation job status monitor (loop).
    /// </summary>
    public int CalculationJobStatusExpiryTimeInSeconds { get; set; } = 3600 * 12; // 1 hour * 12

    /// <summary>
    /// Expiry time of the actor messages enqueuing monitor.
    /// </summary>
    public int MessagesEnqueuingExpiryTimeInSeconds { get; set; } = 3600 * 24 * 14; // 1 hour * 24 * 14 = 14 days
}
