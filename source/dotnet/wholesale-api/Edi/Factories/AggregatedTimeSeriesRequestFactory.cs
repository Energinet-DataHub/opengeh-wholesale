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
using Energinet.DataHub.Wholesale.EDI.Models;
using NodaTime.Serialization.Protobuf;
using NodaTime.Text;

namespace Energinet.DataHub.Wholesale.EDI.Factories;

public class AggregatedTimeSeriesRequestFactory : IAggregatedTimeSeriesRequestFactory
{
    public AggregatedTimeSeriesRequest Parse(ServiceBusReceivedMessage request)
    {
        var aggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest.Parser.ParseFrom(request.Body);

        return MapAggregatedTimeSeriesRequest(aggregatedTimeSeriesRequest);
    }

    public AggregatedTimeSeriesRequest Parse(Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest request)
    {
        return MapAggregatedTimeSeriesRequest(request);
    }

    private AggregatedTimeSeriesRequest MapAggregatedTimeSeriesRequest(Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest aggregatedTimeSeriesRequest)
    {
        return new AggregatedTimeSeriesRequest(
            MapPeriod(aggregatedTimeSeriesRequest.Period),
            MapTimeSeriesType(aggregatedTimeSeriesRequest.TimeSeriesType),
            MapAggregationPerRoleAndGridArea(aggregatedTimeSeriesRequest));
    }

    private AggregationPerRoleAndGridArea MapAggregationPerRoleAndGridArea(Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest aggregatedTimeSeriesRequest)
    {
        return aggregatedTimeSeriesRequest.AggregationLevelCase switch
        {
            Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest.AggregationLevelOneofCase.AggregationPerGridarea =>
                new AggregationPerRoleAndGridArea(GridAreaCode: aggregatedTimeSeriesRequest.AggregationPerGridarea.GridAreaCode),
            Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest.AggregationLevelOneofCase.AggregationPerEnergysupplierPerGridarea =>
                new AggregationPerRoleAndGridArea(
                    GridAreaCode: aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerGridarea.GridAreaCode,
                    EnergySupplierId: aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerGridarea.EnergySupplierId),
            Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest.AggregationLevelOneofCase.AggregationPerBalanceresponsiblepartyPerGridarea =>
                new AggregationPerRoleAndGridArea(
                    GridAreaCode: aggregatedTimeSeriesRequest.AggregationPerBalanceresponsiblepartyPerGridarea.GridAreaCode,
                    BalanceResponsibleId: aggregatedTimeSeriesRequest.AggregationPerBalanceresponsiblepartyPerGridarea.BalanceResponsiblePartyId),
            Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest.AggregationLevelOneofCase.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea =>
                new AggregationPerRoleAndGridArea(
                    GridAreaCode: aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.GridAreaCode,
                    EnergySupplierId: aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.EnergySupplierId,
                    BalanceResponsibleId: aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.BalanceResponsiblePartyId),
            _ => throw new InvalidOperationException("Unknown aggregation level"),
        };
    }

    private TimeSeriesType MapTimeSeriesType(Energinet.DataHub.Edi.Requests.TimeSeriesType timeSeriesType)
    {
        return timeSeriesType switch
        {
            Energinet.DataHub.Edi.Requests.TimeSeriesType.Production => TimeSeriesType.Production,
            Energinet.DataHub.Edi.Requests.TimeSeriesType.FlexConsumption => TimeSeriesType.FlexConsumption,
            Energinet.DataHub.Edi.Requests.TimeSeriesType.NonProfiledConsumption => TimeSeriesType.NonProfiledConsumption,
            Energinet.DataHub.Edi.Requests.TimeSeriesType.NetExchangePerGa => TimeSeriesType.NetExchangePerGa,
            Energinet.DataHub.Edi.Requests.TimeSeriesType.NetExchangePerNeighboringGa => TimeSeriesType.NetExchangePerNeighboringGa,
            Energinet.DataHub.Edi.Requests.TimeSeriesType.TotalConsumption => TimeSeriesType.TotalConsumption,
            Energinet.DataHub.Edi.Requests.TimeSeriesType.Unspecified => throw new InvalidOperationException("Unknown time series type"),
            _ => throw new InvalidOperationException("Unknown time series type"),
        };
    }

    private Period MapPeriod(Energinet.DataHub.Edi.Requests.Period period)
    {
        return new Period(
        InstantPattern.General.Parse(period.Start).Value,
        InstantPattern.General.Parse(period.End).Value);
    }
}
