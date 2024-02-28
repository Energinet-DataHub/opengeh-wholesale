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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;

public class SettlementReportResultQueries : ISettlementReportResultQueries
{
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor;
    private readonly DeltaTableOptions _deltaTableOptions;

    public SettlementReportResultQueries(DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor, IOptions<DeltaTableOptions> deltaTableOptions)
    {
        _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
        _deltaTableOptions = deltaTableOptions.Value;
    }

    public async Task<IEnumerable<SettlementReportResultRow>> GetRowsAsync(
        string[] gridAreaCodes,
        CalculationType calculationType,
        Instant periodStart,
        Instant periodEnd,
        string? energySupplier)
    {
        var statement = new QuerySettlementReportStatement(_deltaTableOptions.SCHEMA_NAME, _deltaTableOptions.ENERGY_RESULTS_TABLE_NAME, gridAreaCodes, calculationType, periodStart, periodEnd, energySupplier);
        var rows = await _databricksSqlWarehouseQueryExecutor
            .ExecuteStatementAsync(statement, Format.JsonArray)
            .ToListAsync()
            .ConfigureAwait(false);

        var databricksSqlRows = rows.Select(x => new DatabricksSqlRow(x));
        return SettlementReportDataFactory.Create(databricksSqlRows);
    }
}
