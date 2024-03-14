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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public class WholesaleServicesQueryStatement : DatabricksStatement
{
    private readonly StatementType _statementType;
    private readonly DeltaTableOptions _deltaTableOptions;
    private readonly string? _gridArea;
    private readonly Period _period;
    private readonly IReadOnlyCollection<CalculationForPeriod> _calculations;

    public WholesaleServicesQueryStatement(StatementType statementType, WholesaleServicesQueryParameters queryParameters, DeltaTableOptions deltaTableOptions)
    {
        var (gridArea, period, calculationForPeriods) = queryParameters;
        _gridArea = gridArea;
        _period = period;
        _calculations = calculationForPeriods;
        _statementType = statementType;
        _deltaTableOptions = deltaTableOptions;
    }

    protected override string GetSqlStatement()
    {
        var selectTarget = _statementType switch
        {
            StatementType.Select => $"{string.Join(", ", ColumnsToSelect)}",
            StatementType.Exists => "1",
            _ => throw new ArgumentOutOfRangeException(nameof(_statementType), _statementType, "Unknown StatementType"),
        };

        var sql = $@"
            SELECT {selectTarget}
            FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.WHOLESALE_RESULTS_TABLE_NAME}
            WHERE 
        ";

        var calculationPeriodSql = _calculations
            .Select(calculationForPeriod => $@"
                ({WholesaleResultColumnNames.CalculationId} == '{calculationForPeriod.CalculationId}'  
                AND {WholesaleResultColumnNames.Time} >= '{calculationForPeriod.Period.Start}'
                AND {WholesaleResultColumnNames.Time} < '{calculationForPeriod.Period.End}')")
            .ToList();

        sql += $" AND ({string.Join(" OR ", calculationPeriodSql)})";

        sql += $@"
                ORDER BY {WholesaleResultColumnNames.GridArea},
                -- charge owner?
                -- charge type?
                {WholesaleResultColumnNames.CalculationId},
                {WholesaleResultColumnNames.Time}
                ";

        return sql;
    }

    private static string[] ColumnsToSelect { get; } =
    {
        WholesaleResultColumnNames.GridArea,
        WholesaleResultColumnNames.EnergySupplierId,
        WholesaleResultColumnNames.AmountType,
        WholesaleResultColumnNames.MeteringPointType,
        WholesaleResultColumnNames.SettlementMethod,
        WholesaleResultColumnNames.ChargeType,
        WholesaleResultColumnNames.ChargeCode,
        WholesaleResultColumnNames.ChargeOwnerId,
        WholesaleResultColumnNames.Resolution,
        WholesaleResultColumnNames.IsTax,
        WholesaleResultColumnNames.QuantityUnit,
        WholesaleResultColumnNames.Time,
        WholesaleResultColumnNames.Quantity,
        WholesaleResultColumnNames.QuantityQualities,
        WholesaleResultColumnNames.Price,
        WholesaleResultColumnNames.Amount,
    };

    public enum StatementType
    {
        Select,
        Exists,
    }
}
