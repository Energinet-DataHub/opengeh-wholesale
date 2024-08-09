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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class WholesaleServicesQueries(
    DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor,
    RequestSpecificWholesaleServicesQuerySnippetsProviderFactory querySnippetsProviderFactory,
    IOptions<DeltaTableOptions> deltaTableOptions)
    : RequestQueriesBase(databricksSqlWarehouseQueryExecutor), IWholesaleServicesQueries
{
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions = deltaTableOptions;
    private readonly RequestSpecificWholesaleServicesQuerySnippetsProviderFactory _querySnippetsProviderFactory = querySnippetsProviderFactory;

    public async IAsyncEnumerable<WholesaleServices> GetAsync(WholesaleServicesQueryParameters queryParameters)
    {
        var querySnippetsProvider = _querySnippetsProviderFactory.Create(queryParameters);

        var calculationTypePerGridAreas =
            await GetCalculationTypeForGridAreasAsync(
                    querySnippetsProvider.DatabricksContract.GetGridAreaCodeColumnName(),
                    querySnippetsProvider.DatabricksContract.GetCalculationTypeColumnName(),
                    new WholesaleServicesCalculationTypeForGridAreasStatement(_deltaTableOptions.Value, querySnippetsProvider),
                    queryParameters.CalculationType)
                .ConfigureAwait(false);

        var sqlStatement = new WholesaleServicesQueryStatement(
            WholesaleServicesQueryStatement.StatementType.Select,
            calculationTypePerGridAreas,
            querySnippetsProvider,
            _deltaTableOptions.Value);

        var calculationIdColumn = querySnippetsProvider.DatabricksContract.GetCalculationIdColumnName();

        await foreach (var wholesaleServices in CreateSeriesPackagesAsync(
                           (row, points) => WholesaleServicesFactory.Create(row, queryParameters.AmountType, points),
                           (currentRow, previousRow) => querySnippetsProvider.DatabricksContract.GetColumnsToAggregateBy().Any(column => currentRow[column] != previousRow[column])
                                                        || currentRow[calculationIdColumn] != previousRow[calculationIdColumn],
                           WholesaleTimeSeriesPointFactory.Create,
                           sqlStatement))
        {
            yield return wholesaleServices;
        }
    }

    public async Task<bool> AnyAsync(WholesaleServicesQueryParameters queryParameters)
    {
        var querySnippetsProvider = _querySnippetsProviderFactory.Create(queryParameters);

        var calculationTypePerGridAreas =
            await GetCalculationTypeForGridAreasAsync(
                    querySnippetsProvider.DatabricksContract.GetGridAreaCodeColumnName(),
                    querySnippetsProvider.DatabricksContract.GetCalculationTypeColumnName(),
                    new WholesaleServicesCalculationTypeForGridAreasStatement(_deltaTableOptions.Value, querySnippetsProvider),
                    queryParameters.CalculationType)
                .ConfigureAwait(false);

        var sqlStatement = new WholesaleServicesQueryStatement(
            WholesaleServicesQueryStatement.StatementType.Exists,
            calculationTypePerGridAreas,
            _querySnippetsProviderFactory.Create(queryParameters),
            _deltaTableOptions.Value);

        return await _databricksSqlWarehouseQueryExecutor
            .ExecuteStatementAsync(sqlStatement)
            .AnyAsync()
            .ConfigureAwait(false);
    }
}
