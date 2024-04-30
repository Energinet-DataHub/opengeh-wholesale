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
        var rows = await _settlementReportResultQueries
            .GetRowsAsync(
                filter.GridAreas.Select(gridArea => gridArea.Code).ToArray(),
                CalculationType.BalanceFixing,
                filter.PeriodStart.ToInstant(),
                filter.PeriodEnd.ToInstant(),
                null)
            .ConfigureAwait(false);

        foreach (var row in rows)
        {
            yield return new SettlementReportResultRow(
                row.Time,
                row.Quantity,
                new GridAreaCode(row.GridArea),
                row.MeteringPointType,
                row.SettlementMethod);
        }
    }
}
