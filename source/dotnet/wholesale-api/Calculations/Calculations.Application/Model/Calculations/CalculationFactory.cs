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

using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;

public class CalculationFactory : ICalculationFactory
{
    private readonly IClock _clock;
    private readonly DateTimeZone _dateTimeZone;

    public CalculationFactory(IClock clock, DateTimeZone dateTimeZone)
    {
        _clock = clock;
        _dateTimeZone = dateTimeZone;
    }

    public Calculation Create(
        CalculationType calculationType,
        IEnumerable<string> gridAreaCodes,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        DateTimeOffset scheduledAt,
        Guid createdByUserId,
        bool isInternalCalculation)
    {
        var createdTime = _clock.GetCurrentInstant();
        var gridAreas = gridAreaCodes.Select(c => new GridAreaCode(c)).ToList();
        var periodStart = Instant.FromDateTimeOffset(startDate);
        var periodEnd = Instant.FromDateTimeOffset(endDate);
        var scheduledAtInstant = Instant.FromDateTimeOffset(scheduledAt);

        var version = _clock.GetCurrentInstant().ToDateTimeUtc().Ticks;
        return new Calculation(
            createdTime,
            calculationType,
            gridAreas,
            periodStart,
            periodEnd,
            scheduledAtInstant,
            _dateTimeZone,
            createdByUserId,
            version,
            isInternalCalculation);
    }
}
