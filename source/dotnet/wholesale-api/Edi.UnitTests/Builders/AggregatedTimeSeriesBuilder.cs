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

using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;

public class AggregatedTimeSeriesBuilder
{
    private readonly string _gridAreaCode;
    private readonly IList<EnergyTimeSeriesPoint> _timeSeriesPoints = new List<EnergyTimeSeriesPoint>();
    private readonly Guid _batchId;

    private AggregatedTimeSeriesBuilder(CalculationDto calculation)
    {
        _gridAreaCode = calculation.GridAreaCodes.First();
        _batchId = calculation.BatchId;
        var currentTime = calculation.PeriodStart.ToInstant();
        while (currentTime < calculation.PeriodEnd.ToInstant())
        {
            _timeSeriesPoints.Add(new EnergyTimeSeriesPoint(currentTime.ToDateTimeOffset(), 0, new List<QuantityQuality> { QuantityQuality.Measured }));
            currentTime = currentTime.Plus(Duration.FromMinutes(15));
        }
    }

    public static AggregatedTimeSeriesBuilder AggregatedTimeSeries(CalculationDto calculation)
    {
        return new AggregatedTimeSeriesBuilder(calculation);
    }

    public AggregatedTimeSeries Build()
    {
        return new AggregatedTimeSeries(
            _gridAreaCode,
            _timeSeriesPoints.ToArray(),
            TimeSeriesType.Production,
            ProcessType.Aggregation,
            _batchId);
    }
}
