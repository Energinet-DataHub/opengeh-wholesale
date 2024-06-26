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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;

public sealed class SettlementReportMonthlyAmountTotalQueryStatement : DatabricksStatement
{
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions;
    private readonly SettlementReportMonthlyAmountQueryFilter _filter;
    private readonly int _skip;
    private readonly int _take;

    public SettlementReportMonthlyAmountTotalQueryStatement(IOptions<DeltaTableOptions> deltaTableOptions, SettlementReportMonthlyAmountQueryFilter filter, int skip, int take)
    {
        _deltaTableOptions = deltaTableOptions;
        _filter = filter;
        _skip = skip;
        _take = take;
    }

    protected override string GetSqlStatement()
    {
        var monthlyAmount =
            $"""
            SELECT DISTINCT({SettlementReportMonthlyAmountViewColumns.ResultId})
            FROM
                {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.MONTHLY_AMOUNTS_V1_VIEW_NAME}
            WHERE 
                {SettlementReportMonthlyAmountViewColumns.GridAreaCode} = '{SqlStringSanitizer.Sanitize(_filter.GridAreaCode)}' AND
                {SettlementReportMonthlyAmountViewColumns.CalculationType} = '{CalculationTypeMapper.ToDeltaTableValue(_filter.CalculationType)}' AND
                {SettlementReportMonthlyAmountViewColumns.Time} >= '{_filter.PeriodStart}' AND
                {SettlementReportMonthlyAmountViewColumns.Time} < '{_filter.PeriodEnd}' AND
                {SettlementReportMonthlyAmountViewColumns.CalculationId} = '{_filter.CalculationId}' AND
                {SettlementReportMonthlyAmountViewColumns.ChargeType} IS NULL AND 
                {SettlementReportMonthlyAmountViewColumns.ChargeCode} IS NULL
                {(_filter is { MarketRole: MarketRole.SystemOperator or MarketRole.GridAccessProvider, ChargeOwnerId: not null } ? " AND "
                    + SettlementReportMonthlyAmountViewColumns.ChargeOwnerId + " = '" + SqlStringSanitizer.Sanitize(_filter.ChargeOwnerId) + "'" : string.Empty)} 
                {(_filter.EnergySupplier is null ? string.Empty : " AND "
                                                                  + SettlementReportMonthlyAmountViewColumns.EnergySupplierId + " = '" + SqlStringSanitizer.Sanitize(_filter.EnergySupplier) + "'"
                                                                  + " AND " + SettlementReportMonthlyAmountViewColumns.ChargeOwnerId + " IS NULL")}
            ORDER BY 
                {SettlementReportMonthlyAmountViewColumns.ResultId} LIMIT {_take} OFFSET {_skip}
        """.Replace(Environment.NewLine, " ");

        return $"""
                SELECT {string.Join(", ", [
                    "ma." + SettlementReportMonthlyAmountViewColumns.ResultId,
                    SettlementReportMonthlyAmountViewColumns.QuantityUnit,
                    SettlementReportMonthlyAmountViewColumns.Amount,
                    SettlementReportMonthlyAmountViewColumns.CalculationId,
                    SettlementReportMonthlyAmountViewColumns.ChargeCode,
                    SettlementReportMonthlyAmountViewColumns.ChargeType,
                    SettlementReportMonthlyAmountViewColumns.Currency,
                    SettlementReportMonthlyAmountViewColumns.Resolution,
                    SettlementReportMonthlyAmountViewColumns.Time,
                    SettlementReportMonthlyAmountViewColumns.CalculationType,
                    SettlementReportMonthlyAmountViewColumns.ChargeOwnerId,
                    SettlementReportMonthlyAmountViewColumns.EnergySupplierId,
                    SettlementReportMonthlyAmountViewColumns.GridAreaCode
                ])}
                FROM 
                    {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.MONTHLY_AMOUNTS_V1_VIEW_NAME}
                JOIN 
                    ({monthlyAmount}) AS ma ON {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.MONTHLY_AMOUNTS_V1_VIEW_NAME}.{SettlementReportMonthlyAmountViewColumns.ResultId} = ma.{SettlementReportMonthlyAmountViewColumns.ResultId}
                WHERE 
                        {SettlementReportMonthlyAmountViewColumns.GridAreaCode} = '{SqlStringSanitizer.Sanitize(_filter.GridAreaCode)}' AND
                        {SettlementReportMonthlyAmountViewColumns.CalculationType} = '{CalculationTypeMapper.ToDeltaTableValue(_filter.CalculationType)}' AND
                        {SettlementReportMonthlyAmountViewColumns.Time} >= '{_filter.PeriodStart}' AND
                        {SettlementReportMonthlyAmountViewColumns.Time} < '{_filter.PeriodEnd}' AND
                        {SettlementReportMonthlyAmountViewColumns.CalculationId} = '{_filter.CalculationId}' AND
                        {SettlementReportMonthlyAmountViewColumns.ChargeType} IS NULL AND 
                        {SettlementReportMonthlyAmountViewColumns.ChargeCode} IS NULL
                        {(_filter is { MarketRole: MarketRole.SystemOperator or MarketRole.GridAccessProvider, ChargeOwnerId: not null } ? " AND "
                            + SettlementReportMonthlyAmountViewColumns.ChargeOwnerId + " = '" + SqlStringSanitizer.Sanitize(_filter.ChargeOwnerId) + "'" : string.Empty)}
                        {(_filter.EnergySupplier is null ? string.Empty : " AND "
                                                                          + SettlementReportMonthlyAmountViewColumns.EnergySupplierId + " = '" + SqlStringSanitizer.Sanitize(_filter.EnergySupplier) + "'"
                                                                          + " AND " + SettlementReportMonthlyAmountViewColumns.ChargeOwnerId + " IS NULL")}
                """;
    }
}
