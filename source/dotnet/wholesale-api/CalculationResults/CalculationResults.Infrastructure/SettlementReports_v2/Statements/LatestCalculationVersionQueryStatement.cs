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
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;

public sealed class LatestCalculationVersionQueryStatement : DatabricksStatement
{
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions;

    public LatestCalculationVersionQueryStatement(IOptions<DeltaTableOptions> deltaTableOptions)
    {
        _deltaTableOptions = deltaTableOptions;
    }

    protected override string GetSqlStatement()
    {
        return $"""
                    SELECT {Columns.CalculationVersion}
                    FROM
                        {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.CURRENT_BALANCE_FIXING_CALCULATION_VERSION_VIEW_NAME}
                """;
    }

    public static class Columns
    {
        public const string CalculationVersion = "calculation_version";
    }
}
