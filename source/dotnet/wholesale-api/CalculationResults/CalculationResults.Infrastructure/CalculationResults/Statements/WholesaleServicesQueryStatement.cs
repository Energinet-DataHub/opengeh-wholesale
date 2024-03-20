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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public class WholesaleServicesQueryStatement : DatabricksStatement
{
    private readonly StatementType _statementType;
    private readonly WholesaleServicesQueryParameters _queryParameters;
    private readonly DeltaTableOptions _deltaTableOptions;

    public WholesaleServicesQueryStatement(StatementType statementType, WholesaleServicesQueryParameters queryParameters, DeltaTableOptions deltaTableOptions)
    {
        _statementType = statementType;
        _queryParameters = queryParameters;
        _deltaTableOptions = deltaTableOptions;
    }

    protected override string GetSqlStatement()
    {
        var selectTarget = _statementType switch
        {
            StatementType.Select => $"{string.Join(", ", ColumnsToSelect)}",
            StatementType.Exists => "1",
            _ => throw new ArgumentOutOfRangeException(nameof(_statementType), _statementType, "Unknown StatementType"),
        };

        var sql = $@"
            SELECT {selectTarget}
            FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.WHOLESALE_RESULTS_TABLE_NAME}
            WHERE {WholesaleResultColumnNames.AmountType} = '{AmountTypeMapper.ToDeltaTableValue(_queryParameters.AmountType)}'
        ";

        var calculationPeriodSql = _queryParameters.Calculations
            .Select(calculationForPeriod => $@"
                ({WholesaleResultColumnNames.CalculationId} == '{calculationForPeriod.CalculationId}'  
                AND {WholesaleResultColumnNames.Time} >= '{calculationForPeriod.Period.Start}'
                AND {WholesaleResultColumnNames.Time} < '{calculationForPeriod.Period.End}')")
            .ToList();

        sql += $" AND ({string.Join(" OR ", calculationPeriodSql)})";

        if (!string.IsNullOrEmpty(_queryParameters.GridArea))
            sql += $" AND {WholesaleResultColumnNames.GridArea} = '{_queryParameters.GridArea}'";

        if (!string.IsNullOrEmpty(_queryParameters.EnergySupplierId))
            sql += $" AND {WholesaleResultColumnNames.EnergySupplierId} = '{_queryParameters.EnergySupplierId}'";

        if (!string.IsNullOrEmpty(_queryParameters.ChargeOwnerId))
            sql += $" AND {WholesaleResultColumnNames.ChargeOwnerId} = '{_queryParameters.ChargeOwnerId}'";

        if (_queryParameters.ChargeTypes.Any())
        {
            var chargeTypesSql = _queryParameters.ChargeTypes
                .Select(c => CreateChargeTypeSqlStatement(c.ChargeCode, c.ChargeType))
                .ToList();

            sql += $" AND ({string.Join(" OR ", chargeTypesSql)})";
        }

        // The order is important for combining the rows into packages, since the sql rows are streamed and
        //      packages are created on-the-fly each time a new row is received.
        sql += $@"
                ORDER BY 
                    {string.Join(", ", ColumnsToGroupBy)},
                    {WholesaleResultColumnNames.Time}";

        return sql;
    }

    private string CreateChargeTypeSqlStatement(string? chargeCode, ChargeType? chargeType)
    {
        if (chargeCode == null && chargeType == null)
            throw new ArgumentException("Both chargeCode and chargeType cannot be null");

        var sqlStatements = new List<string>();

        if (!string.IsNullOrEmpty(chargeCode))
            sqlStatements.Add($"{WholesaleResultColumnNames.ChargeCode} = '{chargeCode}'");

        if (chargeType != null)
            sqlStatements.Add($"{WholesaleResultColumnNames.ChargeType} = '{ChargeTypeMapper.ToDeltaTableValue(chargeType.Value)}'");

        var combinedString = string.Join(" AND ", sqlStatements);

        if (sqlStatements.Count > 1)
            combinedString = $"({combinedString})";

        return combinedString;
    }

    /// <summary>
    /// Since results are streamed and packages are created on-the-fly, the data need to be ordered so that
    ///     all rows belonging to one package are ordered directly after one another.
    /// </summary>
    public static string[] ColumnsToGroupBy =>
    [
        WholesaleResultColumnNames.GridArea,
        WholesaleResultColumnNames.EnergySupplierId,
        WholesaleResultColumnNames.ChargeOwnerId,
        WholesaleResultColumnNames.ChargeType,
        WholesaleResultColumnNames.ChargeCode,
        WholesaleResultColumnNames.AmountType,
        WholesaleResultColumnNames.MeteringPointType,
        WholesaleResultColumnNames.SettlementMethod,
        WholesaleResultColumnNames.CalculationId,
    ];

    private static string[] ColumnsToSelect { get; } =
    [
        WholesaleResultColumnNames.GridArea,
        WholesaleResultColumnNames.EnergySupplierId,
        WholesaleResultColumnNames.AmountType,
        WholesaleResultColumnNames.MeteringPointType,
        WholesaleResultColumnNames.SettlementMethod,
        WholesaleResultColumnNames.ChargeType,
        WholesaleResultColumnNames.ChargeCode,
        WholesaleResultColumnNames.ChargeOwnerId,
        WholesaleResultColumnNames.Resolution,
        WholesaleResultColumnNames.QuantityUnit,
        WholesaleResultColumnNames.Time,
        WholesaleResultColumnNames.Quantity,
        WholesaleResultColumnNames.QuantityQualities,
        WholesaleResultColumnNames.Price,
        WholesaleResultColumnNames.Amount,
        WholesaleResultColumnNames.CalculationId,
        WholesaleResultColumnNames.CalculationType,
    ];

    public enum StatementType
    {
        Select,
        Exists,
    }
}
