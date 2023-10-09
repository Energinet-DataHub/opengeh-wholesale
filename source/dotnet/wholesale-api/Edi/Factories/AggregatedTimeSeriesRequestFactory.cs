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
using Energinet.DataHub.Wholesale.EDI.Mappers;
using Energinet.DataHub.Wholesale.EDI.Models;
using NodaTime;
using NodaTime.Text;
using Period = Energinet.DataHub.Wholesale.EDI.Models.Period;

namespace Energinet.DataHub.Wholesale.EDI.Factories;

public class AggregatedTimeSeriesRequestFactory : IAggregatedTimeSeriesRequestFactory
{
    public AggregatedTimeSeriesRequest Parse(ServiceBusReceivedMessage request)
    {
        var aggregatedTimeSeriesRequest = Edi.Requests.AggregatedTimeSeriesRequest.Parser.ParseFrom(request.Body);

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
            MapTimeSeriesType(aggregatedTimeSeriesRequest.TimeSeriesType, aggregatedTimeSeriesRequest.MeteringPointType, aggregatedTimeSeriesRequest.SettlementMethod),
            MapAggregationPerRoleAndGridArea(aggregatedTimeSeriesRequest));
    }

    private AggregationPerRoleAndGridArea MapAggregationPerRoleAndGridArea(Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest aggregatedTimeSeriesRequest)
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

    private TimeSeriesType MapTimeSeriesType(Energinet.DataHub.Edi.Requests.TimeSeriesType timeSeriesType, string meteringPointType, string settlementMethod)
    {
        // TODO: Delete this, when EDI has updated their model
        if (string.IsNullOrWhiteSpace(meteringPointType))
        {
            return timeSeriesType switch
            {
                Edi.Requests.TimeSeriesType.Production => TimeSeriesType.Production,
                Edi.Requests.TimeSeriesType.FlexConsumption => TimeSeriesType.FlexConsumption,
                Edi.Requests.TimeSeriesType.NonProfiledConsumption => TimeSeriesType
                    .NonProfiledConsumption,
                Edi.Requests.TimeSeriesType.NetExchangePerGa => TimeSeriesType.NetExchangePerGa,
                Edi.Requests.TimeSeriesType.NetExchangePerNeighboringGa => TimeSeriesType
                    .NetExchangePerNeighboringGa,
                Edi.Requests.TimeSeriesType.TotalConsumption => TimeSeriesType.TotalConsumption,
                Edi.Requests.TimeSeriesType.Unspecified => throw new InvalidOperationException(
                    "Unknown time series type"),
                _ => throw new InvalidOperationException("Unknown time series type"),
            };
        }

        return meteringPointType switch
        {
            "E18" => TimeSeriesType.Production,
            "E20" => TimeSeriesType.NetExchangePerGa,
            "E17" => settlementMethod switch
            {
                "E02" => TimeSeriesType.NonProfiledConsumption,
                "D01" => TimeSeriesType.FlexConsumption,
                var method when
                    string.IsNullOrWhiteSpace(method) => TimeSeriesType.TotalConsumption,
                _ => throw new InvalidOperationException("Unknown time series type"),
            },
            _ => throw new InvalidOperationException("Unknown time series type"),
        };
    }

    private Period MapPeriod(Energinet.DataHub.Edi.Requests.Period period)
    {
        var end = string.IsNullOrWhiteSpace(period.End)
            ? Instant.FromDateTimeUtc(DateTime.UtcNow)
            : InstantPattern.General.Parse(period.End).Value;

        return new Period(
        InstantPattern.General.Parse(period.Start).Value,
        end);
    }
}
