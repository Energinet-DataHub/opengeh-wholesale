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
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public class WholesaleServicesQueryStatement(
    WholesaleServicesQueryStatement.StatementType statementType,
    IReadOnlyCollection<CalculationTypeForGridArea> calculationTypePerGridAreas,
    WholesaleServicesQueryStatementHelper helper,
    DeltaTableOptions deltaTableOptions)
    : DatabricksStatement
{
    private const string WholesaleServicesTemporaryTableName = "wsr";
    private const string ChargesTemporaryTableName = "chrg";
    private const string PackagesWithVersionTemporaryTableName = "pckg";

    private readonly StatementType _statementType = statementType;
    private readonly IReadOnlyCollection<CalculationTypeForGridArea> _calculationTypePerGridAreas = calculationTypePerGridAreas;
    private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;
    private readonly WholesaleServicesQueryStatementHelper _helper = helper;

    protected override string GetSqlStatement()
    {
        var selectTarget = _statementType switch
        {
            StatementType.Select => _helper.GetProjection(ChargesTemporaryTableName),
            StatementType.Exists => "1",
            _ => throw new ArgumentOutOfRangeException(nameof(_statementType), _statementType, "Unknown StatementType"),
        };

        /*
         * SELECT chrg.A, chrg.B, chrg.C, ...
         * FROM (SomeQuery) chrg
         * INNER JOIN (SomeOtherQuery) pckg
         * ON chrg.A = pckg.max_A AND chrg.B = pckg.max_B AND ...
         * ORDER BY chrg.A, chrg.B, ..., chrg.time
         *
         * OR
         *
         * SELECT 1
         * FROM (SomeQuery) chrg
         * INNER JOIN (SomeOtherQuery) pckg
         * ON chrg.A = pckg.max_A AND chrg.B = pckg.max_B AND ...
         * ORDER BY chrg.A, chrg.B, ..., chrg.time
         */
        return $"""
                SELECT {selectTarget}
                FROM ({GetChargesToChooseFrom()}) {ChargesTemporaryTableName}
                INNER JOIN ({GetMaxVersionForEachPackage()}) {PackagesWithVersionTemporaryTableName}
                ON {MatchChargesWithChargePackages(ChargesTemporaryTableName, PackagesWithVersionTemporaryTableName)}
                ORDER BY {string.Join(", ", _helper.GetColumnsToAggregateBy().Select(ctab => $"{ChargesTemporaryTableName}.{ctab}"))}, {ChargesTemporaryTableName}.{_helper.GetTimeColumnName()}
                """;
    }

    public enum StatementType
    {
        Select,
        Exists,
    }

    private string GetChargesToChooseFrom()
    {
        /*
         * SELECT wsr.A, wsr.B, wsr.C, ...
         * FROM SomeTable wsr
         * WHERE wsr.A = a AND wsr.B = b AND ...
         */
        return $"""
                SELECT {_helper.GetProjection(WholesaleServicesTemporaryTableName)}
                FROM {_helper.GetSource(_deltaTableOptions)} {WholesaleServicesTemporaryTableName}
                WHERE {_helper.GetLatestOrFixedCalculationTypeSelection(WholesaleServicesTemporaryTableName, _calculationTypePerGridAreas)}
                """;
    }

    private string GetMaxVersionForEachPackage()
    {
        /*
         * SELECT max(wsr.version) AS max_version, wsr.time AS max_time, wsr.A AS max_A, wsr.B AS max_B, ...
         * FROM SomeTable wsr
         * WHERE wsr.A = a AND wsr.B = b AND ...
         * GROUP BY wsr.time, wsr.A, wsr.B, ...
         */
        return $"""
                SELECT max({WholesaleServicesTemporaryTableName}.{_helper.GetCalculationVersionColumnName()}) AS max_version, {WholesaleServicesTemporaryTableName}.{_helper.GetTimeColumnName()} AS max_time, {string.Join(", ", _helper.GetColumnsToAggregateBy().Select(ctab => $"{WholesaleServicesTemporaryTableName}.{ctab} AS max_{ctab}"))}
                FROM {_helper.GetSource(_deltaTableOptions)} {WholesaleServicesTemporaryTableName}
                WHERE {_helper.GetSelection(WholesaleServicesTemporaryTableName)} AND {_helper.GetLatestOrFixedCalculationTypeSelection(WholesaleServicesTemporaryTableName, _calculationTypePerGridAreas)}
                GROUP BY {WholesaleServicesTemporaryTableName}.{_helper.GetTimeColumnName()}, {string.Join(", ", _helper.GetColumnsToAggregateBy().Select(ctab => $"{WholesaleServicesTemporaryTableName}.{ctab}"))}
                """;
    }

    private string MatchChargesWithChargePackages(string chargesPrefix, string packagesPrefix)
    {
        /*
         * chrg.time = pckg.max_time
         * AND chrg.version = pckg.max_version
         * AND coalesce(chrg.A, 'is_null_value') = coalesce(pckg.max_A, 'is_null_value') AND ...
         */
        return $"""
                {chargesPrefix}.{_helper.GetTimeColumnName()} = {packagesPrefix}.max_time
                AND {chargesPrefix}.{_helper.GetCalculationVersionColumnName()} = {packagesPrefix}.max_version
                AND {string.Join(" AND ", _helper.GetColumnsToAggregateBy().Select(ctgb => $"coalesce({chargesPrefix}.{ctgb}, 'is_null_value') = coalesce({packagesPrefix}.max_{ctgb}, 'is_null_value')"))}
                """;
    }
}
