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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportWholesaleRepository : ISettlementReportWholesaleRepository
{
    private readonly ISettlementReportDatabricksContext _settlementReportDatabricksContext;

    public SettlementReportWholesaleRepository(ISettlementReportDatabricksContext settlementReportDatabricksContext)
    {
        _settlementReportDatabricksContext = settlementReportDatabricksContext;
    }

    public Task<int> CountAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo)
    {
        var view = ApplyFilter(_settlementReportDatabricksContext.WholesaleView, filter, actorInfo);
        return view
            .Select(row => row.ResultId)
            .Distinct()
            .DatabricksSqlCountAsync();
    }

    public async IAsyncEnumerable<SettlementReportWholesaleResultRow> GetAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, int skip, int take)
    {
        var view = ApplyFilter(_settlementReportDatabricksContext.WholesaleView, filter, actorInfo);

        var chunkByCalculationResult = view
            .Select(row => row.ResultId)
            .Distinct()
            .OrderBy(row => row)
            .Skip(skip)
            .Take(take);

        var query = view.Join(
            chunkByCalculationResult,
            outer => outer.ResultId,
            inner => inner,
            (outer, inner) => outer);

        await foreach (var row in query.AsAsyncEnumerable().ConfigureAwait(false))
        {
            yield return new SettlementReportWholesaleResultRow(
                CalculationTypeMapper.FromDeltaTableValue(row.CalculationType),
                row.GridAreaCode,
                row.EnergySupplierId,
                row.Time,
                ResolutionMapper.FromDeltaTableValue(row.Resolution),
                MeteringPointTypeMapper.FromDeltaTableValueNonNull(row.MeteringPointType),
                SettlementMethodMapper.FromDeltaTableValue(row.SettlementMethod),
                QuantityUnitMapper.FromDeltaTableValue(row.QuantityUnit),
                Currency.DKK,
                row.Quantity,
                row.Price,
                row.Amount,
                ChargeTypeMapper.FromDeltaTableValue(row.ChargeType),
                row.ChargeCode,
                row.ChargeOwnerId);
        }
    }

    private static IQueryable<SettlementReportWholesaleViewEntity> ApplyFilter(
        IQueryable<SettlementReportWholesaleViewEntity> source,
        SettlementReportRequestFilterDto filter,
        SettlementReportRequestedByActor actorInfo)
    {
        var (gridAreaCode, calculationId) = filter.GridAreas.Single();

        source = source
            .Where(row => row.GridAreaCode == gridAreaCode)
            .Where(row => row.CalculationType == CalculationTypeMapper.ToDeltaTableValue(filter.CalculationType))
            .Where(row => row.Time >= filter.PeriodStart.ToInstant())
            .Where(row => row.Time < filter.PeriodEnd.ToInstant())
            .Where(row => row.CalculationId == calculationId!.Id);

        if (actorInfo.MarketRole == MarketRole.SystemOperator)
        {
            source = source.Where(wholesaleRow =>
                wholesaleRow.IsTax == false &&
                wholesaleRow.ChargeOwnerId == actorInfo.ChargeOwnerId);
        }
        else if (actorInfo.MarketRole == MarketRole.GridAccessProvider)
        {
            source = source.Where(wholesaleRow =>
                wholesaleRow.IsTax == true ||
                wholesaleRow.ChargeOwnerId == actorInfo.ChargeOwnerId);
        }

        if (!string.IsNullOrWhiteSpace(filter.EnergySupplier))
        {
            source = source.Where(wholesaleRow => wholesaleRow.EnergySupplierId == filter.EnergySupplier);
        }

        return source;
    }
}
