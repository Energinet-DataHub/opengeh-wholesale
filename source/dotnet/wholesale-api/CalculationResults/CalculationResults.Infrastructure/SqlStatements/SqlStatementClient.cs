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
// TODO BJM: Should we avoid referencing the JobsApiClient project "just" to get access to the DatabricksOptions?
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

    public async Task<Table> ExecuteSqlStatementAsync(string sqlStatement)
    {
        const int timeOutPerAttemptSeconds = 30;
        const int maxAttempts = 16; // 8 minutes in total (16 * 30 seconds). The warehouse takes around 5 minutes to start if it has been stopped.

        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"{timeOutPerAttemptSeconds}s", // Make the operation synchronous
            statement = sqlStatement,
            warehouse_id = _options.Value.DATABRICKS_WAREHOUSE_ID,
        };
        // TODO (JMG): Should we use Polly for retrying?
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new DatabricksSqlException($"Unable to get calculation result from Databricks. Status code: {response.StatusCode}");

            var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var databricksSqlResponse = _databricksSqlResponseParser.Parse(jsonResponse);

            if (databricksSqlResponse.State == DatabricksSqlResponseState.Succeeded)
                return databricksSqlResponse.Table!;

            if (databricksSqlResponse.State is not (DatabricksSqlResponseState.Pending or DatabricksSqlResponseState.Cancelled))
                throw new DatabricksSqlException($"Unable to get calculation result from Databricks. State: {databricksSqlResponse.State}");
        }

        throw new DatabricksSqlException($"Unable to get calculation result from Databricks. Max attempts reached ({maxAttempts}) and the state is still not SUCCEEDED.");
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
