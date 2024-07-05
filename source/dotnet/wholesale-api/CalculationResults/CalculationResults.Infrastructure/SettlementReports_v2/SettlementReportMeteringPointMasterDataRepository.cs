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

using System.Runtime.CompilerServices;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportMeteringPointMasterDataRepository : ISettlementReportMeteringPointMasterDataRepository
{
    private readonly ISettlementReportMeteringPointMasterDataQueries _settlementReportResultQueries;

    public SettlementReportMeteringPointMasterDataRepository(ISettlementReportMeteringPointMasterDataQueries settlementReportResultQueries)
    {
        _settlementReportResultQueries = settlementReportResultQueries;
    }

    public Task<int> CountAsync(SettlementReportRequestFilterDto filter, long maximumCalculationVersion)
    {
        if (filter.CalculationType == CalculationType.BalanceFixing)
        {
            return filter.EnergySupplier is not null
                ? CountLatestPerEnergySupplierAsync(filter, maximumCalculationVersion)
                : CountLatestAsync(filter, maximumCalculationVersion);
        }

        return _settlementReportResultQueries.CountAsync(ParseFilter(filter, maximumCalculationVersion));
    }

    public async IAsyncEnumerable<SettlementReportMeteringPointMasterDataRow> GetAsync(SettlementReportRequestFilterDto filter, int skip, int take, long maximumCalculationVersion)
    {
        ConfiguredCancelableAsyncEnumerable<Interfaces.SettlementReports.Model.SettlementReportMeteringPointMasterDataRow> rows;

        if (filter.CalculationType == CalculationType.BalanceFixing)
        {
            rows = filter.EnergySupplier is not null
                ? GetLatestPerEnergySupplier(filter, skip, take, maximumCalculationVersion)
                : GetLatest(filter, skip, take, maximumCalculationVersion);
        }
        else
        {
            rows = _settlementReportResultQueries
                .GetAsync(ParseFilter(filter, maximumCalculationVersion), skip, take)
                .ConfigureAwait(false);
        }

        await foreach (var row in rows.ConfigureAwait(false))
        {
            yield return new SettlementReportMeteringPointMasterDataRow(
                row.MeteringPointId,
                row.MeteringPointType,
                row.GridAreaId,
                row.GridAreaFromId,
                row.GridAreaToId,
                row.SettlementMethod,
                row.EnergySupplierId,
                row.PeriodStart,
                row.PeriodEnd);
        }
    }

    private Task<int> CountLatestAsync(SettlementReportRequestFilterDto filter, long maximumCalculationVersion)
    {
        return _settlementReportResultQueries.CountLatestAsync(ParseFilter(filter, maximumCalculationVersion));
    }

    private Task<int> CountLatestPerEnergySupplierAsync(SettlementReportRequestFilterDto filter, long maximumCalculationVersion)
    {
        return _settlementReportResultQueries.CountLatestPerEnergySupplierAsync(ParseFilter(filter, maximumCalculationVersion));
    }

    private ConfiguredCancelableAsyncEnumerable<Interfaces.SettlementReports.Model.SettlementReportMeteringPointMasterDataRow> GetLatest(SettlementReportRequestFilterDto filter, int skip, int take, long maximumCalculationVersion)
    {
        var rows = _settlementReportResultQueries
            .GetLatestAsync(ParseFilter(filter, maximumCalculationVersion), skip, take)
            .ConfigureAwait(false);

        return rows;
    }

    private ConfiguredCancelableAsyncEnumerable<Interfaces.SettlementReports.Model.SettlementReportMeteringPointMasterDataRow> GetLatestPerEnergySupplier(SettlementReportRequestFilterDto filter, int skip, int take, long maximumCalculationVersion)
    {
        var rows = _settlementReportResultQueries
            .GetLatestPerEnergySupplierAsync(ParseFilter(filter, maximumCalculationVersion), skip, take)
            .ConfigureAwait(false);

        return rows;
    }

    private static SettlementReportMeteringPointMasterDataQueryFilter ParseFilter(SettlementReportRequestFilterDto filter, long maximumCalculationVersion)
    {
        var (gridAreaCode, calculationId) = filter.GridAreas.Single();

        return new SettlementReportMeteringPointMasterDataQueryFilter(
            calculationId!.Id,
            gridAreaCode,
            filter.CalculationType,
            filter.EnergySupplier,
            filter.PeriodStart.ToInstant(),
            filter.PeriodEnd.ToInstant(),
            maximumCalculationVersion);
    }
}
