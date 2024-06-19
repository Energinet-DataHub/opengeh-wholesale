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

public sealed class SettlementReportMeteringPointTimeSeriesResultCountQueryStatement : DatabricksStatement
{
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions;
    private readonly SettlementReportMeteringPointTimeSeriesResultQueryFilter _filter;

    public SettlementReportMeteringPointTimeSeriesResultCountQueryStatement(IOptions<DeltaTableOptions> deltaTableOptions, SettlementReportMeteringPointTimeSeriesResultQueryFilter filter)
    {
        _deltaTableOptions = deltaTableOptions;
        _filter = filter;
    }

    protected override string GetSqlStatement()
    {
        return $"""
                    SELECT COUNT(DISTINCT({SettlementReportMeteringPointMasterDataViewColumns.MeteringPointId})) AS count 
                    FROM
                        {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.ENERGY_RESULTS_METERING_POINT_TIME_SERIES_V1_VIEW_NAME}
                    WHERE
                        {SettlementReportMeteringPointTimeSeriesViewColumns.GridArea} = '{SqlStringSanitizer.Sanitize(_filter.GridAreaCode)}' AND
                        {SettlementReportMeteringPointTimeSeriesViewColumns.StartDateTime} >= '{_filter.PeriodStart}' AND
                        {SettlementReportMeteringPointTimeSeriesViewColumns.StartDateTime} < '{_filter.PeriodEnd}' AND
                        {SettlementReportMeteringPointTimeSeriesViewColumns.CalculationId} = '{_filter.CalculationId}' AND
                        {SettlementReportMeteringPointTimeSeriesViewColumns.Resolution} = '{SqlStringSanitizer.Sanitize(_filter.Resolution)}'
                        {(_filter.EnergySupplier is not null ? $"AND {SettlementReportMeteringPointTimeSeriesViewColumns.EnergySupplier} = '{SqlStringSanitizer.Sanitize(_filter.EnergySupplier)}'" : string.Empty)}
                """;
    }
}
