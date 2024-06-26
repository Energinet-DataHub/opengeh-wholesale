﻿// Copyright 2020 Energinet DataHub A/S
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

public sealed class SettlementReportWholesaleRepository : ISettlementReportWholesaleRepository
{
    private readonly ISettlementReportWholesaleResultQueries _settlementReportResultQueries;

    public SettlementReportWholesaleRepository(ISettlementReportWholesaleResultQueries settlementReportResultQueries)
    {
        _settlementReportResultQueries = settlementReportResultQueries;
    }

    public Task<int> CountAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo)
    {
        return _settlementReportResultQueries.CountAsync(ParseFilter(filter, actorInfo));
    }

    public async IAsyncEnumerable<SettlementReportWholesaleResultRow> GetAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, int skip, int take)
    {
        var rows = _settlementReportResultQueries
            .GetAsync(ParseFilter(filter, actorInfo), skip, take)
            .ConfigureAwait(false);

        await foreach (var row in rows.ConfigureAwait(false))
        {
            yield return new SettlementReportWholesaleResultRow(
                row.CalculationType,
                row.GridArea,
                row.EnergySupplierId,
                row.StartDateTime,
                row.Resolution,
                row.MeteringPointType,
                row.SettlementMethod,
                row.QuantityUnit,
                row.Currency,
                row.Quantity,
                row.Price,
                row.Amount,
                row.ChargeType,
                row.ChargeCode,
                row.ChargeOwnerId);
        }
    }

    private static SettlementReportWholesaleResultQueryFilter ParseFilter(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo)
    {
        var (gridAreaCode, calculationId) = filter.GridAreas.Single();

        return new SettlementReportWholesaleResultQueryFilter(
            calculationId!.Id,
            gridAreaCode,
            filter.CalculationType,
            filter.PeriodStart.ToInstant(),
            filter.PeriodEnd.ToInstant(),
            filter.EnergySupplier,
            actorInfo.ChargeOwnerId,
            actorInfo.MarketRole);
    }
}
