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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportEnergyResultQueries : ISettlementReportEnergyResultQueries
{
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions;
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor;
    private readonly ICalculationsClient _calculationsClient;

    public SettlementReportEnergyResultQueries(
        IOptions<DeltaTableOptions> deltaTableOptions,
        DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor,
        ICalculationsClient calculationsClient)
    {
        _deltaTableOptions = deltaTableOptions;
        _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
        _calculationsClient = calculationsClient;
    }

    public Task<int> CountAsync(SettlementReportEnergyResultQueryFilter filter)
    {
        return InternalCountAsync(new SettlementReportEnergyResultCountQueryStatement(_deltaTableOptions, filter));
    }

    public Task<int> CountAsync(SettlementReportEnergyResultPerEnergySupplierQueryFilter filter)
    {
        return InternalCountAsync(new SettlementReportEnergyResultPerEnergySupplierCountQueryStatement(_deltaTableOptions, filter));
    }

    public Task<int> CountAsync(SettlementReportLatestEnergyResultQueryFilter filter)
    {
        return InternalCountAsync(new SettlementReportLatestEnergyResultCountQueryStatement(_deltaTableOptions, filter));
    }

    public Task<int> CountAsync(SettlementReportLatestEnergyResultPerEnergySupplierQueryFilter filter)
    {
        return InternalCountAsync(new SettlementReportLatestEnergyResultPerEnergySupplierCountQueryStatement(_deltaTableOptions, filter));
    }

    public IAsyncEnumerable<SettlementReportEnergyResultRow> GetAsync(SettlementReportEnergyResultQueryFilter filter, int skip, int take)
    {
        return InternalGetAsync(filter.CalculationId, new SettlementReportEnergyResultQueryStatement(_deltaTableOptions, filter, skip, take), isEnergySupplierIncluded: false);
    }

    public IAsyncEnumerable<SettlementReportEnergyResultRow> GetAsync(SettlementReportEnergyResultPerEnergySupplierQueryFilter filter, int skip, int take)
    {
        return InternalGetAsync(filter.CalculationId, new SettlementReportEnergyResultPerEnergySupplierQueryStatement(_deltaTableOptions, filter, skip, take), isEnergySupplierIncluded: true);
    }

    public async IAsyncEnumerable<SettlementReportEnergyResultRow> GetAsync(SettlementReportLatestEnergyResultQueryFilter filter, int skip, int take)
    {
        var statement = new SettlementReportLatestEnergyResultQueryStatement(_deltaTableOptions, filter, skip, take);
        var calcVersions = new Dictionary<string, long>();

        await foreach (var nextRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement, Format.JsonArray).ConfigureAwait(false))
        {
            var databricksSqlRow = new DatabricksSqlRow(nextRow);
            var calculationId = databricksSqlRow[SettlementReportEnergyResultViewColumns.CalculationId]!;

            if (!calcVersions.TryGetValue(calculationId, out var calculationVersion))
            {
                var calculation = await _calculationsClient.GetAsync(Guid.Parse(calculationId)).ConfigureAwait(false);
                calcVersions[calculationId] = calculationVersion = calculation.Version;
            }

            yield return SettlementReportEnergyResultRowFactory.Create(new DatabricksSqlRow(nextRow), calculationVersion, false);
        }
    }

    public async IAsyncEnumerable<SettlementReportEnergyResultRow> GetAsync(SettlementReportLatestEnergyResultPerEnergySupplierQueryFilter filter, int skip, int take)
    {
        var statement = new SettlementReportLatestEnergyResultPerEnergySupplierQueryStatement(_deltaTableOptions, filter, skip, take);
        var calcVersions = new Dictionary<string, long>();

        await foreach (var nextRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement, Format.JsonArray).ConfigureAwait(false))
        {
            var databricksSqlRow = new DatabricksSqlRow(nextRow);
            var calculationId = databricksSqlRow[SettlementReportEnergyResultViewColumns.CalculationId]!;

            if (!calcVersions.TryGetValue(calculationId, out var calculationVersion))
            {
                var calculation = await _calculationsClient.GetAsync(Guid.Parse(calculationId)).ConfigureAwait(false);
                calcVersions[calculationId] = calculationVersion = calculation.Version;
            }

            yield return SettlementReportEnergyResultRowFactory.Create(new DatabricksSqlRow(nextRow), calculationVersion, false);
        }
    }

    private async IAsyncEnumerable<SettlementReportEnergyResultRow> InternalGetAsync(Guid calculationId, DatabricksStatement statement, bool isEnergySupplierIncluded)
    {
        var calculation = await _calculationsClient.GetAsync(calculationId).ConfigureAwait(false);
        await foreach (var nextRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement, Format.JsonArray).ConfigureAwait(false))
        {
            yield return SettlementReportEnergyResultRowFactory.Create(new DatabricksSqlRow(nextRow), calculation.Version, isEnergySupplierIncluded);
        }
    }

    private async Task<int> InternalCountAsync(DatabricksStatement statement)
    {
        await foreach (var nextRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement, Format.JsonArray).ConfigureAwait(false))
        {
            var rawValue = new DatabricksSqlRow(nextRow)[SettlementReportEnergyResultCountColumns.Count];
            return SqlResultValueConverters.ToInt(rawValue)!.Value;
        }

        throw new InvalidOperationException("Could not count result for SettlementReportEnergyResultQueries.");
    }
}
