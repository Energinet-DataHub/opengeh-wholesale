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

public class SettlementReportWholesaleResultQueryStatement : DatabricksStatement
{
    private readonly Guid _calculationId;

    public SettlementReportWholesaleResultQueryStatement(Guid calculationId)
    {
        _calculationId = calculationId;
    }

    protected override string GetSqlStatement()
    {
        return $"""
                SELECT {string.Join(", ", [
                    ColumnNames.CalculationId,
                    ColumnNames.CalculationType,
                    ColumnNames.GridArea,
                    ColumnNames.EnergySupplierId,
                    ColumnNames.Time,
                    ColumnNames.ChargeType,
                    ColumnNames.ChargeCode,
                    ColumnNames.ChargeOwnerId,
                    ColumnNames.Resolution,
                    ColumnNames.QuantityUnit,
                    ColumnNames.Currency,
                    ColumnNames.Quantity,
                    ColumnNames.Price,
                    ColumnNames.Amount,
                ])}
                FROM settlement_report.wholesale_results_v1
                WHERE {ColumnNames.CalculationId} = '{_calculationId}'
                """;
    }

    public static class ColumnNames
    {
        public const string CalculationId = "calculation_id";
        public const string CalculationType = "calculation_type";
        public const string GridArea = "grid_area_code";
        public const string EnergySupplierId = "energy_supplier_id";
        public const string Time = "time";
        public const string Resolution = "resolution";
        public const string QuantityUnit = "quantity_unit";
        public const string Currency = "currency";
        public const string Quantity = "quantity";
        public const string Price = "price";
        public const string Amount = "amount";
        public const string ChargeType = "charge_type";
        public const string ChargeCode = "charge_code";
        public const string ChargeOwnerId = "charge_owner_id";
    }
}
