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
    WholesaleServicesQueryStatementWhereClauseProvider whereClauseProvider,
    DeltaTableOptions deltaTableOptions)
    : DatabricksStatement
{
    private readonly StatementType _statementType = statementType;
    private readonly WholesaleServicesQueryParameters _queryParameters = queryParameters;
    private readonly IReadOnlyCollection<CalculationTypeForGridArea> _calculationTypePerGridAreas = calculationTypePerGridAreas;
    private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;
    private readonly WholesaleServicesQueryStatementWhereClauseProvider _whereClauseProvider = whereClauseProvider;

    protected override string GetSqlStatement()
    {
        var selectTarget = _statementType switch
        {
            StatementType.Select => GetProjection("wrv"),
            StatementType.Exists => "1",
            _ => throw new ArgumentOutOfRangeException(nameof(_statementType), _statementType, "Unknown StatementType"),
        };

        var sql = $"""
                    SELECT {selectTarget}
                    FROM (SELECT {GetProjection("wr")}
                    FROM {GetSource()} wr
                    WHERE {GenerateLatestOrFixedCalculationTypeConstraint("wr")}) wrv
                    INNER JOIN (SELECT max({AmountsPerChargeViewColumnNames.CalculationVersion}) AS max_version, {WholesaleResultColumnNames.Time} AS max_time, {string.Join(", ", GetColumnsToGroupBy().Select(ctgb => $"{ctgb} AS max_{ctgb}"))}
                    FROM {GetSource()} wr
                    {_whereClauseProvider.GetWhereClauseSqlExpression(_queryParameters, "wr")} AND {GenerateLatestOrFixedCalculationTypeConstraint("wr")}
                    GROUP BY {WholesaleResultColumnNames.Time}, {string.Join(", ", GetColumnsToGroupBy())}) maxver
                    ON wrv.{WholesaleResultColumnNames.Time} = maxver.max_time AND wrv.{AmountsPerChargeViewColumnNames.CalculationVersion} = maxver.max_version AND {string.Join(" AND ", GetColumnsToGroupBy().Select(ctgb => $"coalesce(wrv.{ctgb}, 'is_null_value') = coalesce(maxver.max_{ctgb}, 'is_null_value')"))}
                    """;

        // The order is important for combining the rows into packages, since the sql rows are streamed and
        // packages are created on-the-fly each time a new row is received.
        sql += $"""
                {"\n"}ORDER BY {string.Join(", ", GetColumnsToGroupBy())}, {WholesaleResultColumnNames.Time}
                """;

        return sql;
    }

    private string GetProjection(string prefix)
    {
        return $"{string.Join(", ", GetColumnsToProject().Select(cts => $"`{prefix}`.`{cts}`"))}";
    }

    private string GenerateLatestOrFixedCalculationTypeConstraint(string prefix)
    {
        if (_queryParameters.CalculationType is not null)
        {
            return $"""
                    {prefix}.{GetCalculationTypeColumnName()} = '{CalculationTypeMapper.ToDeltaTableValue(_queryParameters.CalculationType.Value)}'
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
                              ({prefix}.{GetGridAreaCodeColumnName()} = '{ctpga.GridArea}' AND {prefix}.{GetCalculationTypeColumnName()} = '{ctpga.CalculationType}')
                              """);

        return $"""
                ({string.Join(" OR ", calculationTypePerGridAreaConstraints)})
                """;
    }

    private string GetCalculationTypeColumnName()
    {
        return _queryParameters.AmountType switch
        {
            AmountType.AmountPerCharge => AmountsPerChargeViewColumnNames.CalculationType,
            AmountType.MonthlyAmountPerCharge => MonthlyAmountsPerChargeViewColumnNames.CalculationType,
            AmountType.TotalMonthlyAmount => TotalMonthlyAmountsViewColumnNames.CalculationType,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private string GetGridAreaCodeColumnName()
    {
        return _queryParameters.AmountType switch
        {
            AmountType.AmountPerCharge => AmountsPerChargeViewColumnNames.GridAreaCode,
            AmountType.MonthlyAmountPerCharge => MonthlyAmountsPerChargeViewColumnNames.GridAreaCode,
            AmountType.TotalMonthlyAmount => TotalMonthlyAmountsViewColumnNames.GridAreaCode,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private string GetSource()
    {
        return _queryParameters.AmountType switch
        {
            AmountType.AmountPerCharge => $"{_deltaTableOptions.WholesaleCalculationResultsSchemaName}.{_deltaTableOptions.AMOUNTS_PER_CHARGE_V1_VIEW_NAME}",
            AmountType.MonthlyAmountPerCharge => $"{_deltaTableOptions.WholesaleCalculationResultsSchemaName}.{_deltaTableOptions.MONTHLY_AMOUNTS_V1_VIEW_NAME}",
            AmountType.TotalMonthlyAmount => $"{_deltaTableOptions.WholesaleCalculationResultsSchemaName}.{_deltaTableOptions.TOTAL_MONTHLY_AMOUNTS_V1_VIEW_NAME}",
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private string[] GetColumnsToProject()
    {
        return _queryParameters.AmountType switch
        {
            AmountType.AmountPerCharge => ColumnsToProjectForAmountsPerCharge,
            AmountType.MonthlyAmountPerCharge => ColumnsToProjectForMonthlyAmountsPerCharge,
            AmountType.TotalMonthlyAmount => ColumnsToProjectForTotalMonthlyAmounts,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private string[] GetColumnsToGroupBy()
    {
        return _queryParameters.AmountType switch
        {
            AmountType.AmountPerCharge => ColumnsToGroupByForAmountsPerCharge,
            AmountType.MonthlyAmountPerCharge => ColumnsToGroupByForMonthlyAmountsPerCharge,
            AmountType.TotalMonthlyAmount => ColumnsToGroupByForTotalMonthlyAmounts,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static string[] ColumnsToGroupByForAmountsPerCharge =>
    [
        AmountsPerChargeViewColumnNames.GridAreaCode,
        AmountsPerChargeViewColumnNames.EnergySupplierId,
        AmountsPerChargeViewColumnNames.ChargeOwnerId,
        AmountsPerChargeViewColumnNames.ChargeType,
        AmountsPerChargeViewColumnNames.ChargeCode,
        AmountsPerChargeViewColumnNames.Resolution,
        AmountsPerChargeViewColumnNames.MeteringPointType,
        AmountsPerChargeViewColumnNames.SettlementMethod,
    ];

    public static string[] ColumnsToGroupByForMonthlyAmountsPerCharge =>
    [
        MonthlyAmountsPerChargeViewColumnNames.GridAreaCode,
        MonthlyAmountsPerChargeViewColumnNames.EnergySupplierId,
        MonthlyAmountsPerChargeViewColumnNames.ChargeOwnerId,
        MonthlyAmountsPerChargeViewColumnNames.ChargeType,
        MonthlyAmountsPerChargeViewColumnNames.ChargeCode,
    ];

    public static string[] ColumnsToGroupByForTotalMonthlyAmounts =>
    [
        TotalMonthlyAmountsViewColumnNames.GridAreaCode,
        TotalMonthlyAmountsViewColumnNames.EnergySupplierId,
        TotalMonthlyAmountsViewColumnNames.ChargeOwnerId,
    ];

    private static string[] ColumnsToProjectForAmountsPerCharge =>
    [
        AmountsPerChargeViewColumnNames.CalculationId,
        AmountsPerChargeViewColumnNames.CalculationType,
        AmountsPerChargeViewColumnNames.CalculationVersion,
        AmountsPerChargeViewColumnNames.CalculationResultId,
        AmountsPerChargeViewColumnNames.GridAreaCode,
        AmountsPerChargeViewColumnNames.EnergySupplierId,
        AmountsPerChargeViewColumnNames.ChargeCode,
        AmountsPerChargeViewColumnNames.ChargeType,
        AmountsPerChargeViewColumnNames.ChargeOwnerId,
        AmountsPerChargeViewColumnNames.Resolution,
        AmountsPerChargeViewColumnNames.QuantityUnit,
        AmountsPerChargeViewColumnNames.MeteringPointType,
        AmountsPerChargeViewColumnNames.SettlementMethod,
        AmountsPerChargeViewColumnNames.IsTax,
        AmountsPerChargeViewColumnNames.Currency,
        AmountsPerChargeViewColumnNames.Time,
        AmountsPerChargeViewColumnNames.Quantity,
        AmountsPerChargeViewColumnNames.QuantityQualities,
        AmountsPerChargeViewColumnNames.Price,
        AmountsPerChargeViewColumnNames.Amount,
    ];

    private static string[] ColumnsToProjectForMonthlyAmountsPerCharge =>
    [
        MonthlyAmountsPerChargeViewColumnNames.CalculationId,
        MonthlyAmountsPerChargeViewColumnNames.CalculationType,
        MonthlyAmountsPerChargeViewColumnNames.CalculationVersion,
        MonthlyAmountsPerChargeViewColumnNames.CalculationResultId,
        MonthlyAmountsPerChargeViewColumnNames.GridAreaCode,
        MonthlyAmountsPerChargeViewColumnNames.EnergySupplierId,
        MonthlyAmountsPerChargeViewColumnNames.ChargeCode,
        MonthlyAmountsPerChargeViewColumnNames.ChargeType,
        MonthlyAmountsPerChargeViewColumnNames.ChargeOwnerId,
        MonthlyAmountsPerChargeViewColumnNames.QuantityUnit,
        MonthlyAmountsPerChargeViewColumnNames.IsTax,
        MonthlyAmountsPerChargeViewColumnNames.Currency,
        MonthlyAmountsPerChargeViewColumnNames.Time,
        MonthlyAmountsPerChargeViewColumnNames.Amount,
    ];

    private static string[] ColumnsToProjectForTotalMonthlyAmounts =>
    [
        TotalMonthlyAmountsViewColumnNames.CalculationId,
        TotalMonthlyAmountsViewColumnNames.CalculationType,
        TotalMonthlyAmountsViewColumnNames.CalculationVersion,
        TotalMonthlyAmountsViewColumnNames.CalculationResultId,
        TotalMonthlyAmountsViewColumnNames.GridAreaCode,
        TotalMonthlyAmountsViewColumnNames.EnergySupplierId,
        TotalMonthlyAmountsViewColumnNames.ChargeOwnerId,
        TotalMonthlyAmountsViewColumnNames.Currency,
        TotalMonthlyAmountsViewColumnNames.Time,
        TotalMonthlyAmountsViewColumnNames.Amount,
    ];

    public enum StatementType
    {
        Select,
        Exists,
    }
}
