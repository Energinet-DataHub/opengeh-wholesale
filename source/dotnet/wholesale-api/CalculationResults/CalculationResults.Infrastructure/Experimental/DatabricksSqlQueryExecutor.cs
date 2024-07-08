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

using System.ComponentModel.DataAnnotations.Schema;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Experimental;

public sealed class DatabricksSqlQueryExecutor
{
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor;
    private readonly DatabricksSqlQueryBuilder _sqlQueryBuilder;
    private readonly DatabricksSqlRowHydrator _sqlRowHydrator;

    public DatabricksSqlQueryExecutor(DbContext dbContext, DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor)
    {
        _sqlQueryBuilder = new DatabricksSqlQueryBuilder(dbContext);
        _sqlRowHydrator = new DatabricksSqlRowHydrator();
        _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
    }

    public async Task<int> CountAsync(DatabricksSqlQueryable query, CancellationToken cancellationToken = default)
    {
        var countStatement = _sqlQueryBuilder.Build(query, sqlQuery => $"SELECT COUNT(*) AS count FROM ({sqlQuery})");
        var rows = ExecuteStatementAsync<CountResult>(countStatement, cancellationToken);

        await foreach (var row in rows.ConfigureAwait(false))
            return row.Count;

        return 0;
    }

    public IAsyncEnumerable<TElement> ExecuteAsync<TElement>(DatabricksSqlQueryable query, CancellationToken cancellationToken = default)
    {
        var databricksStatement = _sqlQueryBuilder.Build(query);
        return ExecuteStatementAsync<TElement>(databricksStatement, cancellationToken);
    }

    private IAsyncEnumerable<TElement> ExecuteStatementAsync<TElement>(DatabricksStatement databricksStatement, CancellationToken cancellationToken = default)
    {
        var rows = _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(databricksStatement, Format.JsonArray, cancellationToken);
        return _sqlRowHydrator.HydrateAsync<TElement>(rows, cancellationToken);
    }

    private sealed class CountResult
    {
        [Column("count")]
        public int Count { get; set; }
    }
}
