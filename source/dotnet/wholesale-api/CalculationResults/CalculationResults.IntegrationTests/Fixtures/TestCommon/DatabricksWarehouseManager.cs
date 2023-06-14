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
using Microsoft.Extensions.Options;
using Moq;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures.TestCommon;

/// <summary>
/// A manager for managing Databricks SQL schemas and tables from integration tests.
/// </summary>
public class DatabricksWarehouseManager
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";

    private readonly HttpClient _httpClient;
    private readonly List<string> _createdSchemas = new();

    public DatabricksWarehouseManager(DatabricksWarehouseSettings settings)
    {
        Settings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        _httpClient = CreateHttpClient(Settings);

        // TODO: Create unique schema name and use that
    }

    // TODO: Consider if we can hide these settings or ensure they are readonly in DatabricksWarehouseSettings,
    // otherwise external developers can manipulate them even after we created the manager
    public DatabricksWarehouseSettings Settings { get; }

    public async Task CreateSchemaAsync(string schemaName)
    {
        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = "50s", // Make the operation synchronous
            statement = @$"CREATE SCHEMA IF NOT EXISTS {schemaName}",
            warehouse_id = Settings.WarehouseId,
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
            warehouse_id = Settings.WarehouseId,
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
            warehouse_id = Settings.WarehouseId,
        };

        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new DatabricksSqlException($"Unable to create schema on Databricks. Status code: {response.StatusCode}");

        _createdSchemas.Remove(schemaName);
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
