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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Edi.Mappers;
using Energinet.DataHub.Wholesale.Edi.Models;
using Google.Protobuf.Collections;
using NodaTime;
using NodaTime.Text;
using ChargeType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.ChargeType;
using Period = Energinet.DataHub.Wholesale.Edi.Models.Period;
using Resolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution;

namespace Energinet.DataHub.Wholesale.Edi.Factories;

public class WholesaleServicesRequestMapper(DateTimeZone dateTimeZone)
{
    public IReadOnlyCollection<WholesaleServicesRequest> Map(Energinet.DataHub.Edi.Requests.WholesaleServicesRequest request)
    {
        var periodStart = InstantPattern.General.Parse(request.PeriodStart).Value;

        var periodEnd = request.HasPeriodEnd
            ? InstantPattern.General.Parse(request.PeriodEnd).Value
            : CalculateMaxPeriodEnd(periodStart);

        var resolution = request.HasResolution ? ResolutionMapper.Map(request.Resolution) : (Resolution?)null;

        // If no charge types are requested, both monthly amount and total monthly amount is requested
        var amountTypes = AmountTypeMapper.Map(resolution, AllChargesIsRequested(request));

        return amountTypes.Select(amountType => new WholesaleServicesRequest(
                amountType,
                request.GridAreaCodes,
                request.HasEnergySupplierId ? request.EnergySupplierId : null,
                request.HasChargeOwnerId ? request.ChargeOwnerId : null,
                MapChargeTypes(request.ChargeTypes),
                new Period(
                    periodStart,
                    periodEnd),
                RequestedCalculationTypeMapper.ToRequestedCalculationType(
                    request.BusinessReason,
                    request.HasSettlementVersion ? request.SettlementVersion : null),
                request.RequestedForActorRole,
                request.RequestedForActorNumber))
            .ToList();
    }

    private static bool AllChargesIsRequested(DataHub.Edi.Requests.WholesaleServicesRequest request)
    {
        return request.ChargeTypes.Count == 0;
    }

    private List<ChargeCodeAndType> MapChargeTypes(RepeatedField<Energinet.DataHub.Edi.Requests.ChargeType> chargeTypes)
    {
        return chargeTypes
            .Select(c => new ChargeCodeAndType(
                c.HasChargeCode ? c.ChargeCode : null,
                c.HasChargeType_ ? ChargeTypeMapper.Map(c.ChargeType_) : null))
            .ToList();
    }

    private Instant CalculateMaxPeriodEnd(Instant start)
    {
        var endDateTime = start.InZone(dateTimeZone).LocalDateTime.PlusMonths(1);
        return endDateTime.InZoneLeniently(dateTimeZone).ToInstant();
    }
}
