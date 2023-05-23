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

using NodaTime;

namespace Energinet.DataHub.Wholesale.IntegrationEventsPublishing.Application.BatchAggregate;

public sealed class CompletedBatch
{
    public CompletedBatch(
        Guid id,
        List<string> gridAreaCodes,
        // TODO BJM: Is it ok to use this type here?
        ProcessType processType,
        Instant periodStart,
        Instant periodEnd,
        Instant completedTime,
        bool isPublished)
    {
        Id = id;
        GridAreaCodes = gridAreaCodes;
        ProcessType = processType;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        CompletedTime = completedTime;
        IsPublished = isPublished;
    }

    public bool IsPublished { get; set; }

    public Guid Id { get; init; }

    public List<string> GridAreaCodes { get; init; }

    public ProcessType ProcessType { get; init; }

    public Instant PeriodStart { get; init; }

    public Instant PeriodEnd { get; init; }

    public Instant CompletedTime { get; init; }
}
