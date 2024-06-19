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
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;

public sealed class SettlementReportChargePriceQueryStatement : DatabricksStatement
{
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions;
    private readonly SettlementReportChargePriceQueryFilter _filter;
    private readonly int _skip;
    private readonly int _take;

    public SettlementReportChargePriceQueryStatement(
        IOptions<DeltaTableOptions> deltaTableOptions,
        SettlementReportChargePriceQueryFilter filter,
        int skip,
        int take)
    {
        _deltaTableOptions = deltaTableOptions;
        _filter = filter;
        _skip = skip;
        _take = take;
    }

    protected override string GetSqlStatement()
    {
        var chargePrice =
            $"""
                     SELECT DISTINCT({SettlementReportChargePriceViewColumns.StartTime}) 
                     FROM
                         {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.CHARGE_PRICES_V1_VIEW_NAME}
                     WHERE
                         {SettlementReportChargePriceViewColumns.GridArea} = '{SqlStringSanitizer.Sanitize(_filter.GridAreaCode)}' AND
                         {SettlementReportChargePriceViewColumns.StartTime} >= '{_filter.PeriodStart}' AND
                         {SettlementReportChargePriceViewColumns.CalculationId} = '{_filter.CalculationId}' AND
                         {SettlementReportChargePriceViewColumns.CalculationType} = '{CalculationTypeMapper.ToDeltaTableValue(_filter.CalculationType)}' AND
                         {(_filter.EnergySupplier is null ? string.Empty : SettlementReportChargePriceViewColumns.EnergySupplierId + " = '" + SqlStringSanitizer.Sanitize(_filter.EnergySupplier) + "'")}
                     ORDER BY 
                         {SettlementReportChargePriceViewColumns.StartTime} LIMIT {_take} OFFSET {_skip}
                 """.Replace(Environment.NewLine, " ");

        return $"""
                SELECT {string.Join(", ", [
                    SettlementReportChargePriceViewColumns.ChargeType,
                    SettlementReportChargePriceViewColumns.ChargeCode,
                    SettlementReportChargePriceViewColumns.ChargeOwnerId,
                    SettlementReportChargePriceViewColumns.Resolution,
                    SettlementReportChargePriceViewColumns.Taxation,
                    "cp." + SettlementReportChargePriceViewColumns.StartTime,
                    SettlementReportChargePriceViewColumns.EnergyPrices,
                ])}
                FROM 
                    {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.CHARGE_PRICES_V1_VIEW_NAME}
                JOIN 
                      ({chargePrice}) AS cp ON
                      {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.CHARGE_PRICES_V1_VIEW_NAME}.{SettlementReportChargePriceViewColumns.StartTime} = cp.{SettlementReportChargePriceViewColumns.StartTime}
                WHERE 
                        {SettlementReportChargePriceViewColumns.GridArea} = '{SqlStringSanitizer.Sanitize(_filter.GridAreaCode)}' AND
                        cp.{SettlementReportChargePriceViewColumns.StartTime} >= '{_filter.PeriodStart}' AND
                        {SettlementReportChargePriceViewColumns.CalculationId} = '{_filter.CalculationId}' AND
                        {SettlementReportChargePriceViewColumns.CalculationType} = '{CalculationTypeMapper.ToDeltaTableValue(_filter.CalculationType)}' AND
                        {(_filter.EnergySupplier is null ? string.Empty : SettlementReportChargePriceViewColumns.EnergySupplierId + " = '" + SqlStringSanitizer.Sanitize(_filter.EnergySupplier) + "'")}
             """;
    }
}
