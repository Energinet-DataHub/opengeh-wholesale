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

using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation;

namespace Energinet.DataHub.Wholesale.Orchestrations.Extensions.Options;

/// <summary>
/// Options for the CalculationJob status monitor implemented as part of the
/// <see cref="CalculationOrchestration.Calculation"/>.
/// </summary>
public class CalculationJobStatusMonitorOptions
{
    public const string SectionName = "CalculationJobStatusMonitor";

    /// <summary>
    /// Time between each call to get the job status.
    /// </summary>
    public int PollingIntervalInSeconds { get; set; } = 60;

    /// <summary>
    /// Expiry time of the job status monitor (loop).
    /// </summary>
    public int ExpiryTimeInSeconds { get; set; } = 3600;
}
