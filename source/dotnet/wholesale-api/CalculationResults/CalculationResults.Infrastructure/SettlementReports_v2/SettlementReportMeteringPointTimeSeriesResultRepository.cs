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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Extensions;

using DbFunctions = Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Experimental.DatabricksSqlQueryableExtensions.Functions;
using Resolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.Resolution;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportMeteringPointTimeSeriesResultRepository : ISettlementReportMeteringPointTimeSeriesResultRepository
{
    private readonly ISettlementReportDatabricksContext _settlementReportDatabricksContext;
    private readonly ILogger<SettlementReportMeteringPointTimeSeriesResultRepository> _logger;

    public SettlementReportMeteringPointTimeSeriesResultRepository(
        ISettlementReportDatabricksContext settlementReportDatabricksContext,
        ILoggerFactory loggerFactory)
    {
        _settlementReportDatabricksContext = settlementReportDatabricksContext;
        _logger = loggerFactory.CreateLogger<SettlementReportMeteringPointTimeSeriesResultRepository>();
    }

    public Task<int> CountAsync(SettlementReportRequestFilterDto filter, long maximumCalculationVersion, Resolution resolution)
    {
        if (filter.CalculationType == CalculationType.BalanceFixing)
        {
            return CountLatestAsync(filter, maximumCalculationVersion, resolution);
        }

        var (_, calculationId) = filter.GridAreas.Single();

        return ApplyFilter(_settlementReportDatabricksContext.MeteringPointTimeSeriesView, filter, resolution)
            .Where(row => row.CalculationId == calculationId!.Id)
            .Select(row => row.MeteringPointId)
            .Distinct()
            .DatabricksSqlCountAsync();
    }

    public async IAsyncEnumerable<SettlementReportMeteringPointTimeSeriesResultRow> GetAsync(SettlementReportRequestFilterDto filter, long maximumCalculationVersion, Resolution resolution, int skip, int take)
    {
        IAsyncEnumerable<AggregatedByDay> rows;

        if (filter.CalculationType == CalculationType.BalanceFixing)
        {
            rows = GetLatestAsync(filter, maximumCalculationVersion, resolution, skip, take);
        }
        else
        {
            var view = ApplyFilter(_settlementReportDatabricksContext.MeteringPointTimeSeriesView, filter, resolution);

            var (_, calculationId) = filter.GridAreas.Single();
            view = view.Where(row => row.CalculationId == calculationId!.Id);

            var chunkByMeteringPointId = view
                .Select(row => row.MeteringPointId)
                .Distinct()
                .OrderBy(row => row)
                .Skip(skip)
                .Take(take);

            var query =
                from row in view
                join meteringPointId in chunkByMeteringPointId on row.MeteringPointId equals meteringPointId
                group row by new
                {
                    row.MeteringPointId,
                    row.MeteringPointType,
                    start_of_day = DbFunctions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"),
                }
                into meteringPointGroup
                select new AggregatedByDay
                {
                    MeteringPointId = meteringPointGroup.Key.MeteringPointId,
                    MeteringPointType = meteringPointGroup.Key.MeteringPointType,
                    StartOfDay = DbFunctions.ToUtcFromTimeZoned(meteringPointGroup.Key.start_of_day, "Europe/Copenhagen"),
                    Quantities = DbFunctions.AggregateFields(meteringPointGroup.First().Time, meteringPointGroup.First().Quantity),
                };

            rows = query.AsAsyncEnumerable();
        }

        await foreach (var row in rows.ConfigureAwait(false))
        {
            yield return new SettlementReportMeteringPointTimeSeriesResultRow(
                row.MeteringPointId,
                MeteringPointTypeMapper.FromDeltaTableValueNonNull(row.MeteringPointType),
                row.StartOfDay,
                row.Quantities
                    .Select(quant => new SettlementReportMeteringPointTimeSeriesResultQuantity(quant.Time, quant.Quantity))
                    .ToList());
        }

        _logger.LogInformation("Queried MeteringPointTimeSeriesView {Skip} and {Take} with {Filter}", skip, take, filter);
    }

    private Task<int> CountLatestAsync(SettlementReportRequestFilterDto filter, long maximumCalculationVersion, Resolution resolution)
    {
        var view = ApplyFilter(_settlementReportDatabricksContext.MeteringPointTimeSeriesView, filter, resolution);

        var dailyCalculationVersion = view
            .Where(row => row.CalculationVersion <= maximumCalculationVersion)
            .GroupBy(row => DbFunctions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"))
            .Select(group => new
            {
                start_of_day = group.Key,
                max_calc_version = group.Max(row => row.CalculationVersion),
            });

        var dailyMeteringPoints =
            from row in view
            join calculationVersion in dailyCalculationVersion on
                new { start_of_day = DbFunctions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"), max_calc_version = row.CalculationVersion }
                equals
                new { calculationVersion.start_of_day, calculationVersion.max_calc_version }
            select new
            {
                calculationVersion.start_of_day,
                row.CalculationId,
                row.MeteringPointId,
            };

        return dailyMeteringPoints
            .Distinct()
            .DatabricksSqlCountAsync();
    }

    private IAsyncEnumerable<AggregatedByDay> GetLatestAsync(SettlementReportRequestFilterDto filter, long maximumCalculationVersion, Resolution resolution, int skip, int take)
    {
        var view = ApplyFilter(_settlementReportDatabricksContext.MeteringPointTimeSeriesView, filter, resolution);

        var dailyCalculationVersion = view
            .Where(row => row.CalculationVersion <= maximumCalculationVersion)
            .GroupBy(row => DbFunctions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"))
            .Select(group => new
            {
                start_of_day = group.Key,
                max_calc_version = group.Max(row => row.CalculationVersion),
            });

        var dailyMeteringPoints =
            from row in view
            join calculationVersion in dailyCalculationVersion on
                new { start_of_day = DbFunctions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"), max_calc_version = row.CalculationVersion }
                equals
                new { calculationVersion.start_of_day, calculationVersion.max_calc_version }
            select new
            {
                calculationVersion.start_of_day,
                row.CalculationId,
                row.MeteringPointId,
            };

        var chunkByDailyMeteringPoints = dailyMeteringPoints
            .Distinct()
            .OrderBy(row => row.start_of_day)
            .ThenBy(row => row.CalculationId)
            .ThenBy(row => row.MeteringPointId)
            .Skip(skip)
            .Take(take);

        var query =
            from row in view
            join dailyMeteringPointIds in chunkByDailyMeteringPoints on
                new { start_of_day = DbFunctions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"), row.CalculationId, row.MeteringPointId }
                equals dailyMeteringPointIds
            group row by new
            {
                row.MeteringPointId,
                row.MeteringPointType,
                start_of_day = DbFunctions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"),
            }
            into meteringPointGroup
            select new AggregatedByDay
            {
                MeteringPointId = meteringPointGroup.Key.MeteringPointId,
                MeteringPointType = meteringPointGroup.Key.MeteringPointType,
                StartOfDay = DbFunctions.ToUtcFromTimeZoned(meteringPointGroup.Key.start_of_day, "Europe/Copenhagen"),
                Quantities = DbFunctions.AggregateFields(meteringPointGroup.First().Time, meteringPointGroup.First().Quantity),
            };

        return query.AsAsyncEnumerable();
    }

    private static IQueryable<SettlementReportMeteringPointTimeSeriesEntity> ApplyFilter(
        IQueryable<SettlementReportMeteringPointTimeSeriesEntity> source,
        SettlementReportRequestFilterDto filter,
        Resolution resolution)
    {
        var viewResolution = resolution switch
        {
            Resolution.Hour => "PT1H",
            Resolution.Quarter => "PT15M",
            _ => throw new ArgumentOutOfRangeException(nameof(resolution)),
        };

        var (gridAreaCode, _) = filter.GridAreas.Single();

        source = source
            .Where(row => row.GridAreaCode == gridAreaCode)
            .Where(row => row.CalculationType == CalculationTypeMapper.ToDeltaTableValue(filter.CalculationType))
            .Where(row => row.Time >= filter.PeriodStart.ToInstant())
            .Where(row => row.Time < filter.PeriodEnd.ToInstant())
            .Where(row => row.Resolution == viewResolution);

        if (!string.IsNullOrWhiteSpace(filter.EnergySupplier))
        {
            source = source.Where(row => row.EnergySupplierId == filter.EnergySupplier);
        }

        return source;
    }

    private sealed class AggregatedByDay
    {
        public string MeteringPointId { get; set; } = null!;

        public string MeteringPointType { get; set; } = null!;

        public Instant StartOfDay { get; set; }

        public IEnumerable<DatabricksSqlQueryableExtensions.TimeQuantityStruct> Quantities { get; set; } = null!;
    }
}
