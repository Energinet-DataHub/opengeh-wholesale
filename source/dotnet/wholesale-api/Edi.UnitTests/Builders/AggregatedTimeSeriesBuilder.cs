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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;

public class AggregatedTimeSeriesBuilder
{
    private readonly IList<EnergyTimeSeriesPoint> _timeSeriesPoints = new List<EnergyTimeSeriesPoint>();
    private Instant _startDate = Instant.FromUtc(2024, 1, 1, 0, 0);
    private Instant _endDate = Instant.FromUtc(2024, 1, 31, 0, 0);
    private string _gridAreaCode = "543";
    private Guid _calculationId = Guid.NewGuid();
    private int _quantity = 0;

    private AggregatedTimeSeriesBuilder()
    {
    }

    public static AggregatedTimeSeriesBuilder AggregatedTimeSeries(CalculationDto calculation)
    {
        return new AggregatedTimeSeriesBuilder()
            .WithCalculation(calculation);
    }

    public static AggregatedTimeSeriesBuilder AggregatedTimeSeries()
    {
        return new AggregatedTimeSeriesBuilder();
    }

    public AggregatedTimeSeries Build()
    {
        var currentTime = _startDate;
        while (currentTime < _endDate)
        {
            _timeSeriesPoints.Add(new EnergyTimeSeriesPoint(currentTime.ToDateTimeOffset(), _quantity, new List<QuantityQuality> { QuantityQuality.Measured }));
            currentTime = currentTime.Plus(Duration.FromMinutes(15));
        }

        return new AggregatedTimeSeries(
            _gridAreaCode,
            _timeSeriesPoints.ToArray(),
            TimeSeriesType.Production,
            ProcessType.Aggregation,
            _calculationId);
    }

    public AggregatedTimeSeriesBuilder WithTimeSeriePointQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public AggregatedTimeSeriesBuilder ForPeriod(Instant periodStart, Instant periodEnd)
    {
        _startDate = periodStart;
        _endDate = periodEnd;
        return this;
    }

    public AggregatedTimeSeriesBuilder WithCalculationId(Guid calculationId)
    {
        _calculationId = calculationId;
        return this;
    }

    private AggregatedTimeSeriesBuilder WithCalculation(CalculationDto calculation)
    {
        _gridAreaCode = calculation.GridAreaCodes.First();
        _startDate = calculation.PeriodStart.ToInstant();
        _endDate = calculation.PeriodEnd.ToInstant();
        _calculationId = calculation.BatchId;
        return this;
    }
}
