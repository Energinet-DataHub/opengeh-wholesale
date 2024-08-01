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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public class WholesaleServicesQueryStatement(
    WholesaleServicesQueryStatement.StatementType statementType,
    IReadOnlyCollection<CalculationTypeForGridArea> calculationTypePerGridAreas,
    WholesaleServicesQueryStatementHelper helper,
    DeltaTableOptions deltaTableOptions)
    : DatabricksStatement
{
    private readonly StatementType _statementType = statementType;
    private readonly IReadOnlyCollection<CalculationTypeForGridArea> _calculationTypePerGridAreas = calculationTypePerGridAreas;
    private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;
    private readonly WholesaleServicesQueryStatementHelper _helper = helper;

    protected override string GetSqlStatement()
    {
        var selectTarget = _statementType switch
        {
            StatementType.Select => _helper.GetProjection("wrv"),
            StatementType.Exists => "1",
            _ => throw new ArgumentOutOfRangeException(nameof(_statementType), _statementType, "Unknown StatementType"),
        };

        return $"""
                SELECT {selectTarget}
                FROM (SELECT {_helper.GetProjection("wr")}
                FROM {_helper.GetSource(_deltaTableOptions)} wr
                WHERE {_helper.GetLatestOrFixedCalculationTypeSelection("wr", _calculationTypePerGridAreas)}) wrv
                INNER JOIN (SELECT max({_helper.GetCalculationVersionColumnName()}) AS max_version, {_helper.GetTimeColumnName()} AS max_time, {string.Join(", ", _helper.GetColumnsToAggregateBy().Select(ctgb => $"{ctgb} AS max_{ctgb}"))}
                FROM {_helper.GetSource(_deltaTableOptions)} wr
                WHERE {_helper.GetSelection("wr")} AND {_helper.GetLatestOrFixedCalculationTypeSelection("wr", _calculationTypePerGridAreas)}
                GROUP BY {_helper.GetTimeColumnName()}, {string.Join(", ", _helper.GetColumnsToAggregateBy())}) maxver
                ON wrv.{_helper.GetTimeColumnName()} = maxver.max_time AND wrv.{_helper.GetCalculationVersionColumnName()} = maxver.max_version AND {string.Join(" AND ", _helper.GetColumnsToAggregateBy().Select(ctgb => $"coalesce(wrv.{ctgb}, 'is_null_value') = coalesce(maxver.max_{ctgb}, 'is_null_value')"))}
                ORDER BY {string.Join(", ", _helper.GetColumnsToAggregateBy())}, {_helper.GetTimeColumnName()}
                """;
    }

    public enum StatementType
    {
        Select,
        Exists,
    }
}
