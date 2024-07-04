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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Experimental;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Persistence.Databricks;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.EnergyResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportEnergyResultRepository : ISettlementReportEnergyResultRepository
{
    private readonly ISettlementReportDatabricksContext _settlementReportDatabricksContext;

    public SettlementReportEnergyResultRepository(ISettlementReportDatabricksContext settlementReportDatabricksContext)
    {
        _settlementReportDatabricksContext = settlementReportDatabricksContext;
    }

    public Task<int> CountAsync(SettlementReportRequestFilterDto filter, long maximumCalculationVersion)
    {
        if (filter.EnergySupplier is not null)
        {
            if (filter.CalculationType == CalculationType.BalanceFixing)
            {
                return ApplyFilter(_settlementReportDatabricksContext.EnergyResultPointsPerEnergySupplierGridAreaView, filter, maximumCalculationVersion)
                    .Select(row => DatabricksSqlQueryableExtensions.Functions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"))
                    .Distinct()
                    .DatabricksSqlCountAsync();
            }

            return ApplyFilter(_settlementReportDatabricksContext.EnergyResultPointsPerEnergySupplierGridAreaView, filter, maximumCalculationVersion)
                .Select(row => row.ResultId)
                .Distinct()
                .DatabricksSqlCountAsync();
        }

        if (filter.CalculationType == CalculationType.BalanceFixing)
        {
            return ApplyFilter(_settlementReportDatabricksContext.EnergyResultPointsPerGridAreaView, filter, maximumCalculationVersion)
                .Select(row => DatabricksSqlQueryableExtensions.Functions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"))
                .Distinct()
                .DatabricksSqlCountAsync();
        }

        return ApplyFilter(_settlementReportDatabricksContext.EnergyResultPointsPerGridAreaView, filter, maximumCalculationVersion)
            .Select(row => row.ResultId)
            .Distinct()
            .DatabricksSqlCountAsync();
    }

    public IAsyncEnumerable<SettlementReportEnergyResultRow> GetAsync(SettlementReportRequestFilterDto filter, long maximumCalculationVersion, int skip, int take)
    {
        return filter.EnergySupplier is not null
            ? GetForEnergySupplierAsync(filter, maximumCalculationVersion, skip, take)
            : GetWithoutEnergySupplierAsync(filter, maximumCalculationVersion, skip, take);
    }

    private async IAsyncEnumerable<SettlementReportEnergyResultRow> GetWithoutEnergySupplierAsync(SettlementReportRequestFilterDto filter, long maximumCalculationVersion, int skip, int take)
    {
        IAsyncEnumerable<SettlementReportEnergyResultPointsPerGridAreaViewEntity> rows;

        var filteredView = ApplyFilter(_settlementReportDatabricksContext.EnergyResultPointsPerGridAreaView, filter, maximumCalculationVersion);

        if (filter.CalculationType == CalculationType.BalanceFixing)
        {
            var chunkByDate = filteredView
                .GroupBy(row => DatabricksSqlQueryableExtensions.Functions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"))
                .Select(group => new
                {
                    max_calc_version = group.Max(row => row.CalculationVersion),
                    start_of_day = group.Key,
                })
                .OrderBy(row => row.start_of_day)
                .Skip(skip)
                .Take(take);

            rows = filteredView
                .Join(
                    chunkByDate,
                    outer => new { max_calc_version = outer.CalculationVersion, start_of_day = DatabricksSqlQueryableExtensions.Functions.ToStartOfDayInTimeZone(outer.Time, "Europe/Copenhagen") },
                    inner => inner,
                    (outer, inner) => outer)
                .AsAsyncEnumerable();
        }
        else
        {
            var chunkByResultId = filteredView
                .Select(row => row.ResultId)
                .Distinct()
                .OrderBy(resultId => resultId)
                .Skip(skip)
                .Take(take);

            rows = filteredView
                .Join(
                    chunkByResultId,
                    outer => outer.ResultId,
                    inner => inner,
                    (outer, inner) => outer)
                .AsAsyncEnumerable();
        }

        await foreach (var row in rows.ConfigureAwait(false))
        {
            yield return new SettlementReportEnergyResultRow(
                row.Time,
                row.Quantity,
                row.GridAreaCode,
                ResolutionMapper.FromDeltaTableValue(row.Resolution),
                CalculationTypeMapper.FromDeltaTableValue(row.CalculationType),
                MeteringPointTypeMapper.FromDeltaTableValueNonNull(row.MeteringPointType),
                SettlementMethodMapper.FromDeltaTableValue(row.SettlementMethod),
                null);
        }
    }

    private async IAsyncEnumerable<SettlementReportEnergyResultRow> GetForEnergySupplierAsync(SettlementReportRequestFilterDto filter, long maximumCalculationVersion, int skip, int take)
    {
        IAsyncEnumerable<SettlementReportEnergyResultPointsPerEnergySupplierGridAreaViewEntity> rows;

        var filteredView = ApplyFilter(_settlementReportDatabricksContext.EnergyResultPointsPerEnergySupplierGridAreaView, filter, maximumCalculationVersion);

        if (filter.CalculationType == CalculationType.BalanceFixing)
        {
            var chunkByDate = filteredView
                .GroupBy(row => DatabricksSqlQueryableExtensions.Functions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"))
                .Select(group => new
                {
                    max_calc_version = group.Max(row => row.CalculationVersion),
                    start_of_day = group.Key,
                })
                .OrderBy(row => row.start_of_day)
                .Skip(skip)
                .Take(take);

            rows = filteredView
                .Join(
                    chunkByDate,
                    outer => new { max_calc_version = outer.CalculationVersion, start_of_day = DatabricksSqlQueryableExtensions.Functions.ToStartOfDayInTimeZone(outer.Time, "Europe/Copenhagen") },
                    inner => inner,
                    (outer, inner) => outer)
                .AsAsyncEnumerable();
        }
        else
        {
            var chunkByResultId = filteredView
                .Select(row => row.ResultId)
                .Distinct()
                .OrderBy(resultId => resultId)
                .Skip(skip)
                .Take(take);

            rows = filteredView
                .Join(
                    chunkByResultId,
                    outer => outer.ResultId,
                    inner => inner,
                    (outer, inner) => outer)
                .AsAsyncEnumerable();
        }

        await foreach (var row in rows.ConfigureAwait(false))
        {
            yield return new SettlementReportEnergyResultRow(
                row.Time,
                row.Quantity,
                row.GridAreaCode,
                ResolutionMapper.FromDeltaTableValue(row.Resolution),
                CalculationTypeMapper.FromDeltaTableValue(row.CalculationType),
                MeteringPointTypeMapper.FromDeltaTableValueNonNull(row.MeteringPointType),
                SettlementMethodMapper.FromDeltaTableValue(row.SettlementMethod),
                row.EnergySupplierId);
        }
    }

    private static IQueryable<SettlementReportEnergyResultPointsPerGridAreaViewEntity> ApplyFilter(
        IQueryable<SettlementReportEnergyResultPointsPerGridAreaViewEntity> source,
        SettlementReportRequestFilterDto filter,
        long maximumCalculationVersion)
    {
        var (gridAreaCode, _) = filter.GridAreas.Single();

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
        var (gridAreaCode, _) = filter.GridAreas.Single();

        source = source
            .Where(row => row.GridAreaCode == gridAreaCode)
            .Where(row => row.CalculationType == CalculationTypeMapper.ToDeltaTableValue(filter.CalculationType))
            .Where(row => row.Time >= filter.PeriodStart.ToInstant())
            .Where(row => row.Time < filter.PeriodEnd.ToInstant())
            .Where(row => row.CalculationVersion <= maximumCalculationVersion)
            .Where(row => row.EnergySupplierId == filter.EnergySupplier);

        return source;
    }
}
