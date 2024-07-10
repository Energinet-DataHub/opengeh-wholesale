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
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Extensions;

using DbFunctions = Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Experimental.DatabricksSqlQueryableExtensions.Functions;
using Resolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.Resolution;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportMeteringPointTimeSeriesResultRepository : ISettlementReportMeteringPointTimeSeriesResultRepository
{
    private readonly ISettlementReportDatabricksContext _settlementReportDatabricksContext;

    public SettlementReportMeteringPointTimeSeriesResultRepository(ISettlementReportDatabricksContext settlementReportDatabricksContext)
    {
        _settlementReportDatabricksContext = settlementReportDatabricksContext;
    }

    public Task<int> CountAsync(SettlementReportRequestFilterDto filter, Resolution resolution)
    {
        return ApplyFilter(_settlementReportDatabricksContext.MeteringPointTimeSeriesView, filter, resolution)
            .Select(row => row.MeteringPointId)
            .Distinct()
            .DatabricksSqlCountAsync();
    }

    public async IAsyncEnumerable<SettlementReportMeteringPointTimeSeriesResultRow> GetAsync(SettlementReportRequestFilterDto filter, Resolution resolution, int skip, int take)
    {
        var view = ApplyFilter(_settlementReportDatabricksContext.MeteringPointTimeSeriesView, filter, resolution);

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
                start_of_day = DbFunctions.ToUtcFromTimeZoned(DbFunctions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"), "Europe/Copenhagen"),
            }
            into meteringPointGroup
            select new GroupedResult
            {
                MeteringPointId = meteringPointGroup.Key.MeteringPointId,
                MeteringPointType = meteringPointGroup.Key.MeteringPointType,
                StartOfDay = meteringPointGroup.Key.start_of_day,
                Quantities = DbFunctions.AggregateFields(meteringPointGroup.First().Time, meteringPointGroup.First().Quantity),
            };

        await foreach (var row in query.AsAsyncEnumerable().ConfigureAwait(false))
        {
            yield return new SettlementReportMeteringPointTimeSeriesResultRow(
                row.MeteringPointId,
                MeteringPointTypeMapper.FromDeltaTableValueNonNull(row.MeteringPointType),
                row.StartOfDay,
                row.Quantities
                    .Select(quant => new SettlementReportMeteringPointTimeSeriesResultQuantity(quant.Time, quant.Quantity))
                    .ToList());
        }
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

        var (gridAreaCode, calculationId) = filter.GridAreas.Single();

        source = source
            .Where(row => row.GridAreaCode == gridAreaCode)
            .Where(row => row.CalculationType == CalculationTypeMapper.ToDeltaTableValue(filter.CalculationType))
            .Where(row => row.Time >= filter.PeriodStart.ToInstant())
            .Where(row => row.Time < filter.PeriodEnd.ToInstant())
            .Where(row => row.Resolution == viewResolution)
            .Where(row => row.CalculationId == calculationId!.Id);

        if (!string.IsNullOrWhiteSpace(filter.EnergySupplier))
        {
            source = source.Where(row => row.EnergySupplierId == filter.EnergySupplier);
        }

        return source;
    }

    private sealed class GroupedResult
    {
        public string MeteringPointId { get; set; } = null!;

        public string MeteringPointType { get; set; } = null!;

        public Instant StartOfDay { get; set; }

        public IEnumerable<DatabricksSqlQueryableExtensions.AggregatedStruct> Quantities { get; set; } = null!;
    }
}
