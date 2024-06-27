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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;

public sealed class SettlementReportMeteringPointTimeSeriesResultCountQueryStatement2 : DatabricksStatement
{
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions;
    private readonly SettlementReportMeteringPointTimeSeriesResultQueryFilter _filter;

    public SettlementReportMeteringPointTimeSeriesResultCountQueryStatement2(IOptions<DeltaTableOptions> deltaTableOptions, SettlementReportMeteringPointTimeSeriesResultQueryFilter filter)
    {
        _deltaTableOptions = deltaTableOptions;
        _filter = filter;
    }

    protected override string GetSqlStatement()
    {
        return $"""
                 SELECT COUNT(DISTINCT({SettlementReportMeteringPointTimeSeriesViewColumns.StartDateTime})) AS count
                 FROM (
                     SELECT
                         c.calculation_id,
                         FIRST(c.calculation_type) as calculation_type,
                         FIRST(c.version) as calculation_version,
                         m.metering_point_id,
                         m.metering_point_type,
                         m.resolution,
                         m.grid_area_code,
                         m.energy_supplier_id,
                         TO_UTC_TIMESTAMP(
                             DATE_TRUNC(
                             'day',
                             FROM_UTC_TIMESTAMP(t.observation_time, 'Europe/Copenhagen')
                             ),
                             'Europe/Copenhagen'
                         ) AS start_date_time,
                         ARRAY_SORT(
                             ARRAY_AGG(struct(t.observation_time, t.quantity))
                         ) AS quantities
                     FROM
                         basis_data.time_series_points t
                         INNER JOIN basis_data.calculations AS c ON c.calculation_id = t.calculation_id
                         INNER JOIN basis_data.metering_point_periods AS m ON m.metering_point_id = t.metering_point_id AND m.calculation_id = t.calculation_id
                     WHERE
                         c.calculation_type IN (
                           'balance_fixing',
                           'wholesale_fixing',
                           'first_correction_settlement',
                           'second_correction_settlement',
                           'third_correction_settlement'
                         )
                         AND t.calculation_id = '{_filter.CalculationId}'
                         AND t.observation_time >= '{_filter.PeriodStart}'
                         AND t.observation_time < '{_filter.PeriodEnd}'
                         AND m.grid_area_code = '{SqlStringSanitizer.Sanitize(_filter.GridAreaCode)}'
                         AND m.resolution = '{SqlStringSanitizer.Sanitize(_filter.Resolution)}'
                         {(_filter.EnergySupplier is not null ? $"AND m.energy_supplier_id = '{SqlStringSanitizer.Sanitize(_filter.EnergySupplier)}'" : string.Empty)}
                         AND t.observation_time >= m.from_date
                         AND (
                           m.to_date IS NULL
                           OR t.observation_time < m.to_date
                         )
                     GROUP BY
                       c.calculation_id,
                       m.metering_point_id,
                       m.metering_point_type,
                       DATE_TRUNC(
                         'day',
                         FROM_UTC_TIMESTAMP(t.observation_time, 'Europe/Copenhagen')
                       ),
                       m.resolution,
                       m.grid_area_code,
                       m.energy_supplier_id
                     )
                 """;
    }
}
