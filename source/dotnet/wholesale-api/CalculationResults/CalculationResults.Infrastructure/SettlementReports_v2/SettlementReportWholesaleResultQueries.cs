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

public sealed class SettlementReportWholesaleResultQueries : ISettlementReportWholesaleResultQueries
{
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions;
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor;
    private readonly ICalculationsClient _calculationsClient;

    public SettlementReportWholesaleResultQueries(
        IOptions<DeltaTableOptions> deltaTableOptions,
        DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor,
        ICalculationsClient calculationsClient)
    {
        _deltaTableOptions = deltaTableOptions;
        _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
        _calculationsClient = calculationsClient;
    }

    public async Task<int> CountAsync(SettlementReportWholesaleResultQueryFilter filter)
    {
        var statement = new SettlementReportWholesaleResultCountQueryStatement(_deltaTableOptions, filter);

        await foreach (var nextRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement, Format.JsonArray).ConfigureAwait(false))
        {
            var rawValue = new DatabricksSqlRow(nextRow)[SettlementReportWholesaleResultCountQueryStatement.Columns.Count];
            return SqlResultValueConverters.ToInt(rawValue)!.Value;
        }

        return 0;
    }

    public async IAsyncEnumerable<SettlementReportWholesaleResultRow> GetAsync(SettlementReportWholesaleResultQueryFilter filter, int skip, int take)
    {
        var calculation = await _calculationsClient.GetAsync(filter.CalculationId).ConfigureAwait(false);
        var statement = new SettlementReportWholesaleResultQueryStatement(_deltaTableOptions, filter, skip, take);

        await foreach (var nextRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement, Format.JsonArray).ConfigureAwait(false))
        {
            yield return SettlementReportWholesaleResultRowFactory.Create(new DatabricksSqlRow(nextRow), calculation?.Version ?? 1);
        }
    }
}
