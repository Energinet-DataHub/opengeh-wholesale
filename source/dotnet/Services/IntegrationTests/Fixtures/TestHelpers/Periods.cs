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
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Fixtures.TestHelpers;

public static class Periods
{
    public static (DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd, DateTimeZone DateTimeZone)
        January_EuropeCopenhagen =>
        (
            DateTimeOffset.Parse("2021-12-31T23:00Z"),
            DateTimeOffset.Parse("2022-01-31T22:59:59.999Z"),
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!);

    public static (Instant PeriodStart, Instant PeriodEnd, DateTimeZone DateTimeZone) January_EuropeCopenhagen_Instant
    {
        get
        {
            (var periodStart, var periodEnd, var dateTimeZone) = January_EuropeCopenhagen;
            return (periodStart.ToInstant(), periodEnd.ToInstant(), dateTimeZone);
        }
    }
}
