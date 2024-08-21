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
using InternalPeriod = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.Period;
using WholesaleResolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;

public static class PeriodHelper
{
    private static readonly DateTimeZone _dkTimeZone = DateTimeZoneProviders.Tzdb["Europe/Copenhagen"];

    public static (Instant Start, Instant End) GetPeriod(IReadOnlyCollection<EnergyTimeSeriesPoint> timeSeriesPoints, EnergyResultsResolution resolution)
    {
        var start = timeSeriesPoints.Min(x => x.Time);
        var end = timeSeriesPoints.Max(x => x.Time);
        // The end date is the start of the next period.
        var endWithResolutionOffset = GetDateTimeWithResolutionOffset(resolution, end);
        return (start.ToInstant(), endWithResolutionOffset.ToInstant());
    }

    public static InternalPeriod GetPeriod(IReadOnlyCollection<WholesaleTimeSeriesPoint> timeSeriesPoints, Interfaces.CalculationResults.Model.WholesaleResults.Resolution resolution)
    {
        var start = timeSeriesPoints.Min(x => x.Time);
        var end = timeSeriesPoints.Max(x => x.Time);
        // The end date is the start of the next period.
        var endWithResolutionOffset = GetDateTimeWithResolutionOffset(resolution, end);
        return new InternalPeriod(start.ToInstant(), endWithResolutionOffset.ToInstant());
    }

    public static DateTimeOffset GetDateTimeWithResolutionOffset(EnergyResultsResolution resolution, DateTimeOffset dateTime) => resolution switch
    {
        EnergyResultsResolution.Quarter => dateTime.AddMinutes(15),
        _ => dateTime.AddMinutes(60),
    };

    public static DateTimeOffset GetDateTimeWithResolutionOffset(WholesaleResolution resolution, DateTimeOffset dateTime) => resolution switch
    {
        WholesaleResolution.Hour => dateTime.AddMinutes(60),
        WholesaleResolution.Day => dateTime.AddDays(1),
        _ => AddAMonth(dateTime.ToInstant()),
    };

    private static DateTimeOffset AddAMonth(Instant dateTime)
    {
        var timeForLatestPointInLocalTime = dateTime.InZone(_dkTimeZone).LocalDateTime;
        var endAtMidnightInLocalTime = timeForLatestPointInLocalTime.PlusMonths(1).Date.AtMidnight();
        var endAtMidnightInUtc = endAtMidnightInLocalTime.InZoneStrictly(_dkTimeZone);
        return endAtMidnightInUtc.ToDateTimeOffset();
    }
}
