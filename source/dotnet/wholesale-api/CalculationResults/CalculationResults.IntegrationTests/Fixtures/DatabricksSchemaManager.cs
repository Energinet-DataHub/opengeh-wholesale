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
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;

/// <summary>
/// A manager for managing Databricks SQL schemas and tables from integration tests.
/// </summary>
public class DatabricksSchemaManager
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";
    private readonly HttpClient _httpClient;

    public DatabricksSchemaManager(DatabricksSettings settings, string schemaPrefix)
    {
        Settings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        _httpClient = CreateHttpClient(Settings);
        SchemaName = $"{schemaPrefix}_{DateTime.Now:yyyyMMddHHmmss}";
    }

    // TODO JMG: Consider if we can hide these settings or ensure they are readonly in DatabricksWarehouseSettings,
    // otherwise external developers can manipulate them even after we created the manager
    public DatabricksSettings Settings { get; }

    public string SchemaName { get; }

    /// <summary>
    /// Create schema (formerly known as database).
    /// See more here https://docs.databricks.com/lakehouse/data-objects.html.
    /// </summary>
    public async Task CreateSchemaAsync()
    {
        var sqlStatement = @$"CREATE SCHEMA {SchemaName}";
        await ExecuteSql(sqlStatement);
    }

    /// <summary>
    /// Create table with a specified column definition (column name, data type)
    /// See more here https://docs.databricks.com/lakehouse/data-objects.html.
    /// </summary>
    public async Task CreateTableAsync(string tableName, Dictionary<string, string> columnDefinition)
    {
        var columnDefinitions = string.Join(", ", columnDefinition.Select(c => $"{c.Key} {c.Value}"));
        var sqlStatement = $@"CREATE TABLE {SchemaName}.{tableName} ({columnDefinitions})";
        await ExecuteSql(sqlStatement);
    }

    public async Task InsertIntoAsync(string tableName, IEnumerable<string> values)
    {
        var sqlStatement = $@"INSERT INTO {SchemaName}.{tableName} VALUES ({string.Join(",", values)})";
        await ExecuteSql(sqlStatement);
    }

    public async Task DropSchemaAsync()
    {
        var sqlStatement = @$"DROP SCHEMA {SchemaName} CASCADE";
        await ExecuteSql(sqlStatement);
    }

    private async Task ExecuteSql(string sqlStatement)
    {
        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"50s", // Make the operation synchronous
            statement = sqlStatement,
            warehouse_id = Settings.WarehouseId,
        };
        var httpResponse = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!httpResponse.IsSuccessStatusCode)
            throw new DatabricksSqlException($"Unable to execute SQL statement on Databricks. Status code: {httpResponse.StatusCode}");

        var jsonResponse = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None, };
        var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonResponse, settings) ??
                         throw new InvalidOperationException();

        var state = jsonObject["status"]?["state"]?.ToString() ?? throw new InvalidOperationException("Unable to retrieve 'state' from the responseJsonObject");
        if (state != "SUCCEEDED")
            throw new DatabricksSqlException($"Failed to execute SQL statement: {sqlStatement}. Response: {jsonResponse}");
    }

    private static HttpClient CreateHttpClient(DatabricksSettings settings)
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
