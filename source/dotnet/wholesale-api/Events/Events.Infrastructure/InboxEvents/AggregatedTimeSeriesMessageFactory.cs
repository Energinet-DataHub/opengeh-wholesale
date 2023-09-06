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

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.InboxEvents;

public class AggregatedTimeSeriesMessageFactory : IAggregatedTimeSeriesMessageFactory
{
    /// <summary>
    /// THIS IS ALL MOCKED DATA
    /// </summary>
    public ServiceBusMessage Create(List<object> aggregatedTimeSeries)
    {
        var body = aggregatedTimeSeries.Any()
            ? CreateAcceptedResponse()
            : CreateRejectedResponse();

        var message = new ServiceBusMessage()
        {
            Body = new BinaryData(body.ToByteArray()),
            Subject = body.GetType().Name,
            MessageId = Guid.NewGuid().ToString(),
        };

        message.ApplicationProperties.Add("ReferenceId", Guid.NewGuid().ToString());
        return message;
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
