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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;

public sealed class SettlementReportChargePriceQueryStatement : DatabricksStatement
{
    private readonly Guid _calculationId;

    public SettlementReportChargePriceQueryStatement(Guid calculationId)
    {
        _calculationId = calculationId;
    }

    protected override string GetSqlStatement()
    {
        return $"""
                SELECT {string.Join(", ", [
                    SettlementReportChargePriceViewColumns.PeriodStart,
                    SettlementReportChargePriceViewColumns.PeriodEnd,
                    SettlementReportChargePriceViewColumns.Quantity,
                    SettlementReportChargePriceViewColumns.ChargeType,
                    SettlementReportChargePriceViewColumns.ChargeCode,
                    SettlementReportChargePriceViewColumns.ChargeOwnerId,
                    SettlementReportChargePriceViewColumns.MeteringPointId,
                    SettlementReportChargePriceViewColumns.MeteringPointType,
                ])}
                FROM settlement_report.charge_link_periods_v1
                WHERE {SettlementReportChargePriceViewColumns.CalculationId} = '{_calculationId}'
                """;
    }
}
