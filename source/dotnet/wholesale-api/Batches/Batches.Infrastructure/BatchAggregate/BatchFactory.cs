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

using Energinet.DataHub.Wholesale.Batches.Infrastructure.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Batches.Infrastructure.BatchAggregate;

public class BatchFactory : IBatchFactory
{
    private readonly IClock _clock;
    private readonly DateTimeZone _dateTimeZone;

    public BatchFactory(IClock clock, DateTimeZone dateTimeZone)
    {
        _clock = clock;
        _dateTimeZone = dateTimeZone;
    }

    public Batch Create(
        ProcessType processType,
        IEnumerable<string> gridAreaCodes,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        Guid createdByUserId)
    {
        var gridAreas = gridAreaCodes.Select(c => new GridAreaCode(c)).ToList();
        var periodStart = Instant.FromDateTimeOffset(startDate);
        var periodEnd = Instant.FromDateTimeOffset(endDate);
        var executionTimeStart = _clock.GetCurrentInstant();
        return new Batch(processType, gridAreas, periodStart, periodEnd, executionTimeStart, _dateTimeZone, createdByUserId);
    }
}
