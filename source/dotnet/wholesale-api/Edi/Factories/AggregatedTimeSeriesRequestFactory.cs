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

using Energinet.DataHub.Wholesale.EDI.Mappers;
using Energinet.DataHub.Wholesale.EDI.Models;
using NodaTime.Text;
using Period = Energinet.DataHub.Wholesale.EDI.Models.Period;

namespace Energinet.DataHub.Wholesale.EDI.Factories;

public static class AggregatedTimeSeriesRequestFactory
{
    public static AggregatedTimeSeriesRequest Parse(Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest request)
    {
        return new AggregatedTimeSeriesRequest(
            MapPeriod(request.Period),
            GetTimeSeriesTypes(request),
            MapAggregationPerRoleAndGridArea(request),
            RequestedCalculationTypeMapper.ToRequestedCalculationType(request.BusinessReason, request.HasSettlementSeriesVersion ? request.SettlementSeriesVersion : null));
    }

    private static TimeSeriesType[] GetTimeSeriesTypes(
        Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest request)
    {
        return request.MeteringPointType != string.Empty
            ? [TimeSeriesTypeMapper.MapTimeSeriesType(request.MeteringPointType, request.SettlementMethod)]
            : request.RequestedByActorRole switch
            {
                ActorRoleCode.EnergySupplier =>
                [
                    TimeSeriesType.Production,
                    TimeSeriesType.FlexConsumption,
                    TimeSeriesType.NonProfiledConsumption,
                ],
                ActorRoleCode.BalanceResponsibleParty =>
                [
                    TimeSeriesType.Production,
                    TimeSeriesType.FlexConsumption,
                    TimeSeriesType.NonProfiledConsumption,
                ],
                ActorRoleCode.MeteredDataResponsible =>
                [
                    TimeSeriesType.Production,
                    TimeSeriesType.FlexConsumption,
                    TimeSeriesType.NonProfiledConsumption,
                    TimeSeriesType.TotalConsumption,
                    TimeSeriesType.NetExchangePerGa,
                ],
                _ => throw new ArgumentOutOfRangeException(
                    nameof(request.RequestedByActorRole),
                    request.RequestedByActorRole,
                    "Value does not contain a valid string representation of a requested by actor role."),
            };
    }

    private static AggregationPerRoleAndGridArea MapAggregationPerRoleAndGridArea(Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest request)
    {
        return new AggregationPerRoleAndGridArea(
            GridAreaCode: request.HasGridAreaCode ? request.GridAreaCode : null,
            EnergySupplierId: request.HasEnergySupplierId ? request.EnergySupplierId : null,
            BalanceResponsibleId: request.HasBalanceResponsibleId ? request.BalanceResponsibleId : null);
    }

    private static Period MapPeriod(Energinet.DataHub.Edi.Requests.Period period)
    {
        return new Period(
        InstantPattern.General.Parse(period.Start).Value,
        InstantPattern.General.Parse(period.End).Value);
    }
}
