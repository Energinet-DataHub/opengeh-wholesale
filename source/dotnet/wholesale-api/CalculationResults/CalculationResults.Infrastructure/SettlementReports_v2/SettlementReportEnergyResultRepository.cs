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

public sealed class SettlementReportEnergyResultRepository : ISettlementReportEnergyResultRepository
{
    private readonly ISettlementReportEnergyResultQueries _settlementReportResultQueries;

    public SettlementReportEnergyResultRepository(ISettlementReportEnergyResultQueries settlementReportResultQueries)
    {
        _settlementReportResultQueries = settlementReportResultQueries;
    }

    public Task<int> CountAsync(SettlementReportRequestFilterDto filter)
    {
        if (filter.CalculationType == CalculationType.BalanceFixing)
        {
            return filter.EnergySupplier is not null
                ? _settlementReportResultQueries.CountAsync(ParseLatestEnergyFilterWithEnergySupplier(filter))
                : _settlementReportResultQueries.CountAsync(ParseLatestEnergyFilter(filter));
        }

        return filter.EnergySupplier is not null
            ? _settlementReportResultQueries.CountAsync(ParseEnergyFilterWithEnergySupplier(filter))
            : _settlementReportResultQueries.CountAsync(ParseEnergyFilter(filter));
    }

    public async IAsyncEnumerable<SettlementReportEnergyResultRow> GetAsync(SettlementReportRequestFilterDto filter, int skip, int take)
    {
        IAsyncEnumerable<Interfaces.SettlementReports.Model.SettlementReportEnergyResultRow> rows;

        if (filter.CalculationType == CalculationType.BalanceFixing)
        {
            rows = filter.EnergySupplier is not null
                ? _settlementReportResultQueries.GetAsync(ParseLatestEnergyFilterWithEnergySupplier(filter), skip, take)
                : _settlementReportResultQueries.GetAsync(ParseLatestEnergyFilter(filter), skip, take);
        }
        else
        {
            rows = filter.EnergySupplier is not null
                ? _settlementReportResultQueries.GetAsync(ParseEnergyFilterWithEnergySupplier(filter), skip, take)
                : _settlementReportResultQueries.GetAsync(ParseEnergyFilter(filter), skip, take);
        }

        await foreach (var row in rows.ConfigureAwait(false))
        {
            yield return new SettlementReportEnergyResultRow(
                row.StartDateTime,
                row.Quantity,
                row.GridArea,
                row.Resolution,
                row.CalculationType,
                row.MeteringPointType,
                row.SettlementMethod,
                row.EnergySupplierId);
        }
    }

    private static SettlementReportEnergyResultQueryFilter ParseEnergyFilter(SettlementReportRequestFilterDto filter)
    {
        var (gridAreaCode, calculationId) = filter.GridAreas.Single();

        return new SettlementReportEnergyResultQueryFilter(
            calculationId.Id,
            gridAreaCode,
            filter.PeriodStart.ToInstant(),
            filter.PeriodEnd.ToInstant());
    }

    private static SettlementReportLatestEnergyResultQueryFilter ParseLatestEnergyFilter(SettlementReportRequestFilterDto filter)
    {
        var (gridAreaCode, _) = filter.GridAreas.Single();

        return new SettlementReportLatestEnergyResultQueryFilter(
            gridAreaCode,
            filter.PeriodStart.ToInstant(),
            filter.PeriodEnd.ToInstant());
    }

    private static SettlementReportEnergyResultPerEnergySupplierQueryFilter ParseEnergyFilterWithEnergySupplier(SettlementReportRequestFilterDto filter)
    {
        var (gridAreaCode, calculationId) = filter.GridAreas.Single();

        return new SettlementReportEnergyResultPerEnergySupplierQueryFilter(
            calculationId.Id,
            gridAreaCode,
            filter.EnergySupplier!,
            filter.PeriodStart.ToInstant(),
            filter.PeriodEnd.ToInstant());
    }

    private static SettlementReportLatestEnergyResultPerEnergySupplierQueryFilter ParseLatestEnergyFilterWithEnergySupplier(SettlementReportRequestFilterDto filter)
    {
        var (gridAreaCode, _) = filter.GridAreas.Single();

        return new SettlementReportLatestEnergyResultPerEnergySupplierQueryFilter(
            gridAreaCode,
            filter.EnergySupplier!,
            filter.PeriodStart.ToInstant(),
            filter.PeriodEnd.ToInstant());
    }
}
