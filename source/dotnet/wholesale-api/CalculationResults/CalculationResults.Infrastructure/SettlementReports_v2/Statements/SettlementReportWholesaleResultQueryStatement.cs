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

public sealed class SettlementReportWholesaleResultQueryStatement : DatabricksStatement
{
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions;
    private readonly SettlementReportWholesaleResultQueryFilter _filter;
    private readonly int _skip;
    private readonly int _take;

    public SettlementReportWholesaleResultQueryStatement(IOptions<DeltaTableOptions> deltaTableOptions, SettlementReportWholesaleResultQueryFilter filter, int skip, int take)
    {
        _deltaTableOptions = deltaTableOptions;
        _filter = filter;
        _skip = skip;
        _take = take;
    }

    protected override string GetSqlStatement()
    {
        return $"""
                    SELECT {string.Join(", ", [
                        SettlementReportWholesaleViewColumns.CalculationId,
                        SettlementReportWholesaleViewColumns.CalculationType,
                        SettlementReportWholesaleViewColumns.GridArea,
                        SettlementReportWholesaleViewColumns.EnergySupplierId,
                        SettlementReportWholesaleViewColumns.StartDateTime,
                        SettlementReportWholesaleViewColumns.ChargeType,
                        SettlementReportWholesaleViewColumns.ChargeCode,
                        SettlementReportWholesaleViewColumns.ChargeOwnerId,
                        SettlementReportWholesaleViewColumns.Resolution,
                        SettlementReportWholesaleViewColumns.QuantityUnit,
                        SettlementReportWholesaleViewColumns.Currency,
                        SettlementReportWholesaleViewColumns.Quantity,
                        SettlementReportWholesaleViewColumns.Price,
                        SettlementReportWholesaleViewColumns.Amount,
                        SettlementReportWholesaleViewColumns.MeteringPointType,
                        SettlementReportWholesaleViewColumns.SettlementMethod,
                    ])}
                    FROM
                        {_deltaTableOptions.Value.SCHEMA_NAME}.{_deltaTableOptions.Value.WHOLESALE_RESULTS_V1_VIEW_NAME}
                    WHERE 
                        {SettlementReportWholesaleViewColumns.GridArea} = '{_filter.GridAreaCode}' AND
                        {SettlementReportWholesaleViewColumns.CalculationType} = '{CalculationTypeMapper.ToDeltaTableValue(_filter.CalculationType)}' AND
                        {SettlementReportWholesaleViewColumns.StartDateTime} >= '{_filter.PeriodStart}' AND
                        {SettlementReportWholesaleViewColumns.StartDateTime} < '{_filter.PeriodEnd}' AND
                        {SettlementReportWholesaleViewColumns.CalculationId} = '{_filter.CalculationId}'
                    ORDER BY
                        {SettlementReportWholesaleViewColumns.StartDateTime} LIMIT {_take} OFFSET {_skip}
                """;
    }
}
