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

using System.Net.Http.Headers;
using System.Net.Http.Json;
using Energinet.DataHub.Wholesale.Common.DatabricksClient;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-7.0
// https://learn.microsoft.com/en-gb/azure/databricks/sql/api/sql-execution-tutorial
public class SqlStatementClient : ISqlStatementClient
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";
    private readonly HttpClient _httpClient;
    private readonly IOptions<DatabricksOptions> _options;
    private readonly IDatabricksSqlResponseParser _databricksSqlResponseParser;

    public SqlStatementClient(HttpClient httpClient, IOptions<DatabricksOptions> options, IDatabricksSqlResponseParser databricksSqlResponseParser)
    {
        _httpClient = httpClient;
        _options = options;
        _databricksSqlResponseParser = databricksSqlResponseParser;
        ConfigureHttpClient(_httpClient, _options);
    }

    public async IAsyncEnumerable<TableChunk> ExecuteAsync(string sqlStatement)
    {
        var hasMoreRows = false;
        var path = string.Empty;

        var response = await BeginExecuteAsync(sqlStatement).ConfigureAwait(false);

        if (response.State is DatabricksSqlResponseState.Cancelled or DatabricksSqlResponseState.Failed or DatabricksSqlResponseState.Closed)
            throw new DatabricksSqlException($"Unable to get calculation result from Databricks. State: {response.State}");

        if (response.State is DatabricksSqlResponseState.Pending or DatabricksSqlResponseState.Running)
        {
            hasMoreRows = true;
            path = $"{StatementsEndpointPath}/{response.StatementId}";
        }

        if (response.State == DatabricksSqlResponseState.Succeeded)
        {
            hasMoreRows = response.HasMoreRows;
            path = response.NextChunkInternalLink!;
            yield return response.Table!;
        }

        while (hasMoreRows)
        {
            var httpResponse = await _httpClient.GetAsync(path).ConfigureAwait(false);
            if (!httpResponse.IsSuccessStatusCode)
                throw new DatabricksSqlException($"Unable to get calculation result from Databricks. HTTP status code: {httpResponse.StatusCode}");

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var databricksSqlResponse = _databricksSqlResponseParser.Parse(jsonResponse);

            if (response.State is DatabricksSqlResponseState.Cancelled or DatabricksSqlResponseState.Failed or DatabricksSqlResponseState.Closed)
                throw new DatabricksSqlException($"Unable to get calculation result from Databricks. State: {response.State}");

            // Handle the case where the statement is still pending
            if (databricksSqlResponse.State is DatabricksSqlResponseState.Pending or DatabricksSqlResponseState.Running)
            {
                await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                continue;
            }

            // Handle the case where the statement did not succeed
            if (databricksSqlResponse.State is not DatabricksSqlResponseState.Succeeded)
                throw new DatabricksSqlException($"Unable to get calculation result from Databricks. State: {databricksSqlResponse.State}");

            if (databricksSqlResponse.State == DatabricksSqlResponseState.Succeeded)
                yield return databricksSqlResponse.Table!;

            hasMoreRows = databricksSqlResponse.HasMoreRows;
            path = databricksSqlResponse.NextChunkInternalLink;
        }
    }

    /// <summary>
    /// Begins executing the SQL statement and returns the ID of the statement.
    /// </summary>
    private async Task<DatabricksSqlResponse> BeginExecuteAsync(string sqlStatement)
    {
        const int timeOutPerAttemptSeconds = 30;

        var requestObject = new
        {
            wait_timeout = $"{timeOutPerAttemptSeconds}s", // Make the operation synchronous
            statement = sqlStatement,
            warehouse_id = _options.Value.DATABRICKS_WAREHOUSE_ID,
        };
        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new DatabricksSqlException($"Unable to get calculation result from Databricks. HTTP status code: {response.StatusCode}");

        var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return _databricksSqlResponseParser.Parse(jsonResponse);
    }

    private static void ConfigureHttpClient(HttpClient httpClient, IOptions<DatabricksOptions> options)
    {
        httpClient.BaseAddress = new Uri(options.Value.DATABRICKS_WORKSPACE_URL);
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", options.Value.DATABRICKS_WORKSPACE_TOKEN);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        httpClient.BaseAddress = new Uri(options.Value.DATABRICKS_WORKSPACE_URL);
    }
}
