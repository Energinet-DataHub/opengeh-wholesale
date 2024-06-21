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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public class WholesaleServicesQueryStatementWhereClauseProvider
{
    internal string GetWhereClauseToSqlExpression(WholesaleServicesQueryParameters queryParameters, string table = "wrv")
    {
        var sql = $"""
                {"\n"}WHERE {table}.{WholesaleResultColumnNames.AmountType} = '{AmountTypeMapper.ToDeltaTableValue(queryParameters.AmountType)}'
                AND ({table}.{WholesaleResultColumnNames.Time} >= '{queryParameters.Period.Start}'
                     AND {table}.{WholesaleResultColumnNames.Time} < '{queryParameters.Period.End}')
                """;

        if (queryParameters.GridAreaCodes.Count != 0)
        {
            sql += $"""
                    AND {table}.{WholesaleResultColumnNames.GridArea} in ({string.Join(',', queryParameters.GridAreaCodes.Select(gridAreaCode => $"'{gridAreaCode}'"))})
                    """;
        }

        if (!string.IsNullOrEmpty(queryParameters.EnergySupplierId))
        {
            sql += $"""
                    AND {table}.{WholesaleResultColumnNames.EnergySupplierId} = '{queryParameters.EnergySupplierId}'
                    """;
        }

        if (!string.IsNullOrEmpty(queryParameters.ChargeOwnerId))
        {
            sql += $"""
                    AND {table}.{WholesaleResultColumnNames.ChargeOwnerId} = '{queryParameters.ChargeOwnerId}'
                    """;
        }

        if (queryParameters.ChargeTypes.Count != 0)
        {
            var chargeTypesSql = queryParameters.ChargeTypes
                .Select<(string? ChargeCode, ChargeType? ChargeType), string>(c =>
                    CreateChargeTypeSqlStatement(c.ChargeCode, c.ChargeType, table))
                .ToList();

            sql += $"""
                    AND ({string.Join(" OR ", chargeTypesSql)})
                    """;
        }

        return sql;
    }

    internal string AddWhereClauseToSqlExpression(string sql, WholesaleServicesQueryParameters queryParameters)
    {
        sql += GetWhereClauseToSqlExpression(queryParameters);

        return sql;
    }

    private string CreateChargeTypeSqlStatement(string? chargeCode, ChargeType? chargeType, string table)
    {
        if (chargeCode == null && chargeType == null)
            throw new ArgumentException("Both chargeCode and chargeType cannot be null");

        var sqlStatements = new List<string>();

        if (!string.IsNullOrEmpty(chargeCode))
            sqlStatements.Add($"{table}.{WholesaleResultColumnNames.ChargeCode} = '{chargeCode}'");

        if (chargeType != null)
            sqlStatements.Add($"{table}.{WholesaleResultColumnNames.ChargeType} = '{ChargeTypeMapper.ToDeltaTableValue(chargeType.Value)}'");

        var combinedString = string.Join(" AND ", sqlStatements);

        if (sqlStatements.Count > 1)
            combinedString = $"({combinedString})";

        return combinedString;
    }
}
