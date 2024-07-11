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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.IdentityModel.Tokens;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public class WholesaleServicesQueryStatement(
    WholesaleServicesQueryStatement.StatementType statementType,
    WholesaleServicesQueryParameters queryParameters,
    IReadOnlyCollection<CalculationTypeForGridArea> calculationTypePerGridAreas,
    WholesaleServicesQueryStatementHelper helper,
    DeltaTableOptions deltaTableOptions)
    : DatabricksStatement
{
    private readonly StatementType _statementType = statementType;
    private readonly WholesaleServicesQueryParameters _queryParameters = queryParameters;
    private readonly IReadOnlyCollection<CalculationTypeForGridArea> _calculationTypePerGridAreas = calculationTypePerGridAreas;
    private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;
    private readonly WholesaleServicesQueryStatementHelper _helper = helper;

    protected override string GetSqlStatement()
    {
        var selectTarget = _statementType switch
        {
            StatementType.Select => _helper.GetProjection("wrv", _queryParameters.AmountType),
            StatementType.Exists => "1",
            _ => throw new ArgumentOutOfRangeException(nameof(_statementType), _statementType, "Unknown StatementType"),
        };

        var sql = $"""
                    SELECT {selectTarget}
                    FROM (SELECT {_helper.GetProjection("wr", _queryParameters.AmountType)}
                    FROM {_helper.GetSource(_queryParameters.AmountType, _deltaTableOptions)} wr
                    WHERE {GenerateLatestOrFixedCalculationTypeConstraint("wr")}) wrv
                    INNER JOIN (SELECT max({_helper.GetCalculationVersionColumnName(_queryParameters.AmountType)}) AS max_version, {_helper.GetTimeColumnName(_queryParameters.AmountType)} AS max_time, {string.Join(", ", _helper.GetColumnsToAggregateBy(_queryParameters.AmountType).Select(ctgb => $"{ctgb} AS max_{ctgb}"))}
                    FROM {_helper.GetSource(_queryParameters.AmountType, _deltaTableOptions)} wr
                    {_helper.GetWhereClauseSqlExpression(_queryParameters, "wr")} AND {GenerateLatestOrFixedCalculationTypeConstraint("wr")}
                    GROUP BY {_helper.GetTimeColumnName(_queryParameters.AmountType)}, {string.Join(", ", _helper.GetColumnsToAggregateBy(_queryParameters.AmountType))}) maxver
                    ON wrv.{_helper.GetTimeColumnName(_queryParameters.AmountType)} = maxver.max_time AND wrv.{_helper.GetCalculationVersionColumnName(_queryParameters.AmountType)} = maxver.max_version AND {string.Join(" AND ", _helper.GetColumnsToAggregateBy(_queryParameters.AmountType).Select(ctgb => $"coalesce(wrv.{ctgb}, 'is_null_value') = coalesce(maxver.max_{ctgb}, 'is_null_value')"))}
                    """;

        // The order is important for combining the rows into packages, since the sql rows are streamed and
        // packages are created on-the-fly each time a new row is received.
        sql += $"""
                {"\n"}ORDER BY {string.Join(", ", _helper.GetColumnsToAggregateBy(_queryParameters.AmountType))}, {_helper.GetTimeColumnName(_queryParameters.AmountType)}
                """;

        return sql;
    }

    private string GenerateLatestOrFixedCalculationTypeConstraint(string prefix)
    {
        if (_queryParameters.CalculationType is not null)
        {
            return $"""
                    {prefix}.{_helper.GetCalculationTypeColumnName(_queryParameters.AmountType)} = '{CalculationTypeMapper.ToDeltaTableValue(_queryParameters.CalculationType.Value)}'
                    """;
        }

        if (_calculationTypePerGridAreas.IsNullOrEmpty())
        {
            return """
                   FALSE
                   """;
        }

        var calculationTypePerGridAreaConstraints = _calculationTypePerGridAreas
            .Select(ctpga => $"""
                              ({prefix}.{_helper.GetGridAreaCodeColumnName(_queryParameters.AmountType)} = '{ctpga.GridArea}' AND {prefix}.{_helper.GetCalculationTypeColumnName(_queryParameters.AmountType)} = '{ctpga.CalculationType}')
                              """);

        return $"""
                ({string.Join(" OR ", calculationTypePerGridAreaConstraints)})
                """;
    }

    public enum StatementType
    {
        Select,
        Exists,
    }
}
