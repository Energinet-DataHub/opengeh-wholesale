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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;
using Energinet.DataHub.Wholesale.Common.DatabricksClient;
using Energinet.DataHub.Wholesale.Common.Models;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;

public class SettlementReportResultQueries : ISettlementReportResultQueries
{
    private readonly ISqlStatementClient _sqlStatementClient;
    private readonly DatabricksOptions _databricksOptions;

    public SettlementReportResultQueries(ISqlStatementClient sqlStatementClient, IOptions<DatabricksOptions> databricksOptions)
    {
        _sqlStatementClient = sqlStatementClient;
        _databricksOptions = databricksOptions.Value;
    }

    public async Task<IEnumerable<SettlementReportResultRow>> GetRowsAsync(
        string[] gridAreaCodes,
        ProcessType processType,
        Instant periodStart,
        Instant periodEnd,
        string? energySupplier)
    {
        var sql = SettlementReportSqlStatementFactory.Create(_databricksOptions.SCHEMA_NAME, _databricksOptions.RESULT_TABLE_NAME, gridAreaCodes, processType, periodStart, periodEnd, energySupplier);
        var rows = await _sqlStatementClient.ExecuteAsync(sql).ToListAsync().ConfigureAwait(false);
        return SettlementReportDataFactory.Create(rows);
    }
}
