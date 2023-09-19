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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Wholesale.Events.Application.InboxEvents;
using NodaTime.Serialization.Protobuf;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.InboxEvents;

public class AggregatedTimeSeriesRequestMessageParser : IAggregatedTimeSeriesRequestMessageParser
{
    public AggregatedTimeSeriesRequest Parse(ServiceBusReceivedMessage request)
    {
        var aggregatedTimeSeriesRequest = Edi.Requests.AggregatedTimeSeriesRequest.Parser.ParseFrom(request.Body);

        return MapAggregatedTimeSeriesRequest(aggregatedTimeSeriesRequest);
    }

    private AggregatedTimeSeriesRequest MapAggregatedTimeSeriesRequest(Edi.Requests.AggregatedTimeSeriesRequest aggregatedTimeSeriesRequest)
    {
        return new AggregatedTimeSeriesRequest(
            MapPeriod(aggregatedTimeSeriesRequest.Period),
            MapTimeSeriesType(aggregatedTimeSeriesRequest.TimeSeriesType),
            MapAggregationPerRoleAndGridArea(aggregatedTimeSeriesRequest));
    }

    private AggregationPerRoleAndGridArea MapAggregationPerRoleAndGridArea(Edi.Requests.AggregatedTimeSeriesRequest aggregatedTimeSeriesRequest)
    {
        return aggregatedTimeSeriesRequest.AggregationLevelCase switch
        {
            Edi.Requests.AggregatedTimeSeriesRequest.AggregationLevelOneofCase.AggregationPerGridarea =>
                new AggregationPerRoleAndGridArea(GridAreaCode: aggregatedTimeSeriesRequest.AggregationPerGridarea.GridAreaCode),
            Edi.Requests.AggregatedTimeSeriesRequest.AggregationLevelOneofCase.AggregationPerEnergysupplierPerGridarea =>
                new AggregationPerRoleAndGridArea(
                    GridAreaCode: aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerGridarea.GridAreaCode,
                    EnergySupplierId: aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerGridarea.EnergySupplierId),
            Edi.Requests.AggregatedTimeSeriesRequest.AggregationLevelOneofCase.AggregationPerBalanceresponsiblepartyPerGridarea =>
                new AggregationPerRoleAndGridArea(
                    GridAreaCode: aggregatedTimeSeriesRequest.AggregationPerBalanceresponsiblepartyPerGridarea.GridAreaCode,
                    BalanceResponsibleId: aggregatedTimeSeriesRequest.AggregationPerBalanceresponsiblepartyPerGridarea.BalanceResponsiblePartyId),
            Edi.Requests.AggregatedTimeSeriesRequest.AggregationLevelOneofCase.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea =>
                new AggregationPerRoleAndGridArea(
                    GridAreaCode: aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.GridAreaCode,
                    EnergySupplierId: aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.EnergySupplierId,
                    BalanceResponsibleId: aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.BalanceResponsiblePartyId),
            _ => throw new InvalidOperationException("Unknown aggregation level"),
        };
    }

    private TimeSeriesType MapTimeSeriesType(Edi.Requests.TimeSeriesType timeSeriesType)
    {
        return timeSeriesType switch
        {
            Edi.Requests.TimeSeriesType.Production => TimeSeriesType.Production,
            Edi.Requests.TimeSeriesType.FlexConsumption => TimeSeriesType.FlexConsumption,
            Edi.Requests.TimeSeriesType.NonProfiledConsumption => TimeSeriesType.NonProfiledConsumption,
            Edi.Requests.TimeSeriesType.NetExchangePerGa => TimeSeriesType.NetExchangePerGa,
            Edi.Requests.TimeSeriesType.NetExchangePerNeighboringGa => TimeSeriesType.NetExchangePerNeighboringGa,
            Edi.Requests.TimeSeriesType.TotalConsumption => TimeSeriesType.TotalConsumption,
            Edi.Requests.TimeSeriesType.Unspecified => throw new InvalidOperationException("Unknown time series type"),
            _ => throw new InvalidOperationException("Unknown time series type"),
        };
    }

    private Period MapPeriod(Edi.Requests.Period period)
    {
        return new Period(period.StartOfPeriod.ToInstant(), period.EndOfPeriod.ToInstant());
    }
}
