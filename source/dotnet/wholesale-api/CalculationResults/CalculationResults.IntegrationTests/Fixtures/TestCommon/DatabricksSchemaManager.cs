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

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures.TestCommon;

/// <summary>
/// A manager for managing Databricks SQL schemas and tables from integration tests.
/// </summary>
public class DatabricksSchemaManager
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";
    private readonly HttpClient _httpClient;

    public DatabricksSchemaManager(DatabricksWarehouseSettings settings)
    {
        Settings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        _httpClient = CreateHttpClient(Settings);
        SchemaName = $"TestSchema_{Guid.NewGuid().ToString("N")[..8]}";
    }

    // TODO: Consider if we can hide these settings or ensure they are readonly in DatabricksWarehouseSettings,
    // otherwise external developers can manipulate them even after we created the manager
    public DatabricksWarehouseSettings Settings { get; }

    public string SchemaName { get; }

    /// <summary>
    /// Create schema (formerly known as database).
    /// See more here https://docs.databricks.com/lakehouse/data-objects.html.
    /// </summary>
    public async Task CreateSchemaAsync()
    {
        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = "50s", // Make the operation synchronous
            statement = @$"CREATE SCHEMA {SchemaName}",
            warehouse_id = Settings.WarehouseId,
        };

        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new DatabricksSqlException($"Unable to create schema on Databricks. Status code: {response.StatusCode}");
    }

    /// <summary>
    /// Create table based on with a specified column definition (column name, data type)
    /// See more here https://docs.databricks.com/lakehouse/data-objects.html.
    /// </summary>
    public async Task CreateTableAsync(string tableName, Dictionary<string, string> columnDefinition)
    {
        var columnDefinitions = string.Join(", ", columnDefinition.Select(c => $"{c.Key} {c.Value}"));

        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"50s", // Make the operation synchronous
            statement = $@"CREATE TABLE {SchemaName}.{tableName} ({columnDefinitions})",
            warehouse_id = Settings.WarehouseId,
        };

        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new DatabricksSqlException($"Unable to create table {SchemaName}.{tableName} on Databricks. Status code: {response.StatusCode}");
    }

    public async Task InsertIntoAsync(string tableName, string values)
    {
        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"50s", // Make the operation synchronous
            statement = $@"INSERT INTO {SchemaName}.{tableName} VALUES {values}",
            warehouse_id = Settings.WarehouseId,
        };

        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new DatabricksSqlException($"Unable to execute SQL statement on Databricks. Status code: {response.StatusCode}");
    }

    public async Task DropSchemaAsync()
    {
        var sqlStatement = @$"DROP SCHEMA {SchemaName} CASCADE";

        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"50s", // Make the operation synchronous
            statement = sqlStatement,
            warehouse_id = Settings.WarehouseId,
        };

        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new DatabricksSqlException($"Unable to create schema on Databricks. Status code: {response.StatusCode}");
    }

    private static HttpClient CreateHttpClient(DatabricksWarehouseSettings settings)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(settings.WorkspaceUrl),
        };

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", settings.WorkspaceAccessToken);

        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

        httpClient.BaseAddress = new Uri(settings.WorkspaceUrl);

        return httpClient;
    }
}
