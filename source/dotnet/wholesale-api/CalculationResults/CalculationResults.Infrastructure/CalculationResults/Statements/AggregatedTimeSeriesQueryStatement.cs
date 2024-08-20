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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public sealed class AggregatedTimeSeriesQueryStatement(
    IReadOnlyCollection<CalculationTypeForGridArea> calculationTypePerGridAreas,
    AggregatedTimeSeriesQuerySnippetProvider querySnippetProvider,
    TimeSeriesType timeSeriesType,
    DeltaTableOptions deltaTableOptions)
    : DatabricksStatement
{
    private const string EnergyResultTableName = "er";
    private const string EnergyMeasurementTableName = "em";
    private const string PackagesWithVersionTableName = "pckg";

    private readonly IReadOnlyCollection<CalculationTypeForGridArea> _calculationTypePerGridAreas =
        calculationTypePerGridAreas;

    private readonly AggregatedTimeSeriesQuerySnippetProvider _querySnippetProvider = querySnippetProvider;
    private readonly TimeSeriesType _timeSeriesType = timeSeriesType;
    private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;

    protected override string GetSqlStatement()
    {
        /*
         * SELECT em.A, em.B, em.C, ...
         * FROM (SomeQuery) em
         * INNER JOIN (SomeOtherQuery) pckg
         * ON em.A = pckg.A AND em.B = pckg.B AND ...
         * ORDER BY em.A, em.B, ...
         */
        return $"""
                SELECT {_querySnippetProvider.GetProjection(EnergyMeasurementTableName)}
                FROM ({GetEnergyMeasurementsToChooseFrom()}) {EnergyMeasurementTableName}
                INNER JOIN ({GetMaxVersionForEachPackage()}) {PackagesWithVersionTableName}
                ON {MatchEnergyMeasurementsWithPackages(EnergyMeasurementTableName, PackagesWithVersionTableName)}
                ORDER BY {_querySnippetProvider.GetOrdering(EnergyMeasurementTableName)}
                """;
    }

    private string GetEnergyMeasurementsToChooseFrom()
    {
        /*
         * SELECT er.A, er.B, er.C, ...
         * FROM Source er
         * WHERE (SomeConditions) AND (SomeOtherConditions)
         */
        return $"""
                SELECT {_querySnippetProvider.GetProjection(EnergyResultTableName)}
                FROM {_querySnippetProvider.DatabricksContract.GetSource(_deltaTableOptions)} {EnergyResultTableName}
                WHERE {_querySnippetProvider.GetSelection(EnergyResultTableName, _timeSeriesType)} AND {_querySnippetProvider.GetLatestOrFixedCalculationTypeSelection(EnergyResultTableName, _calculationTypePerGridAreas)}
                """;
    }

    private string GetMaxVersionForEachPackage()
    {
        /*
         * SELECT max(er.calculation_version) AS max_version, er.time AS max_time, er.A AS max_A, ...
         * FROM Source er
         * WHERE (SomeConditions) AND (SomeOtherConditions)
         * GROUP BY er.time, er.A, ...
         */
        return $"""
                SELECT max({EnergyResultTableName}.{_querySnippetProvider.DatabricksContract.GetCalculationVersionColumnName()}) AS max_version, {EnergyResultTableName}.{_querySnippetProvider.DatabricksContract.GetTimeColumnName()} AS max_time, {string.Join(", ", _querySnippetProvider.DatabricksContract.GetColumnsToAggregateBy().Select(ctgb => $"{EnergyResultTableName}.{ctgb} AS max_{ctgb}"))}
                FROM {_querySnippetProvider.DatabricksContract.GetSource(_deltaTableOptions)} {EnergyResultTableName}
                WHERE {_querySnippetProvider.GetSelection(EnergyResultTableName, _timeSeriesType)} AND {_querySnippetProvider.GetLatestOrFixedCalculationTypeSelection(EnergyResultTableName, _calculationTypePerGridAreas)}
                GROUP BY {EnergyResultTableName}.{_querySnippetProvider.DatabricksContract.GetTimeColumnName()}, {string.Join(", ", _querySnippetProvider.DatabricksContract.GetColumnsToAggregateBy().Select(ctab => $"{EnergyResultTableName}.{ctab}"))}
                """;
    }

    private string MatchEnergyMeasurementsWithPackages(string energyMeasurementPrefix, string packagesPrefix)
    {
        /*
         * em.time = pckg.max_time
         * AND em.calculation_version = pckg.max_version
         * AND coalesce(em.A, 'is_null_value') = coalesce(pckg.max_A, 'is_null_value') AND ...
         */
        return $"""
                {energyMeasurementPrefix}.{_querySnippetProvider.DatabricksContract.GetTimeColumnName()} = {packagesPrefix}.max_time
                AND {energyMeasurementPrefix}.{_querySnippetProvider.DatabricksContract.GetCalculationVersionColumnName()} = {packagesPrefix}.max_version
                AND {string.Join(" AND ", _querySnippetProvider.DatabricksContract.GetColumnsToAggregateBy().Select(ctab => $"coalesce({energyMeasurementPrefix}.{ctab}, 'is_null_value') = coalesce({packagesPrefix}.max_{ctab}, 'is_null_value')"))}
                """;
    }
}
