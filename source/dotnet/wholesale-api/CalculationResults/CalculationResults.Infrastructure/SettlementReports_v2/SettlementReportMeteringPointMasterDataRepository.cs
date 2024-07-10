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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Experimental;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Persistence.Databricks;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportMeteringPointMasterDataRepository : ISettlementReportMeteringPointMasterDataRepository
{
    private readonly ISettlementReportDatabricksContext _settlementReportDatabricksContext;

    public SettlementReportMeteringPointMasterDataRepository(ISettlementReportDatabricksContext settlementReportDatabricksContext)
    {
        _settlementReportDatabricksContext = settlementReportDatabricksContext;
    }

    public Task<int> CountAsync(SettlementReportRequestFilterDto filter, long maximumCalculationVersion)
    {
        if (filter.CalculationType == CalculationType.BalanceFixing)
        {
            var view = ApplyFilter(_settlementReportDatabricksContext.SettlementReportMeteringPointMasterDataView, filter);
            return filter.EnergySupplier is not null
                ? CountForBalanceFixingPerEnergySupplierAsync(view, ApplyFilter(_settlementReportDatabricksContext.EnergyResultPointsPerEnergySupplierGridAreaView, filter, maximumCalculationVersion))
                : CountForBalanceFixingAsync(view, ApplyFilter(_settlementReportDatabricksContext.EnergyResultPointsPerGridAreaView, filter, maximumCalculationVersion));
        }

        return ApplyFilter(_settlementReportDatabricksContext.SettlementReportMeteringPointMasterDataView, filter)
            .Select(x => x.MeteringPointId)
            .Distinct()
            .DatabricksSqlCountAsync();
    }

    public async IAsyncEnumerable<SettlementReportMeteringPointMasterDataRow> GetAsync(SettlementReportRequestFilterDto filter, int skip, int take, long maximumCalculationVersion)
    {
        var view = ApplyFilter(_settlementReportDatabricksContext.SettlementReportMeteringPointMasterDataView, filter);
        var query = filter.CalculationType == CalculationType.BalanceFixing
            ? filter.EnergySupplier is not null
                ? GetForBalanceFixingPerEnergySupplier(skip, take, view,  ApplyFilter(_settlementReportDatabricksContext.EnergyResultPointsPerEnergySupplierGridAreaView, filter, maximumCalculationVersion))
                : GetForBalanceFixing(skip, take, view, ApplyFilter(_settlementReportDatabricksContext.EnergyResultPointsPerGridAreaView, filter, maximumCalculationVersion))
            : GetForNonBalanceFixing(skip, take, view);

        await foreach (var row in query.AsAsyncEnumerable().ConfigureAwait(false))
        {
            yield return new SettlementReportMeteringPointMasterDataRow(
                row.MeteringPointId,
                MeteringPointTypeMapper.FromDeltaTableValue(row.MeteringPointType),
                row.GridAreaCode,
                row.GridAreaFromCode,
                row.GridAreaToCode,
                SettlementMethodMapper.FromDeltaTableValue(row.SettlementMethod),
                row.EnergySupplierId,
                row.FromDate,
                row.ToDate);
        }
    }

    private static Task<int> CountForBalanceFixingAsync(
        IQueryable<SettlementReportMeteringPointMasterDataViewEntity> view,
        IQueryable<SettlementReportEnergyResultPointsPerGridAreaViewEntity> viewForLatest)
    {
        var latestJoin = viewForLatest
            .GroupBy(row =>
                DatabricksSqlQueryableExtensions.Functions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"))
            .Select(group => new { day = group.Key, max_calc_version = group.Max(row => row.CalculationVersion), });

        var latestCalcIdForMetering = viewForLatest
            .Join(
                latestJoin,
                outer => new
                {
                    day =
                        DatabricksSqlQueryableExtensions.Functions.ToStartOfDayInTimeZone(
                            outer.Time,
                            "Europe/Copenhagen"),
                    max_calc_version = outer.CalculationVersion,
                },
                inner => inner,
                (outer, inner) => new { outer.CalculationId })
            .Distinct();

        var query = view.Join(
                latestCalcIdForMetering,
                outer => outer.CalculationId,
                inner => inner.CalculationId,
                (outer, inner) => outer)
            .OrderBy(row => row.MeteringPointId)
            .Select(row => row.MeteringPointId)
            .Distinct()
            .DatabricksSqlCountAsync();
        return query;
    }

    private static Task<int> CountForBalanceFixingPerEnergySupplierAsync(
        IQueryable<SettlementReportMeteringPointMasterDataViewEntity> view,
        IQueryable<SettlementReportEnergyResultPointsPerEnergySupplierGridAreaViewEntity> viewForLatest)
    {
        var latestJoin = viewForLatest
            .GroupBy(row =>
                DatabricksSqlQueryableExtensions.Functions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"))
            .Select(group => new { day = group.Key, max_calc_version = group.Max(row => row.CalculationVersion), });

        var latestCalcIdForMetering = viewForLatest
            .Join(
                latestJoin,
                outer => new
                {
                    day =
                        DatabricksSqlQueryableExtensions.Functions.ToStartOfDayInTimeZone(
                            outer.Time,
                            "Europe/Copenhagen"),
                    max_calc_version = outer.CalculationVersion,
                },
                inner => inner,
                (outer, inner) => new { outer.CalculationId })
            .Distinct();

        var query = view.Join(
                latestCalcIdForMetering,
                outer => outer.CalculationId,
                inner => inner.CalculationId,
                (outer, inner) => outer)
            .OrderBy(row => row.MeteringPointId)
            .Select(row => row.MeteringPointId)
            .Distinct()
            .DatabricksSqlCountAsync();
        return query;
    }

    private static IQueryable<SettlementReportMeteringPointMasterDataViewEntity> GetForNonBalanceFixing(
        int skip,
        int take,
        IQueryable<SettlementReportMeteringPointMasterDataViewEntity> view)
    {
        var chunkByMeteringPointId = view
            .Select(row => row.MeteringPointId)
            .Distinct()
            .OrderBy(row => row)
            .Skip(skip)
            .Take(take);

        var query = view.Join(
            chunkByMeteringPointId,
            outer => outer.MeteringPointId,
            inner => inner,
            (outer, inner) => outer);
        return query;
    }

    private static IQueryable<SettlementReportMeteringPointMasterDataViewEntity> GetForBalanceFixing(
        int skip,
        int take,
        IQueryable<SettlementReportMeteringPointMasterDataViewEntity> view,
        IQueryable<SettlementReportEnergyResultPointsPerGridAreaViewEntity> viewForLatest)
    {
        var latestJoin = viewForLatest
            .GroupBy(row =>
                DatabricksSqlQueryableExtensions.Functions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"))
            .Select(group => new { day = group.Key, max_calc_version = group.Max(row => row.CalculationVersion), });

        var latestCalcIdForMetering = viewForLatest
            .Join(
                latestJoin,
                outer => new
                {
                    day =
                        DatabricksSqlQueryableExtensions.Functions.ToStartOfDayInTimeZone(
                            outer.Time,
                            "Europe/Copenhagen"),
                    max_calc_version = outer.CalculationVersion,
                },
                inner => inner,
                (outer, inner) => new { outer.CalculationId })
            .Distinct();

        var meteringPointIds = view.Join(
                latestCalcIdForMetering,
                outer => outer.CalculationId,
                inner => inner.CalculationId,
                (outer, inner) => outer)
            .Select(row => row.MeteringPointId)
            .Distinct()
            .OrderBy(row => row)
            .Skip(skip)
            .Take(take);

        var query = view.Join(
            meteringPointIds,
            outer => outer.MeteringPointId,
            inner => inner,
            (outer, inner) => outer)
        .Distinct()
        .OrderBy(row => row.MeteringPointId);

        return query;
    }

    private static IQueryable<SettlementReportMeteringPointMasterDataViewEntity> GetForBalanceFixingPerEnergySupplier(
        int skip,
        int take,
        IQueryable<SettlementReportMeteringPointMasterDataViewEntity> view,
        IQueryable<SettlementReportEnergyResultPointsPerEnergySupplierGridAreaViewEntity> viewForLatest)
    {
        var latestJoin = viewForLatest
            .GroupBy(row =>
                DatabricksSqlQueryableExtensions.Functions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"))
            .Select(group => new { day = group.Key, max_calc_version = group.Max(row => row.CalculationVersion), });

        var latestCalcIdForMetering = viewForLatest
            .Join(
                latestJoin,
                outer => new
                {
                    day =
                        DatabricksSqlQueryableExtensions.Functions.ToStartOfDayInTimeZone(
                            outer.Time,
                            "Europe/Copenhagen"),
                    max_calc_version = outer.CalculationVersion,
                },
                inner => inner,
                (outer, inner) => new { outer.CalculationId })
            .Distinct();

        var meteringPointIds = view.Join(
                latestCalcIdForMetering,
                outer => outer.CalculationId,
                inner => inner.CalculationId,
                (outer, inner) => outer)
            .Select(row => row.MeteringPointId)
            .Distinct()
            .OrderBy(row => row)
            .Skip(skip)
            .Take(take);

        var query = view.Join(
                meteringPointIds,
                outer => outer.MeteringPointId,
                inner => inner,
                (outer, inner) => outer)
            .Distinct()
            .OrderBy(row => row.MeteringPointId);

        return query;
    }

    private static IQueryable<SettlementReportMeteringPointMasterDataViewEntity> ApplyFilter(
        IQueryable<SettlementReportMeteringPointMasterDataViewEntity> source,
        SettlementReportRequestFilterDto filter)
    {
        var (gridAreaCode, calculationId) = filter.GridAreas.Single();

        source = source
            .Where(row => row.GridAreaCode == gridAreaCode)
            .Where(row => row.CalculationType == CalculationTypeMapper.ToDeltaTableValue(filter.CalculationType))
            .Where(row => row.FromDate <= filter.PeriodEnd.ToInstant())
            .Where(row => row.ToDate >= filter.PeriodStart.ToInstant());

        if (filter.CalculationType != CalculationType.BalanceFixing)
        {
            source = source.Where(row => row.CalculationId == calculationId!.Id);
        }

        if (!string.IsNullOrWhiteSpace(filter.EnergySupplier))
        {
            source = source.Where(row => row.EnergySupplierId == filter.EnergySupplier);
        }

        return source;
    }

    private static IQueryable<SettlementReportEnergyResultPointsPerGridAreaViewEntity> ApplyFilter(
        IQueryable<SettlementReportEnergyResultPointsPerGridAreaViewEntity> source,
        SettlementReportRequestFilterDto filter,
        long maximumCalculationVersion)
    {
        var (gridAreaCode, calculationId) = filter.GridAreas.Single();

        source = source
            .Where(row => row.GridAreaCode == gridAreaCode)
            .Where(row => row.CalculationType == CalculationTypeMapper.ToDeltaTableValue(filter.CalculationType))
            .Where(row => row.Time >= filter.PeriodStart.ToInstant())
            .Where(row => row.Time < filter.PeriodEnd.ToInstant())
            .Where(row => row.CalculationVersion <= maximumCalculationVersion);

        return source;
    }

    private static IQueryable<SettlementReportEnergyResultPointsPerEnergySupplierGridAreaViewEntity> ApplyFilter(
        IQueryable<SettlementReportEnergyResultPointsPerEnergySupplierGridAreaViewEntity> source,
        SettlementReportRequestFilterDto filter,
        long maximumCalculationVersion)
    {
        var (gridAreaCode, calculationId) = filter.GridAreas.Single();

        source = source
            .Where(row => row.GridAreaCode == gridAreaCode)
            .Where(row => row.CalculationType == CalculationTypeMapper.ToDeltaTableValue(filter.CalculationType))
            .Where(row => row.Time >= filter.PeriodStart.ToInstant())
            .Where(row => row.Time < filter.PeriodEnd.ToInstant())
            .Where(row => row.CalculationVersion <= maximumCalculationVersion);

        if (!string.IsNullOrWhiteSpace(filter.EnergySupplier))
        {
            source = source.Where(row => row.EnergySupplierId == filter.EnergySupplier);
        }

        return source;
    }
}
