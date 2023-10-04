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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.EDI.Mappers;
using FluentValidation.Results;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using PeriodContract = Energinet.DataHub.Edi.Responses.Period;
using TimeSeriesPoint = Energinet.DataHub.Edi.Responses.TimeSeriesPoint;

namespace Energinet.DataHub.Wholesale.EDI.Factories;

public class AggregatedTimeSeriesMessageFactory : IAggregatedTimeSeriesMessageFactory
{
    public ServiceBusMessage Create(EnergyResult? calculationResult, string referenceId)
    {
        var body = calculationResult is null
            ? CreateRejectedResponse()
            : CreateAcceptedResponse(calculationResult);

        var message = new ServiceBusMessage()
        {
            Body = new BinaryData(body.ToByteArray()),
            Subject = body.GetType().Name,
        };

        message.ApplicationProperties.Add("ReferenceId", referenceId);
        return message;
    }

    public ServiceBusMessage CreateRejected(List<ValidationFailure> errors, string referenceId)
    {
        var body = CreateRejected(errors);

        var message = new ServiceBusMessage()
        {
            Body = new BinaryData(body.ToByteArray()),
            Subject = body.GetType().Name,
        };

        message.ApplicationProperties.Add("ReferenceId", referenceId);
        return message;
    }

    private static IMessage CreateRejected(List<ValidationFailure> errors)
    {
        var response = new AggregatedTimeSeriesRequestRejected();
        response.RejectReasons.AddRange(errors.Select(CreateRejectReasonFromError));
        return response;
    }

    private static RejectReason CreateRejectReasonFromError(ValidationFailure error)
    {
        //TODO: Do we want such a mapper? Do we want to define every possible error code in our rejected contract?
        return new RejectReason() { ErrorCode = ErrorCodeMapper.MapErrorCode(error.ErrorCode), ErrorMessage = error.ErrorMessage, };
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

    private static AggregatedTimeSeriesRequestAccepted CreateAcceptedResponse(EnergyResult energyResult)
    {
        var points = CreateTimeSeriesPoints(energyResult);

        var period = new PeriodContract()
        {
            StartOfPeriod = new Timestamp() { Seconds = energyResult.PeriodStart.ToUnixTimeSeconds(), },
            EndOfPeriod = new Timestamp() { Seconds = energyResult.PeriodEnd.ToUnixTimeSeconds(), },
            Resolution = Resolution.Pt15M,
        };

        return new AggregatedTimeSeriesRequestAccepted()
        {
            GridArea = energyResult.GridArea,
            QuantityUnit = QuantityUnit.Kwh,
            Period = period,
            TimeSeriesPoints = { points },
            TimeSeriesType = TimeSeriesTypeMapper.MapTimeSeriesTypeFromCalculationsResult(energyResult.TimeSeriesType),
        };
    }

    private static IList<TimeSeriesPoint> CreateTimeSeriesPoints(EnergyResult energyResult)
    {
        const decimal nanoFactor = 1_000_000_000;
        var points = new List<TimeSeriesPoint>();
        foreach (var timeSeriesPoint in energyResult.TimeSeriesPoints)
        {
            var units = decimal.ToInt64(timeSeriesPoint.Quantity);
            var nanos = decimal.ToInt32((timeSeriesPoint.Quantity - units) * nanoFactor);
            var point = new TimeSeriesPoint()
            {
                Quantity = new DecimalValue() { Units = units, Nanos = nanos },
                QuantityQuality = QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality),
                Time = new Timestamp() { Seconds = timeSeriesPoint.Time.ToUnixTimeSeconds(), },
            };
            points.Add(point);
        }

        return points;
    }
}
