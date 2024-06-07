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

using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;

public class AggregatedTimeSeries
{
    public AggregatedTimeSeries(
        string gridArea,
        EnergyTimeSeriesPoint[] timeSeriesPoints,
        TimeSeriesType timeSeriesType,
        CalculationType calculationType,
        Instant periodStart,
        Instant periodEnd,
        Resolution resolution,
        long version)
    {
        if (timeSeriesPoints.Length == 0)
            throw new ArgumentException($"{nameof(timeSeriesPoints)} are empty.");

        GridArea = gridArea;
        TimeSeriesPoints = timeSeriesPoints;
        TimeSeriesType = timeSeriesType;
        CalculationType = calculationType;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        Resolution = resolution;
        Version = version;
    }

    public string GridArea { get; init; }

    /// <summary>
    /// Time series points for the period excluding the end time
    /// </summary>
    public EnergyTimeSeriesPoint[] TimeSeriesPoints { get; init; }

    public TimeSeriesType TimeSeriesType { get; init; }

    public CalculationType CalculationType { get; init; }

    public Instant PeriodStart { get; init; }

    /// <summary>
    /// The point are exclusive the end time.
    /// </summary>
    public Instant PeriodEnd { get; init; }

    public Resolution Resolution { get; }

    public long Version { get; init; }
}
