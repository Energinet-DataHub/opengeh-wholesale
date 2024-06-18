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

public sealed class SettlementReportMeteringPointMasterDataCountQueryStatement : DatabricksStatement
{
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions;
    private readonly SettlementReportMeteringPointMasterDataQueryFilter _filter;

    public SettlementReportMeteringPointMasterDataCountQueryStatement(IOptions<DeltaTableOptions> deltaTableOptions, SettlementReportMeteringPointMasterDataQueryFilter filter)
    {
        _deltaTableOptions = deltaTableOptions;
        _filter = filter;
    }

    protected override string GetSqlStatement()
    {
        return $"""
                    SELECT COUNT(DISTINCT({SettlementReportMeteringPointMasterDataViewColumns.MeteringPointId})) AS {Columns.Count}
                    FROM
                        {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME}
                    WHERE
                        {SettlementReportMeteringPointMasterDataViewColumns.GridArea} = '{SqlStringSanitizer.Sanitize(_filter.GridAreaCode)}' AND
                        {SettlementReportMeteringPointMasterDataViewColumns.CalculationType} = '{CalculationTypeMapper.ToDeltaTableValue(_filter.CalculationType)}' AND
                        {SettlementReportMeteringPointMasterDataViewColumns.FromDate} >= '{_filter.PeriodStart}' AND
                        {SettlementReportMeteringPointMasterDataViewColumns.ToDate} < '{_filter.PeriodEnd}' AND
                        {(_filter.EnergySupplier is null ? string.Empty : SettlementReportMeteringPointMasterDataViewColumns.EnergySupplierId + " = '" + SqlStringSanitizer.Sanitize(_filter.EnergySupplier) + "' AND")} 
                        {SettlementReportMeteringPointMasterDataViewColumns.CalculationId} = '{_filter.CalculationId}'
                """;
    }

    public static class Columns
    {
        public const string Count = "count";
    }
}
