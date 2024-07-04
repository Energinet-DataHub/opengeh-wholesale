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
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;

public sealed class SettlementReportLatestMeteringPointMasterDataQueryStatement : DatabricksStatement
{
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions;
    private readonly SettlementReportMeteringPointMasterDataQueryFilter _filter;
    private readonly int _skip;
    private readonly int _take;

    public SettlementReportLatestMeteringPointMasterDataQueryStatement(IOptions<DeltaTableOptions> deltaTableOptions, SettlementReportMeteringPointMasterDataQueryFilter filter, int skip, int take)
    {
        _deltaTableOptions = deltaTableOptions;
        _filter = filter;
        _skip = skip;
        _take = take;
    }

    protected override string GetSqlStatement()
    {
        var calcQuery = $"""
            SELECT DISTINCT(enResult.{SettlementReportEnergyResultViewColumns.CalculationId})
            FROM
                {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME} AS enResult
            JOIN (
                SELECT
                    DATE_TRUNC(
                        'day',
                        FROM_UTC_TIMESTAMP({SettlementReportEnergyResultViewColumns.Time}, 'Europe/Copenhagen')
                    ) as day,
                    MAX({SettlementReportEnergyResultViewColumns.CalculationVersion}) AS MaxCalcVersion
                FROM
                    {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME}
                WHERE
                    {SettlementReportEnergyResultViewColumns.Time} >= '{_filter.PeriodStart}' AND
                    {SettlementReportEnergyResultViewColumns.Time} < '{_filter.PeriodEnd}'
                GROUP BY day ) AS prDay ON enResult.{SettlementReportEnergyResultViewColumns.CalculationVersion} = prDay.MaxCalcVersion AND DATE_TRUNC(
                        'day',
                        FROM_UTC_TIMESTAMP({SettlementReportEnergyResultViewColumns.Time}, 'Europe/Copenhagen')
                    ) = prDay.Day
                WHERE
                    {SettlementReportEnergyResultViewColumns.GridArea} = '{SqlStringSanitizer.Sanitize(_filter.GridAreaCode)}' AND
                    {SettlementReportEnergyResultViewColumns.Time} >= '{_filter.PeriodStart}' AND
                    {SettlementReportEnergyResultViewColumns.Time} < '{_filter.PeriodEnd}' AND
                    {SettlementReportEnergyResultViewColumns.CalculationType} = '{CalculationTypeMapper.ToDeltaTableValue(CalculationType.BalanceFixing)}'
            """;

        return $"""
            SELECT {string.Join(", ", [
                SettlementReportMeteringPointMasterDataViewColumns.MeteringPointId,
                SettlementReportMeteringPointMasterDataViewColumns.MeteringPointType,
                SettlementReportMeteringPointMasterDataViewColumns.SettlementMethod,
                SettlementReportMeteringPointMasterDataViewColumns.FromDate,
                SettlementReportMeteringPointMasterDataViewColumns.ToDate,
                SettlementReportMeteringPointMasterDataViewColumns.GridArea,
                SettlementReportMeteringPointMasterDataViewColumns.GridAreaFrom,
                SettlementReportMeteringPointMasterDataViewColumns.GridAreaTo,
                SettlementReportMeteringPointMasterDataViewColumns.EnergySupplierId,
            ])}
            FROM 
                {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME}
        JOIN
            ({calcQuery}) as calc ON calc.{SettlementReportEnergyResultViewColumns.CalculationId} = {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME}.{SettlementReportMeteringPointMasterDataViewColumns.CalculationId}
            WHERE 
                    {SettlementReportMeteringPointMasterDataViewColumns.GridArea} = '{SqlStringSanitizer.Sanitize(_filter.GridAreaCode)}' AND
                    {SettlementReportMeteringPointMasterDataViewColumns.CalculationType} = '{CalculationTypeMapper.ToDeltaTableValue(_filter.CalculationType)}' AND
                    {SettlementReportMeteringPointMasterDataViewColumns.FromDate} >= '{_filter.PeriodStart}' AND
                    {SettlementReportMeteringPointMasterDataViewColumns.ToDate} < '{_filter.PeriodEnd}'
                    {(_filter.EnergySupplier is null ? string.Empty : " And " + SettlementReportMeteringPointMasterDataViewColumns.EnergySupplierId + " = '" + SqlStringSanitizer.Sanitize(_filter.EnergySupplier) + "' AND")}
        """;
    }
}
