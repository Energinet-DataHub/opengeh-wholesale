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

public sealed class SettlementReportMonthlyAmountTotalRepository : ISettlementReportMonthlyAmountTotalRepository
{
    private readonly ISettlementReportMonthlyAmountTotalQueries _settlementReportResultQueries;

    public SettlementReportMonthlyAmountTotalRepository(ISettlementReportMonthlyAmountTotalQueries settlementReportResultQueries)
    {
        _settlementReportResultQueries = settlementReportResultQueries;
    }

    public Task<int> CountAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestInputActorInfo actorInfo)
    {
        return _settlementReportResultQueries.CountAsync(ParseFilter(filter, actorInfo));
    }

    public async IAsyncEnumerable<SettlementReportMonthlyAmountRow> GetAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestInputActorInfo actorInfo, int skip, int take)
    {
        var rows = _settlementReportResultQueries
            .GetAsync(ParseFilter(filter, actorInfo), skip, take)
            .ConfigureAwait(false);

        await foreach (var row in rows.ConfigureAwait(false))
        {
            yield return new SettlementReportMonthlyAmountRow(
                row.CalculationType,
                row.GridArea,
                row.EnergySupplierId,
                row.StartDateTime,
                row.Resolution,
                row.QuantityUnit,
                row.Currency,
                row.Amount,
                row.ChargeType,
                row.ChargeCode,
                row.ChargeOwnerId);
        }
    }

    private static SettlementReportMonthlyAmountQueryFilter ParseFilter(SettlementReportRequestFilterDto filter, SettlementReportRequestInputActorInfo actorInfo)
    {
        var (gridAreaCode, calculationId) = filter.GridAreas.Single();

        return new SettlementReportMonthlyAmountQueryFilter(
            calculationId!.Id,
            gridAreaCode,
            filter.CalculationType,
            filter.EnergySupplier,
            actorInfo.ChargeOwnerId,
            filter.PeriodStart.ToInstant(),
            filter.PeriodEnd.ToInstant(),
            actorInfo.MarketRole);
    }
}
