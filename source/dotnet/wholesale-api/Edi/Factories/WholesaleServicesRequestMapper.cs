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

using Energinet.DataHub.Wholesale.Edi.Mappers;
using Energinet.DataHub.Wholesale.Edi.Models;
using NodaTime;
using NodaTime.Text;
using Period = Energinet.DataHub.Wholesale.Edi.Models.Period;

namespace Energinet.DataHub.Wholesale.Edi.Factories;

public class WholesaleServicesRequestMapper(DateTimeZone dateTimeZone)
{
    public WholesaleServicesRequest Map(Energinet.DataHub.Edi.Requests.WholesaleServicesRequest request)
    {
        var periodStart = InstantPattern.General.Parse(request.PeriodStart).Value;

        var periodEnd = request.HasPeriodEnd
            ? InstantPattern.General.Parse(request.PeriodEnd).Value
            : CalculateMaxPeriodEnd(periodStart);

        return new WholesaleServicesRequest(
            request.GridAreaCode,
            new Period(
                periodStart,
                periodEnd),
            RequestedCalculationTypeMapper.ToRequestedCalculationType(request.BusinessReason, request.HasSettlementSeriesVersion ? request.SettlementSeriesVersion : null));
    }

    private Instant CalculateMaxPeriodEnd(Instant start)
    {
        var endDateTime = start.InZone(dateTimeZone).LocalDateTime.PlusMonths(1);
        return endDateTime.InZoneLeniently(dateTimeZone).ToInstant();
    }
}
