﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public class TotalMonthlyAmountResultQueryStatement : DatabricksStatement
{
    private readonly DeltaTableOptions _deltaTableOptions;
    private readonly Guid _calculationId;

    public TotalMonthlyAmountResultQueryStatement(Guid calculationId, DeltaTableOptions deltaTableOptions)
    {
        _deltaTableOptions = deltaTableOptions;
        _calculationId = calculationId;
    }

    protected override string GetSqlStatement()
    {
        return $@"
            SELECT {string.Join(", ", SqlColumnNames)}
            FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.TOTAL_MONTHLY_AMOUNTS_TABLE_NAME}
            WHERE {WholesaleResultColumnNames.CalculationId} = '{_calculationId}'
            ORDER BY {WholesaleResultColumnNames.CalculationResultId}, {WholesaleResultColumnNames.Time}
            ";
    }

    public static string[] SqlColumnNames { get; } =
    {
        TotalMonthlyAmountsColumnNames.CalculationResultId,
        TotalMonthlyAmountsColumnNames.CalculationId,
        TotalMonthlyAmountsColumnNames.CalculationType,
        TotalMonthlyAmountsColumnNames.GridAreaCode,
        TotalMonthlyAmountsColumnNames.EnergySupplierId,
        TotalMonthlyAmountsColumnNames.ChargeOwnerId,
        TotalMonthlyAmountsColumnNames.Amount,
    };
}
