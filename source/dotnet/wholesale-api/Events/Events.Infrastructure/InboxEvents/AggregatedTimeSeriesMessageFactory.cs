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
using Energinet.DataHub.Edi.Responses;
using Energinet.DataHub.Wholesale.Events.Application.InboxEvents;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NodaTime.Serialization.Protobuf;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;
using Period = Energinet.DataHub.Edi.Responses.Period;
using TimeSeriesType = Energinet.DataHub.Edi.Responses.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.InboxEvents;

public class AggregatedTimeSeriesMessageFactory : IAggregatedTimeSeriesMessageFactory
{
    public Application.InboxEvents.AggregatedTimeSeriesRequest GetAggregatedTimeSeriesRequest(ServiceBusReceivedMessage request)
    {
        var aggregatedTimeSeriesRequest = AggregatedTimeSeriesRequest.Parser.ParseFrom(request.Body);

        if (aggregatedTimeSeriesRequest.AggregationLevelCase ==
            AggregatedTimeSeriesRequest.AggregationLevelOneofCase.None)
            throw new InvalidOperationException("Unknown aggregation level");

        return new Energinet.DataHub.Wholesale.Events.Application.InboxEvents.AggregatedTimeSeriesRequest(
            MapPeriod(aggregatedTimeSeriesRequest.Period),
            MapTimeSeriesType(aggregatedTimeSeriesRequest.TimeSeriesType),
            aggregatedTimeSeriesRequest.AggregationLevelCase == AggregatedTimeSeriesRequest.AggregationLevelOneofCase.AggregationPerGridarea ? MapAggregationPerGridArea(aggregatedTimeSeriesRequest.AggregationPerGridarea) : null,
            aggregatedTimeSeriesRequest.AggregationLevelCase == AggregatedTimeSeriesRequest.AggregationLevelOneofCase.AggregationPerEnergysupplierPerGridarea ? MapAggregationPerEnergySupplierPerGridArea(aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerGridarea) : null,
            aggregatedTimeSeriesRequest.AggregationLevelCase == AggregatedTimeSeriesRequest.AggregationLevelOneofCase.AggregationPerBalanceresponsiblepartyPerGridarea ? MapAggregationPerBalanceResponsiblePartyPerGridArea(aggregatedTimeSeriesRequest.AggregationPerBalanceresponsiblepartyPerGridarea) : null,
            aggregatedTimeSeriesRequest.AggregationLevelCase == AggregatedTimeSeriesRequest.AggregationLevelOneofCase.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea ? MapAggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea(aggregatedTimeSeriesRequest.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea) : null);
    }

    /// <summary>
    /// THIS IS ALL MOCKED DATA
    /// </summary>
    public ServiceBusMessage CreateResponse(List<object> aggregatedTimeSeries, string referenceId, bool rejected)
    {
        var body = rejected
            ? CreateRejectedResponse()
            : CreateAcceptedResponse();

        var message = new ServiceBusMessage()
        {
            Body = new BinaryData(body.ToByteArray()),
            Subject = body.GetType().Name,
        };

        message.ApplicationProperties.Add("ReferenceId", referenceId);
        return message;
    }

    private AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea MapAggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea(
        Edi.Requests.AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea aggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea)
    {
        return new AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea(
            aggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea.GridAreaCode,
            aggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea.BalanceResponsiblePartyGlnOrEic,
            aggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea.EnergySupplierGlnOrEic);
    }

    private AggregationPerBalanceResponsiblePartyPerGridArea MapAggregationPerBalanceResponsiblePartyPerGridArea(Edi.Requests.AggregationPerBalanceResponsiblePartyPerGridArea aggregationPerBalanceResponsiblePartyPerGridArea)
    {
        return new AggregationPerBalanceResponsiblePartyPerGridArea(
            aggregationPerBalanceResponsiblePartyPerGridArea.GridAreaCode,
            aggregationPerBalanceResponsiblePartyPerGridArea.BalanceResponsiblePartyGlnOrEic,
            aggregationPerBalanceResponsiblePartyPerGridArea.EnergySupplierGlnOrEic);
    }

    private AggregationPerEnergySupplierPerGridArea MapAggregationPerEnergySupplierPerGridArea(
        Edi.Requests.AggregationPerEnergySupplierPerGridArea aggregationPerEnergySupplierPerGridArea)
    {
        return new AggregationPerEnergySupplierPerGridArea(
            aggregationPerEnergySupplierPerGridArea.GridAreaCode,
            aggregationPerEnergySupplierPerGridArea.BalanceResponsiblePartyGlnOrEic,
            aggregationPerEnergySupplierPerGridArea.EnergySupplierGlnOrEic);
    }

    private AggregationPerGridArea MapAggregationPerGridArea(Edi.Requests.AggregationPerGridArea aggregationPerGridArea)
    {
        return new AggregationPerGridArea(
            aggregationPerGridArea.GridAreaCode, aggregationPerGridArea.GridResponsibleId);
    }

    private Energinet.DataHub.Wholesale.Events.Application.InboxEvents.TimeSeriesType MapTimeSeriesType(Edi.Requests.TimeSeriesType timeSeriesType)
    {
        return timeSeriesType switch
        {
            Edi.Requests.TimeSeriesType.Production => Energinet.DataHub.Wholesale.Events.Application.InboxEvents.TimeSeriesType.Production,
            Edi.Requests.TimeSeriesType.FlexConsumption => Energinet.DataHub.Wholesale.Events.Application.InboxEvents.TimeSeriesType.FlexConsumption,
            Edi.Requests.TimeSeriesType.NonProfiledConsumption => Energinet.DataHub.Wholesale.Events.Application.InboxEvents.TimeSeriesType.NonProfiledConsumption,
            Edi.Requests.TimeSeriesType.NetExchangePerGa => Energinet.DataHub.Wholesale.Events.Application.InboxEvents.TimeSeriesType.NetExchangePerGa,
            Edi.Requests.TimeSeriesType.NetExchangePerNeighboringGa => Energinet.DataHub.Wholesale.Events.Application.InboxEvents.TimeSeriesType.NetExchangePerNeighboringGa,
            Edi.Requests.TimeSeriesType.TotalConsumption => Energinet.DataHub.Wholesale.Events.Application.InboxEvents.TimeSeriesType.TotalConsumption,
            Edi.Requests.TimeSeriesType.Unspecified => throw new InvalidOperationException("Unknown time series type"),
            _ => throw new InvalidOperationException("Unknown time series type"),
        };
    }

    private Application.InboxEvents.Period MapPeriod(Edi.Requests.Period period)
    {
        return new Application.InboxEvents.Period(period.StartOfPeriod.ToInstant(), period.EndOfPeriod.ToInstant());
    }

    private static IMessage CreateRejectedResponse()
    {
        var response = new AggregatedTimeSeriesRequestRejected();
        response.RejectReasons.Add(CreateRejectReason());
        return response;
    }

    private static RejectReason CreateRejectReason()
    {
        return new RejectReason()
        {
            ErrorCode = ErrorCodes.InvalidBalanceResponsibleForPeriod, ErrorMessage = "something went wrong",
        };
    }

    private static IMessage CreateAcceptedResponse()
    {
        var response = new AggregatedTimeSeriesRequestAccepted();
        response.Series.Add(CreateSerie());

        return response;
    }

    private static Serie CreateSerie()
    {
        var quantity = new DecimalValue() { Units = 12345, Nanos = 123450000, };
        var point = new TimeSeriesPoint()
        {
            Quantity = quantity,
            QuantityQuality = QuantityQuality.Incomplete,
            Time = new Timestamp() { Seconds = 1, },
        };

        var period = new Period()
        {
            StartOfPeriod = new Timestamp() { Seconds = 1, },
            EndOfPeriod = new Timestamp() { Seconds = 2, },
            Resolution = Resolution.Pt15M,
        };

        return new Serie()
        {
            GridArea = "543",   // the grid area code of the metered data responsible we have as an actor, when we test in EDI
            QuantityUnit = QuantityUnit.Kwh,
            Period = period,
            TimeSeriesPoints = { point },
            TimeSeriesType = TimeSeriesType.Production,
        };
    }
}
