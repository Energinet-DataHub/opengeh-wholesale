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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using NodaTime;
using NodaTime.Extensions;
using EnergyResultsResolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.Resolution;
using WholesaleResolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;

public static class PeriodHelper
{
    public static (Instant Start, Instant End) GetPeriod(IReadOnlyCollection<EnergyTimeSeriesPoint> timeSeriesPoints, EnergyResultsResolution resolution)
    {
        var start = timeSeriesPoints.Min(x => x.Time);
        // The end date is the start of the next period.
        var end = GetEndOfPeriod(resolution, timeSeriesPoints.Max(x => x.Time));
        return (start.ToInstant(), end.ToInstant());
    }

    public static (Instant Start, Instant End) GetPeriod(IReadOnlyCollection<WholesaleTimeSeriesPoint> timeSeriesPoints, Interfaces.CalculationResults.Model.WholesaleResults.Resolution resolution)
    {
        var start = timeSeriesPoints.Min(x => x.Time);
        // The end date is the start of the next period.
        var end = GetEndOfPeriod(resolution, timeSeriesPoints.Max(x => x.Time), start);
        return (start.ToInstant(), end.ToInstant());
    }

    private static DateTimeOffset GetEndOfPeriod(EnergyResultsResolution resolution, DateTimeOffset lastPointInPeriod) => resolution switch
    {
        EnergyResultsResolution.Quarter => lastPointInPeriod.AddMinutes(15),
        _ => lastPointInPeriod.AddMinutes(60),
    };

    private static DateTimeOffset GetEndOfPeriod(WholesaleResolution resolution, DateTimeOffset lastPointInPeriod, DateTimeOffset firstPointInPeriod) => resolution switch
    {
        WholesaleResolution.Hour => lastPointInPeriod.AddMinutes(60),
        WholesaleResolution.Day => lastPointInPeriod.AddDays(1),
        _ => firstPointInPeriod.AddMonths(1),
    };
}
