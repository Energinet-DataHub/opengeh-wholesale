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

using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;

public sealed class CompletedCalculation
{
    public CompletedCalculation(
        Guid id,
        List<string> gridAreaCodes,
        ProcessType processType,
        Instant periodStart,
        Instant periodEnd,
        Instant completedTime,
        string version)
    {
        Id = id;
        GridAreaCodes = gridAreaCodes;
        ProcessType = processType;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        CompletedTime = completedTime;
        Version = version;
    }

    /// <summary>
    /// The time when integration events for the batch were published.
    /// </summary>
    public Instant? PublishedTime { get; set; }

    public Guid Id { get; init; }

    public List<string> GridAreaCodes { get; init; }

    public ProcessType ProcessType { get; init; }

    public Instant PeriodStart { get; init; }

    public Instant PeriodEnd { get; init; }

    public Instant CompletedTime { get; private set; }

    public string Version { get; init; }
}
