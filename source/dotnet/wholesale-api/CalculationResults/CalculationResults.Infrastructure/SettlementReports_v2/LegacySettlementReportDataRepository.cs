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

using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

// This implementation redirects to the old SQL statement implementation.
public sealed class LegacySettlementReportDataRepository : ISettlementReportDataRepository
{
    private readonly ISettlementReportResultQueries _settlementReportResultQueries;

    public LegacySettlementReportDataRepository(ISettlementReportResultQueries settlementReportResultQueries)
    {
        _settlementReportResultQueries = settlementReportResultQueries;
    }

    public async IAsyncEnumerable<SettlementReportResultRow> TryReadBalanceFixingResultsAsync(SettlementReportRequestFilterDto filter)
    {
        IEnumerable<Interfaces.SettlementReports.Model.SettlementReportResultRow> rows;

        try
        {
            //rows = await _settlementReportResultQueries
            //    .GetRowsAsync(
            //        filter.GridAreas.Select(gridArea => gridArea.Code).ToArray(),
            //        CalculationType.BalanceFixing,
            //        filter.PeriodStart.ToInstant(),
            //        filter.PeriodEnd.ToInstant(),
            //        null)
            //    .ConfigureAwait(false);
            await Task.Delay(100).ConfigureAwait(false);
            rows = [new Interfaces.SettlementReports.Model.SettlementReportResultRow("042", CalculationType.BalanceFixing, Instant.FromDateTimeOffset(DateTimeOffset.UtcNow), "PT15M", null, null, 42)];
        }
        catch (Exception ex)
        {
            throw new TimeoutException(ISettlementReportDataRepository.DataSourceUnavailableExceptionMessage, ex);
        }

        foreach (var row in rows)
        {
            var resolution = row.Resolution == "PT15M"
                ? Resolution.QuarterHour
                : throw new InvalidOperationException($"Resolution {row.Resolution} is not supported in legacy mode.");

            yield return new SettlementReportResultRow(
                row.Time,
                row.Quantity,
                new GridAreaCode(row.GridArea),
                resolution,
                row.MeteringPointType,
                row.SettlementMethod);
        }
    }
}
