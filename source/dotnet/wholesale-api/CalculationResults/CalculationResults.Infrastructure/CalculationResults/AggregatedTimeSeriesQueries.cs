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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class AggregatedTimeSeriesQueries(
    DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor,
    AggregatedTimeSeriesQuerySnippetProviderFactory querySnippetProviderFactory,
    IOptions<DeltaTableOptions> deltaTableOptions)
    : RequestQueriesBase(databricksSqlWarehouseQueryExecutor), IAggregatedTimeSeriesQueries
{
    private readonly AggregatedTimeSeriesQuerySnippetProviderFactory _querySnippetProviderFactory = querySnippetProviderFactory;
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions = deltaTableOptions;

    public async IAsyncEnumerable<AggregatedTimeSeries> GetAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        var querySnippetProvider = _querySnippetProviderFactory.Create(parameters);

        var calculationTypePerGridAreas =
            await GetCalculationTypeForGridAreasAsync(
                    EnergyResultColumnNames.GridArea,
                    EnergyResultColumnNames.CalculationType,
                    new AggregatedTimeSeriesCalculationTypeForGridAreasQueryStatement(
                        _deltaTableOptions.Value,
                        querySnippetProvider),
                    parameters.CalculationType)
                .ConfigureAwait(false);

        var sqlStatement = new AggregatedTimeSeriesQueryStatement(
            calculationTypePerGridAreas,
            querySnippetProvider,
            _deltaTableOptions.Value);

        await foreach (var aggregatedTimeSeries in CreateSeriesPackagesAsync(
                           AggregatedTimeSeriesFactory.Create,
                           (currentRow, previousRow) =>
                               AggregatedTimeSeriesQueryStatement.ColumnsToGroupBy.Any(column =>
                                   currentRow[column] != previousRow[column])
                               || currentRow[EnergyResultColumnNames.CalculationId] !=
                               previousRow[EnergyResultColumnNames.CalculationId],
                           EnergyTimeSeriesPointFactory.CreateTimeSeriesPoint,
                           sqlStatement))
        {
            yield return aggregatedTimeSeries;
        }
    }
}
