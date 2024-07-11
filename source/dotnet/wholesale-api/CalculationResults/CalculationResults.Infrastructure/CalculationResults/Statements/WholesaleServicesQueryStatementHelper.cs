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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public class WholesaleServicesQueryStatementHelper
{
    public string GetSource(AmountType amountType, DeltaTableOptions tableOptions)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => $"{tableOptions.WholesaleCalculationResultsSchemaName}.{tableOptions.AMOUNTS_PER_CHARGE_V1_VIEW_NAME}",
            AmountType.MonthlyAmountPerCharge => $"{tableOptions.WholesaleCalculationResultsSchemaName}.{tableOptions.MONTHLY_AMOUNTS_PER_CHARGE_V1_VIEW_NAME}",
            AmountType.TotalMonthlyAmount => $"{tableOptions.WholesaleCalculationResultsSchemaName}.{tableOptions.TOTAL_MONTHLY_AMOUNTS_V1_VIEW_NAME}",
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    internal AmountType GuessAmountTypeFromRow(DatabricksSqlRow databricksSqlRow)
    {
        return databricksSqlRow.HasColumn(AmountsPerChargeViewColumnNames.Resolution)
            ? AmountType.AmountPerCharge
            : databricksSqlRow.HasColumn(MonthlyAmountsPerChargeViewColumnNames.ChargeCode)
                ? AmountType.MonthlyAmountPerCharge
                : AmountType.TotalMonthlyAmount;
    }

    internal string GetProjection(string prefix, AmountType amountType)
    {
        return
            $"{string.Join(", ", GetColumnsToProject(amountType).Select<string, string>(cts => $"`{prefix}`.`{cts}`"))}";
    }

    internal string GetCalculationTypeColumnName(AmountType amountType)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => AmountsPerChargeViewColumnNames.CalculationType,
            AmountType.MonthlyAmountPerCharge => MonthlyAmountsPerChargeViewColumnNames.CalculationType,
            AmountType.TotalMonthlyAmount => TotalMonthlyAmountsViewColumnNames.CalculationType,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    internal string GetGridAreaCodeColumnName(AmountType amountType)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => AmountsPerChargeViewColumnNames.GridAreaCode,
            AmountType.MonthlyAmountPerCharge => MonthlyAmountsPerChargeViewColumnNames.GridAreaCode,
            AmountType.TotalMonthlyAmount => TotalMonthlyAmountsViewColumnNames.GridAreaCode,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    internal string GetTimeColumnName(AmountType amountType)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => AmountsPerChargeViewColumnNames.Time,
            AmountType.MonthlyAmountPerCharge => MonthlyAmountsPerChargeViewColumnNames.Time,
            AmountType.TotalMonthlyAmount => TotalMonthlyAmountsViewColumnNames.Time,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    internal string GetEnergySupplierIdColumnName(AmountType amountType)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => AmountsPerChargeViewColumnNames.EnergySupplierId,
            AmountType.MonthlyAmountPerCharge => MonthlyAmountsPerChargeViewColumnNames.EnergySupplierId,
            AmountType.TotalMonthlyAmount => TotalMonthlyAmountsViewColumnNames.EnergySupplierId,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    internal string GetChargeOwnerIdColumnName(AmountType amountType)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => AmountsPerChargeViewColumnNames.ChargeOwnerId,
            AmountType.MonthlyAmountPerCharge => MonthlyAmountsPerChargeViewColumnNames.ChargeOwnerId,
            AmountType.TotalMonthlyAmount => TotalMonthlyAmountsViewColumnNames.ChargeOwnerId,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    internal string GetChargeCodeColumnName(AmountType amountType)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => AmountsPerChargeViewColumnNames.ChargeCode,
            AmountType.MonthlyAmountPerCharge => MonthlyAmountsPerChargeViewColumnNames.ChargeCode,
            AmountType.TotalMonthlyAmount => throw new InvalidOperationException("Och nay, no charge code for total monthly amounts"),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    internal string GetChargeTypeColumnName(AmountType amountType)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => AmountsPerChargeViewColumnNames.ChargeType,
            AmountType.MonthlyAmountPerCharge => MonthlyAmountsPerChargeViewColumnNames.ChargeType,
            AmountType.TotalMonthlyAmount => throw new InvalidOperationException("Och nay, no charge type for total monthly amounts"),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    internal string GetCalculationVersionColumnName(AmountType amountType)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => AmountsPerChargeViewColumnNames.CalculationVersion,
            AmountType.MonthlyAmountPerCharge => MonthlyAmountsPerChargeViewColumnNames.CalculationVersion,
            AmountType.TotalMonthlyAmount => TotalMonthlyAmountsViewColumnNames.CalculationVersion,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    internal string GetCalculationIdColumnName(AmountType amountType)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => AmountsPerChargeViewColumnNames.CalculationId,
            AmountType.MonthlyAmountPerCharge => MonthlyAmountsPerChargeViewColumnNames.CalculationId,
            AmountType.TotalMonthlyAmount => TotalMonthlyAmountsViewColumnNames.CalculationId,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    internal string[] GetColumnsToProject(AmountType amountType)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => ColumnsToProjectForAmountsPerCharge,
            AmountType.MonthlyAmountPerCharge => ColumnsToProjectForMonthlyAmountsPerCharge,
            AmountType.TotalMonthlyAmount => ColumnsToProjectForTotalMonthlyAmounts,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    internal string[] GetColumnsToAggregateBy(AmountType amountType)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => ColumnsToAggregateByForAmountsPerCharge,
            AmountType.MonthlyAmountPerCharge => ColumnsToAggregateByForMonthlyAmountsPerCharge,
            AmountType.TotalMonthlyAmount => ColumnsToAggregateByForTotalMonthlyAmounts,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    internal string GetWhereClauseSqlExpression(WholesaleServicesQueryParameters queryParameters, string table = "wrv")
    {
        var sql = $"""
                   {"\n"}
                   WHERE ({table}.{GetTimeColumnName(queryParameters.AmountType)} >= '{queryParameters.Period.Start}'
                          AND {table}.{GetTimeColumnName(queryParameters.AmountType)} < '{queryParameters.Period.End}')
                   """;

        if (queryParameters.GridAreaCodes.Count != 0)
        {
            sql += $"""
                    AND {table}.{GetGridAreaCodeColumnName(queryParameters.AmountType)} in ({string.Join(',', queryParameters.GridAreaCodes.Select(gridAreaCode => $"'{gridAreaCode}'"))})
                    """;
        }

        sql = queryParameters.AmountType != AmountType.TotalMonthlyAmount
            ? GenerateConstraintForActorsForNonTotalAmounts(queryParameters, table, sql)
            : GenerateConstraintForActorsForTotalAmounts(queryParameters, table, sql);

        if (queryParameters.ChargeTypes.Count != 0)
        {
            var chargeTypesSql = queryParameters.ChargeTypes
                .Select<(string? ChargeCode, ChargeType? ChargeType), string>(c =>
                    GenerateConstraintForChargeType(c.ChargeCode, c.ChargeType, table, queryParameters.AmountType))
                .ToList();

            sql += $"""
                    AND ({string.Join(" OR ", chargeTypesSql)})
                    """;
        }

        return sql;
    }

    internal string AddWhereClauseToSqlExpression(string sql, WholesaleServicesQueryParameters queryParameters)
    {
        sql += GetWhereClauseSqlExpression(queryParameters);

        return sql;
    }

    private string GenerateConstraintForActorsForTotalAmounts(
        WholesaleServicesQueryParameters queryParameters,
        string table,
        string sql)
    {
        if (queryParameters.EnergySupplierId is not null)
        {
            sql += $"""
                    AND {table}.{GetEnergySupplierIdColumnName(queryParameters.AmountType)} = '{queryParameters.EnergySupplierId}'
                    """;
        }

        if (queryParameters.ChargeOwnerId is not null)
        {
            sql += $"""
                    AND {table}.{GetChargeOwnerIdColumnName(queryParameters.AmountType)} = '{queryParameters.ChargeOwnerId}'
                    """;
        }
        else
        {
            sql += $"""
                    AND {table}.{GetChargeOwnerIdColumnName(queryParameters.AmountType)} is null
                    """;
        }

        return sql;
    }

    private string GenerateConstraintForActorsForNonTotalAmounts(
        WholesaleServicesQueryParameters queryParameters,
        string table,
        string sql)
    {
        if (queryParameters.EnergySupplierId is not null)
        {
            sql += $"""
                    AND {table}.{GetEnergySupplierIdColumnName(queryParameters.AmountType)} = '{queryParameters.EnergySupplierId}'
                    """;
        }

        if (queryParameters.ChargeOwnerId is not null)
        {
            sql += $"""
                    AND {table}.{GetChargeOwnerIdColumnName(queryParameters.AmountType)} = '{queryParameters.ChargeOwnerId}'
                    """;
        }

        return sql;
    }

    private string GenerateConstraintForChargeType(
        string? chargeCode,
        ChargeType? chargeType,
        string table,
        AmountType amountType)
    {
        if (chargeCode == null && chargeType == null)
            throw new ArgumentException("Both chargeCode and chargeType cannot be null");

        var sqlStatements = new List<string>();

        if (!string.IsNullOrEmpty(chargeCode))
            sqlStatements.Add($"{table}.{GetChargeCodeColumnName(amountType)} = '{chargeCode}'");

        if (chargeType != null)
            sqlStatements.Add($"{table}.{GetChargeTypeColumnName(amountType)} = '{ChargeTypeMapper.ToDeltaTableValue(chargeType.Value)}'");

        var combinedString = string.Join(" AND ", sqlStatements);

        if (sqlStatements.Count > 1)
            combinedString = $"({combinedString})";

        return combinedString;
    }

    private static string[] ColumnsToAggregateByForAmountsPerCharge =>
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

    private static string[] ColumnsToAggregateByForMonthlyAmountsPerCharge =>
    [
        MonthlyAmountsPerChargeViewColumnNames.GridAreaCode,
        MonthlyAmountsPerChargeViewColumnNames.EnergySupplierId,
        MonthlyAmountsPerChargeViewColumnNames.ChargeOwnerId,
        MonthlyAmountsPerChargeViewColumnNames.ChargeType,
        MonthlyAmountsPerChargeViewColumnNames.ChargeCode,
    ];

    private static string[] ColumnsToAggregateByForTotalMonthlyAmounts =>
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
}
