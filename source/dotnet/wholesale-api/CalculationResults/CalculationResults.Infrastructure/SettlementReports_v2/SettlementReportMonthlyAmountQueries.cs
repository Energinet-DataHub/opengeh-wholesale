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
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportMonthlyAmountQueries : ISettlementReportMonthlyAmountQueries
{
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions;
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor;

    public SettlementReportMonthlyAmountQueries(
        IOptions<DeltaTableOptions> deltaTableOptions,
        DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor)
    {
        _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
        _deltaTableOptions = deltaTableOptions;
    }

    public async IAsyncEnumerable<SettlementReportMonthlyAmountRow> GetAsync(SettlementReportMonthlyAmountQueryFilter filter, int skip, int take)
    {
        var statement = new SettlementReportMonthlyAmountQueryStatement(_deltaTableOptions, filter, skip, take);

        await foreach (var nextRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement, Format.JsonArray).ConfigureAwait(false))
        {
            yield return SettlementReportMonthlyAmountRowFactory.Create(new DatabricksSqlRow(nextRow));
        }
    }

    public async Task<int> CountAsync(SettlementReportMonthlyAmountQueryFilter filter)
    {
        var statement = new SettlementReportMonthlyAmountCountQueryStatement(_deltaTableOptions, filter);

        await foreach (var nextRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement, Format.JsonArray).ConfigureAwait(false))
        {
            var rawValue = new DatabricksSqlRow(nextRow)[SettlementReportMonthlyAmountCountQueryStatement.Columns.Count];
            return SqlResultValueConverters.ToInt(rawValue)!.Value;
        }

        throw new InvalidOperationException("Could not count result for SettlementReportMonthlyAmountQueries.");
    }
}
