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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Edi.Mappers;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using NodaTime.Serialization.Protobuf;
using QuantityUnit = Energinet.DataHub.Wholesale.Common.Interfaces.Models.QuantityUnit;
using Resolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution;

namespace Energinet.DataHub.Wholesale.Edi.Factories.WholesaleServices;

public static class WholesaleServiceRequestAcceptedMessageFactory
{
    public static ServiceBusMessage Create(IReadOnlyCollection<WholesaleResult> wholesaleResults, string referenceId)
    {
        var body = CreateAcceptedResponse(wholesaleResults);

        var message = new ServiceBusMessage
        {
            Body = new BinaryData(body.ToByteArray()),
            Subject = body.GetType().Name,
        };

        message.ApplicationProperties.Add("ReferenceId", referenceId);
        return message;
    }

    private static WholesaleServicesRequestAccepted CreateAcceptedResponse(IReadOnlyCollection<WholesaleResult> wholesaleResults)
    {
        var response = new WholesaleServicesRequestAccepted();
        foreach (var series in wholesaleResults)
        {
            var points = CreateTimeSeriesPoints(series.TimeSeriesPoints);
            var wholesaleSeries = new WholesaleServicesRequestSeries
            {
                Period =
                    new Period()
                    {
                        StartOfPeriod = series.PeriodStart.ToTimestamp(),
                        EndOfPeriod = series.PeriodEnd.ToTimestamp(),
                    },
                GridArea = series.GridArea,
                EnergySupplierId = series.EnergySupplierId,
                ChargeCode = series.ChargeCode,
                ChargeType = MapChargeType(series.ChargeType),
                ChargeOwnerId = series.ChargeOwnerId,
                Resolution = MapResolution(series.Resolution),
                QuantityUnit = MapQuantityUnit(series.QuantityUnit),
                Currency = WholesaleServicesRequestSeries.Types.Currency.Dkk,
                TimeSeriesPoints = { points },
                CalculationResultVersion = series.Version,
            };
            if (series.MeteringPointType is not null)
                wholesaleSeries.MeteringPointType = MapMeteringPointType(series.MeteringPointType.Value);

            if (series.SettlementMethod is not null)
                wholesaleSeries.SettlementMethod = MapSettlementMethod(series.SettlementMethod.Value);

            response.Series.Add(wholesaleSeries);
        }

        return response;
    }

    private static WholesaleServicesRequestSeries.Types.SettlementMethod MapSettlementMethod(SettlementMethod seriesSettlementMethod)
    {
        return seriesSettlementMethod switch
        {
            SettlementMethod.Flex => WholesaleServicesRequestSeries.Types.SettlementMethod.Flex,
            SettlementMethod.NonProfiled => WholesaleServicesRequestSeries.Types.SettlementMethod.NonProfiled,
            _ => throw new ArgumentOutOfRangeException(
                nameof(seriesSettlementMethod),
                actualValue: seriesSettlementMethod,
                $"Value cannot be mapped to a {nameof(WholesaleServicesRequestSeries.Types.SettlementMethod)}."),
        };
    }

    private static WholesaleServicesRequestSeries.Types.MeteringPointType MapMeteringPointType(MeteringPointType seriesMeteringPointType)
    {
        return seriesMeteringPointType switch
        {
            MeteringPointType.Consumption => WholesaleServicesRequestSeries.Types.MeteringPointType.Consumption,
            MeteringPointType.Production => WholesaleServicesRequestSeries.Types.MeteringPointType.Production,
            _ => throw new ArgumentOutOfRangeException(
                nameof(seriesMeteringPointType),
                actualValue: seriesMeteringPointType,
                $"Value cannot be mapped to a {nameof(WholesaleServicesRequestSeries.Types.MeteringPointType)}."),
        };
    }

    private static WholesaleServicesRequestSeries.Types.QuantityUnit MapQuantityUnit(QuantityUnit seriesQuantityUnit)
    {
        return seriesQuantityUnit switch
        {
            QuantityUnit.Kwh => WholesaleServicesRequestSeries.Types.QuantityUnit.Kwh,
            QuantityUnit.Pieces => WholesaleServicesRequestSeries.Types.QuantityUnit.Pieces,
            _ => throw new ArgumentOutOfRangeException(
                nameof(seriesQuantityUnit),
                actualValue: seriesQuantityUnit,
                $"Value cannot be mapped to a {nameof(WholesaleServicesRequestSeries.Types.QuantityUnit)}."),
        };
    }

    private static WholesaleServicesRequestSeries.Types.Resolution MapResolution(Resolution seriesResolution)
    {
        return seriesResolution switch
        {
            Resolution.Hour => WholesaleServicesRequestSeries.Types.Resolution.Hour,
            Resolution.Day => WholesaleServicesRequestSeries.Types.Resolution.Day,
            Resolution.Month => WholesaleServicesRequestSeries.Types.Resolution.Monthly,
            _ => throw new ArgumentOutOfRangeException(
                nameof(seriesResolution),
                actualValue: seriesResolution,
                $"Value cannot be mapped to a {nameof(WholesaleServicesRequestSeries.Types.Resolution)}."),
        };
    }

    private static WholesaleServicesRequestSeries.Types.ChargeType MapChargeType(ChargeType seriesChargeType)
    {
        return seriesChargeType switch
        {
            ChargeType.Fee => WholesaleServicesRequestSeries.Types.ChargeType.Fee,
            ChargeType.Subscription => WholesaleServicesRequestSeries.Types.ChargeType.Subscription,
            ChargeType.Tariff => WholesaleServicesRequestSeries.Types.ChargeType.Tariff,
            _ => throw new ArgumentOutOfRangeException(
                nameof(seriesChargeType),
                actualValue: seriesChargeType,
                $"Value cannot be mapped to a {nameof(WholesaleServicesRequestSeries.Types.ChargeType)}."),
        };
    }

    private static IReadOnlyList<WholesaleServicesRequestSeries.Types.Point> CreateTimeSeriesPoints(IReadOnlyCollection<WholesaleTimeSeriesPoint> points)
    {
        return points.Select(CreateTimeSeriesPoint).ToList();
    }

    private static WholesaleServicesRequestSeries.Types.Point CreateTimeSeriesPoint(WholesaleTimeSeriesPoint point)
    {
        var timeSeriesPoint = new WholesaleServicesRequestSeries.Types.Point() { Time = point.Time.ToTimestamp(), };
        if (point.Quantity is not null)
        {
            timeSeriesPoint.Quantity = DecimalValueMapper.Map(point.Quantity.Value);
        }

        if (point.Qualities is not null)
        {
            timeSeriesPoint.QuantityQualities.AddRange(MapQuantityQualities(point.Qualities));
        }

        if (point.Price is not null)
        {
            timeSeriesPoint.Price = DecimalValueMapper.Map(point.Price.Value);
        }

        if (point.Amount is not null)
        {
            timeSeriesPoint.Amount = DecimalValueMapper.Map(point.Amount.Value);
        }

        return timeSeriesPoint;
    }

    private static RepeatedField<Energinet.DataHub.Edi.Responses.QuantityQuality> MapQuantityQualities(IReadOnlyCollection<CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality> pointQualities)
    {
        return new RepeatedField<Energinet.DataHub.Edi.Responses.QuantityQuality>()
        {
            pointQualities.Select(QuantityQualityMapper.MapQuantityQuality),
        };
    }
}
