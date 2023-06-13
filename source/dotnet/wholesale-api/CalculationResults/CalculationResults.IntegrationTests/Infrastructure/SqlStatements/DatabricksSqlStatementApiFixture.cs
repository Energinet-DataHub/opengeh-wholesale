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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.Common.DatabricksClient;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SqlStatements;

public class DatabricksSqlStatementApiFixture : IAsyncLifetime
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";
    private readonly HttpClient _httpClient;
    private readonly List<string> _createdSchemas = new();

    public DatabricksOptions DatabricksOptions { get; }

    public DatabricksSqlStatementApiFixture()
    {
        DatabricksOptions = new DatabricksOptions
        {
            // TODO: use key vault
            DATABRICKS_WAREHOUSE_ID = "anyDatabricksId",
            DATABRICKS_WORKSPACE_URL = "https://anyDatabricksUrl",
            DATABRICKS_WORKSPACE_TOKEN = "myToken",
        };
        _httpClient = CreateHttpClient();
    }

    public async Task CreateSchemaAsync(string schemaName)
    {
        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"50s", // Make the operation synchronous
            statement = @$"CREATE SCHEMA IF NOT EXISTS {schemaName}",
            warehouse_id = DatabricksOptions.DATABRICKS_WAREHOUSE_ID,
        };

        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new DatabricksSqlException($"Unable to create schema on Databricks. Status code: {response.StatusCode}");

        _createdSchemas.Add(schemaName);
    }

    public async Task CreateTableAsync(string schemaName, string tableName, Dictionary<string, string> columnNamesAndTypes)
    {
        var columnDefinitions = string.Join(", ", columnNamesAndTypes.Select(c => $"{c.Key} {c.Value}"));

        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"50s", // Make the operation synchronous
            statement = $@"CREATE TABLE {schemaName}.{tableName} ({columnDefinitions})",
            warehouse_id = DatabricksOptions.DATABRICKS_WAREHOUSE_ID,
        };

        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new DatabricksSqlException($"Unable to create table {schemaName}.{tableName} on Databricks. Status code: {response.StatusCode}");
    }

    /// <summary>
    /// Drop schema
    /// </summary>
    /// <param name="schemaName"></param>
    /// <param name="cascade">If true, drops all the associated tables and functions recursively.</param>
    /// <exception cref="DatabricksSqlException">Exception thrown if the schema is not successfully deleted </exception>
    public async Task DropSchemaAsync(string schemaName, bool cascade)
    {
        var sqlStatement = @$"DROP SCHEMA {schemaName}";
        if (cascade)
            sqlStatement += " CASCADE";

        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"50s", // Make the operation synchronous
            statement = sqlStatement,
            warehouse_id = DatabricksOptions.DATABRICKS_WAREHOUSE_ID,
        };

        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new DatabricksSqlException($"Unable to create schema on Databricks. Status code: {response.StatusCode}");

        _createdSchemas.Remove(schemaName);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (var schema in _createdSchemas)
        {
            await DropSchemaAsync(schema, true).ConfigureAwait(false);
        }
    }

    private HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(DatabricksOptions.DATABRICKS_WORKSPACE_URL);
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", DatabricksOptions.DATABRICKS_WORKSPACE_TOKEN);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        httpClient.BaseAddress = new Uri(DatabricksOptions.DATABRICKS_WORKSPACE_URL);
        return httpClient;
    }
}
