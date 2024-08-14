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
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public class AggregatedTimeSeriesQueryStatement(
    IReadOnlyCollection<CalculationTypeForGridArea> calculationTypePerGridAreas,
    AggregatedTimeSeriesQuerySnippetProvider querySnippetProvider,
    DeltaTableOptions deltaTableOptions)
    : DatabricksStatement
{
    private const string EnergyResultTableName = "er";
    private const string EnergyMeasurementTableName = "chrg";
    private const string PackagesWithVersionTableName = "pckg";

    private readonly IReadOnlyCollection<CalculationTypeForGridArea> _calculationTypePerGridAreas =
        calculationTypePerGridAreas;

    private readonly AggregatedTimeSeriesQuerySnippetProvider _querySnippetProvider = querySnippetProvider;
    private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;

    protected override string GetSqlStatement()
    {
        var newSql = $"""
                      SELECT {string.Join(", ", _querySnippetProvider.DatabricksContract.GetColumnsToProject().Select(cn => $"{EnergyMeasurementTableName}.{cn}"))}
                      FROM ({GetEnergyMeasurementsToChooseFrom()}) {EnergyMeasurementTableName}
                      INNER JOIN ({GetMaxVersionForEachPackage()}) {PackagesWithVersionTableName}
                      ON {MatchEnergyMeasurementsWithPackages(EnergyMeasurementTableName, PackagesWithVersionTableName)}
                      ORDER BY {string.Join(", ", _querySnippetProvider.DatabricksContract.GetColumnsToAggregateBy().Select(cn => $"{EnergyMeasurementTableName}.{cn}"))}, {EnergyMeasurementTableName}.{EnergyResultColumnNames.Time};
                      """;

        var sql = $"""
                   SELECT {string.Join(", ", _querySnippetProvider.DatabricksContract.GetColumnsToProject().Select(scn => $"`erv`.`{scn}`"))}, `erv`.`{BasisDataCalculationsColumnNames.Version}`
                   FROM (SELECT {string.Join(", ", _querySnippetProvider.DatabricksContract.GetColumnsToProject().Select(scn => $"`er`.`{scn}`"))}, `cs`.`{BasisDataCalculationsColumnNames.Version}`
                   FROM {_querySnippetProvider.DatabricksContract.GetSource(_deltaTableOptions)} er
                   INNER JOIN {_deltaTableOptions.BasisDataSchemaName}.{_deltaTableOptions.CALCULATIONS_TABLE_NAME} cs
                   ON er.{EnergyResultColumnNames.CalculationId} = cs.{BasisDataCalculationsColumnNames.CalculationId}
                   WHERE {_querySnippetProvider.GenerateLatestOrFixedCalculationTypeWhereClause(_calculationTypePerGridAreas)}) erv
                   INNER JOIN (SELECT max({BasisDataCalculationsColumnNames.Version}) AS max_version, {EnergyResultColumnNames.Time} AS max_time, {string.Join(", ", _querySnippetProvider.DatabricksContract.GetColumnsToAggregateBy().Select(ctgb => $"{ctgb} AS max_{ctgb}"))}
                   FROM {_querySnippetProvider.DatabricksContract.GetSource(_deltaTableOptions)} er
                   INNER JOIN {_deltaTableOptions.BasisDataSchemaName}.{_deltaTableOptions.CALCULATIONS_TABLE_NAME} cs
                   ON er.{EnergyResultColumnNames.CalculationId} = cs.{BasisDataCalculationsColumnNames.CalculationId}
                   {_querySnippetProvider.GetWhereClauseSqlExpression("er")} AND {_querySnippetProvider.GenerateLatestOrFixedCalculationTypeWhereClause(_calculationTypePerGridAreas)}
                   GROUP BY {EnergyResultColumnNames.Time}, {string.Join(", ", _querySnippetProvider.DatabricksContract.GetColumnsToAggregateBy())}) maxver
                   ON erv.{EnergyResultColumnNames.Time} = maxver.max_time AND erv.{BasisDataCalculationsColumnNames.Version} = maxver.max_version AND {string.Join(" AND ", _querySnippetProvider.DatabricksContract.GetColumnsToAggregateBy().Select(ctgb => $"coalesce(erv.{ctgb}, 'is_null_value') = coalesce(maxver.max_{ctgb}, 'is_null_value')"))}
                   {_querySnippetProvider.GetWhereClauseSqlExpression("erv")}
                   ORDER BY {string.Join(", ", _querySnippetProvider.DatabricksContract.GetColumnsToAggregateBy())}, {EnergyResultColumnNames.Time};
                   """;

        return newSql;
    }

    private string GetEnergyMeasurementsToChooseFrom()
    {
        return $"""
                SELECT {string.Join(", ", _querySnippetProvider.DatabricksContract.GetColumnsToProject().Select(cn => $"{EnergyResultTableName}.{cn}"))}
                FROM {_querySnippetProvider.DatabricksContract.GetSource(_deltaTableOptions)} {EnergyResultTableName}
                WHERE {_querySnippetProvider.GenerateLatestOrFixedCalculationTypeWhereClause(_calculationTypePerGridAreas)}
                """;
    }

    private string GetMaxVersionForEachPackage()
    {
        return $"""
                SELECT max({EnergyPerGaViewColumnNames.CalculationVersion}) AS max_version, {EnergyResultColumnNames.Time} AS max_time, {string.Join(", ", _querySnippetProvider.DatabricksContract.GetColumnsToAggregateBy().Select(ctgb => $"{ctgb} AS max_{ctgb}"))}
                FROM {_querySnippetProvider.DatabricksContract.GetSource(_deltaTableOptions)} {EnergyResultTableName}
                {_querySnippetProvider.GetWhereClauseSqlExpression("er")} AND {_querySnippetProvider.GenerateLatestOrFixedCalculationTypeWhereClause(_calculationTypePerGridAreas)}
                GROUP BY {EnergyResultColumnNames.Time}, {string.Join(", ", _querySnippetProvider.DatabricksContract.GetColumnsToAggregateBy())}
                """;
    }

    private string MatchEnergyMeasurementsWithPackages(string energyMeasurementPrefix, string packagesPrefix)
    {
        return $"""
                {energyMeasurementPrefix}.{EnergyResultColumnNames.Time} = {packagesPrefix}.max_time
                AND {energyMeasurementPrefix}.{EnergyPerGaViewColumnNames.CalculationVersion} = {packagesPrefix}.max_version
                AND {string.Join(" AND ", _querySnippetProvider.DatabricksContract.GetColumnsToAggregateBy().Select(ctgb => $"coalesce({energyMeasurementPrefix}.{ctgb}, 'is_null_value') = coalesce({packagesPrefix}.max_{ctgb}, 'is_null_value')"))}
                """;
    }
}
