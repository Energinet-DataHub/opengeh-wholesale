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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportChargePriceRepository : ISettlementReportChargePriceRepository
{
    private readonly ISettlementReportDatabricksContext _context;

    public SettlementReportChargePriceRepository(ISettlementReportDatabricksContext context)
    {
        _context = context;
    }

    public Task<int> CountAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo)
    {
        return ApplyFilter(_context.ChargePriceView, filter, actorInfo)
            .Select(row => row.StartTime)
            .Distinct()
            .DatabricksSqlCountAsync();
    }

    public async IAsyncEnumerable<SettlementReportChargePriceRow> GetAsync(
        SettlementReportRequestFilterDto filter,
        SettlementReportRequestedByActor actorInfo,
        int skip,
        int take)
    {
        var view = ApplyFilter(_context.ChargePriceView, filter, actorInfo);

        var chunkByStartTime = view
            .Select(row => row.StartTime)
            .Distinct()
            .OrderBy(row => row)
            .Skip(skip)
            .Take(take);

        var query = view
            .Join(chunkByStartTime, outer => outer.StartTime, inner => inner, (outer, inner) => outer)
            .Select(x => new
            {
                x.ChargeType,
                x.ChargeCode,
                x.ChargeOwnerId,
                x.Resolution,
                x.IsTax,
                x.StartTime,
                x.PricePoints,
            }).Distinct()
            .Select(x => new ChargePriceProjection
            {
                ChargeType = x.ChargeType,
                ChargeCode = x.ChargeCode,
                ChargeOwnerId = x.ChargeOwnerId,
                Resolution = x.Resolution,
                IsTax = x.IsTax,
                StartTime = x.StartTime,
                PricePoints = x.PricePoints,
            });

        await foreach (var row in query.AsAsyncEnumerable().ConfigureAwait(false))
        {
            yield return new SettlementReportChargePriceRow(
                ChargeTypeMapper.FromDeltaTableValue(row.ChargeType),
                row.ChargeCode,
                row.ChargeOwnerId,
                ResolutionMapper.FromDeltaTableValue(row.Resolution),
                row.IsTax,
                row.StartTime,
                row.PricePoints.OrderBy(x => x.Time).Select(x => x.Price).ToList());
        }
    }

    private static IQueryable<SettlementReportChargePriceResultViewEntity> ApplyFilter(
        IQueryable<SettlementReportChargePriceResultViewEntity> source,
        SettlementReportRequestFilterDto filter,
        SettlementReportRequestedByActor actorInfo)
    {
        var (gridAreaCode, calculationId) = filter.GridAreas.Single();

        source = source
            .Where(row => row.GridAreaCode == gridAreaCode)
            .Where(row => row.CalculationType == CalculationTypeMapper.ToDeltaTableValue(filter.CalculationType))
            .Where(row => row.StartTime >= filter.PeriodStart.ToInstant())
            .Where(row => row.CalculationId == calculationId!.Id);

        if (!string.IsNullOrWhiteSpace(filter.EnergySupplier))
        {
            source = source.Where(wholesaleRow => wholesaleRow.EnergySupplierId == filter.EnergySupplier);
        }

        if (actorInfo.MarketRole == MarketRole.SystemOperator)
        {
            source = source.Where(wholesaleRow =>
                wholesaleRow.IsTax == false &&
                wholesaleRow.ChargeOwnerId == actorInfo.ChargeOwnerId);
        }

        if (actorInfo.MarketRole == MarketRole.GridAccessProvider)
        {
            source = source.Where(wholesaleRow =>
                wholesaleRow.IsTax == true ||
                wholesaleRow.ChargeOwnerId == actorInfo.ChargeOwnerId);
        }

        return source;
    }

    private sealed class ChargePriceProjection
    {
        public string ChargeType { get; set; } = null!;

        public string ChargeCode { get; set; } = null!;

        public string ChargeOwnerId { get; set; } = null!;

        public string Resolution { get; set; } = null!;

        public bool IsTax { get; set; }

        public Instant StartTime { get; set; }

        public SettlementReportChargePriceResultViewPricePointEntity[] PricePoints { get; set; } = [];
    }
}
