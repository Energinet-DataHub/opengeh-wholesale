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

        if (aggregatedTimeSeriesRequest.AggregationLevelCase ==
            Edi.Requests.AggregatedTimeSeriesRequest.AggregationLevelOneofCase.None)
            throw new InvalidOperationException("Unknown aggregation level");

        return MapAggregatedTimeSeriesRequest(aggregatedTimeSeriesRequest);
    }

    private AggregatedTimeSeriesRequest MapAggregatedTimeSeriesRequest(Edi.Requests.AggregatedTimeSeriesRequest aggregatedTimeSeriesRequest)
    {
        return new AggregatedTimeSeriesRequest(
            MapPeriod(aggregatedTimeSeriesRequest.Period),
            MapTimeSeriesType(aggregatedTimeSeriesRequest.TimeSeriesType),
            MapAggregationPerGridArea(aggregatedTimeSeriesRequest),
            MapAggregationPerEnergySupplierPerGridArea(aggregatedTimeSeriesRequest),
            MapAggregationPerBalanceResponsiblePartyPerGridArea(aggregatedTimeSeriesRequest),
            MapAggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea(aggregatedTimeSeriesRequest));
    }

    private AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea? MapAggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea(
        Edi.Requests.AggregatedTimeSeriesRequest aggregatedTimeSeriesRequest)
    {
        if (aggregatedTimeSeriesRequest.AggregationLevelCase != Edi.Requests.AggregatedTimeSeriesRequest
                .AggregationLevelOneofCase.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea)
            return null;

        return new AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea(
            aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.GridAreaCode,
            aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.BalanceResponsiblePartyId,
            aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.EnergySupplierId);
    }

    private AggregationPerBalanceResponsiblePartyPerGridArea? MapAggregationPerBalanceResponsiblePartyPerGridArea(
        Edi.Requests.AggregatedTimeSeriesRequest aggregatedTimeSeriesRequest)
    {
        if (aggregatedTimeSeriesRequest.AggregationLevelCase != Edi.Requests.AggregatedTimeSeriesRequest
                .AggregationLevelOneofCase.AggregationPerBalanceresponsiblepartyPerGridarea)
            return null;

        return new AggregationPerBalanceResponsiblePartyPerGridArea(
            aggregatedTimeSeriesRequest.AggregationPerBalanceresponsiblepartyPerGridarea.GridAreaCode,
            aggregatedTimeSeriesRequest.AggregationPerBalanceresponsiblepartyPerGridarea.BalanceResponsiblePartyId,
            aggregatedTimeSeriesRequest.AggregationPerBalanceresponsiblepartyPerGridarea.EnergySupplierId);
    }

    private AggregationPerEnergySupplierPerGridArea? MapAggregationPerEnergySupplierPerGridArea(
        Edi.Requests.AggregatedTimeSeriesRequest aggregatedTimeSeriesRequest)
    {
        if (aggregatedTimeSeriesRequest.AggregationLevelCase != Edi.Requests.AggregatedTimeSeriesRequest
                .AggregationLevelOneofCase.AggregationPerEnergysupplierPerGridarea)
            return null;

        return new AggregationPerEnergySupplierPerGridArea(
            aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerGridarea.GridAreaCode,
            aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerGridarea.BalanceResponsiblePartyId,
            aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerGridarea.EnergySupplierId);
    }

    private AggregationPerGridArea? MapAggregationPerGridArea(Edi.Requests.AggregatedTimeSeriesRequest aggregatedTimeSeriesRequest)
    {
        if (aggregatedTimeSeriesRequest.AggregationLevelCase != Edi.Requests.AggregatedTimeSeriesRequest
                .AggregationLevelOneofCase.AggregationPerGridarea) return null;

        return new AggregationPerGridArea(
            aggregatedTimeSeriesRequest.AggregationPerGridarea.GridAreaCode,
            aggregatedTimeSeriesRequest.AggregationPerGridarea.GridResponsibleId);
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
