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
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportChargeLinkPeriodsRepository : ISettlementReportChargeLinkPeriodsRepository
{
    private readonly ISettlementReportChargeLinkPeriodsQueries _settlementReportResultQueries;

    public SettlementReportChargeLinkPeriodsRepository(ISettlementReportChargeLinkPeriodsQueries settlementReportResultQueries)
    {
        _settlementReportResultQueries = settlementReportResultQueries;
    }

    public Task<int> CountAsync(SettlementReportRequestFilterDto filter)
    {
        return _settlementReportResultQueries.CountAsync(ParseFilter(filter));
    }

    public async IAsyncEnumerable<SettlementReportChargeLinkPeriodsResultRow> GetAsync(SettlementReportRequestFilterDto filter, int skip, int take)
    {
        var rows = _settlementReportResultQueries
            .GetAsync(ParseFilter(filter), skip, take)
            .ConfigureAwait(false);

        await foreach (var row in rows.ConfigureAwait(false))
        {
            yield return new SettlementReportChargeLinkPeriodsResultRow(
                row.MeteringPointId,
                row.MeteringPointType,
                row.ChargeType,
                row.ChargeOwnerId,
                row.ChargeCode,
                row.Quantity,
                row.PeriodStart,
                row.PeriodEnd);
        }
    }

    private static SettlementReportChargeLinkPeriodQueryFilter ParseFilter(SettlementReportRequestFilterDto filter)
    {
        var (gridAreaCode, calculationId) = filter.GridAreas.Single();

        return new SettlementReportChargeLinkPeriodQueryFilter(
            calculationId.Id,
            gridAreaCode,
            filter.PeriodStart.ToInstant(),
            filter.PeriodEnd.ToInstant());
    }
}
