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
using System.Reflection;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Exceptions;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;
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

    public IOptions<DeltaTableOptions> DeltaTableOptions { get; }

    public DatabricksSchemaManager(DatabricksSettings settings, string schemaPrefix)
    {
        Settings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        _httpClient = CreateHttpClient(Settings);

        var schemaName = $"{schemaPrefix}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString()[..8]}";
        DeltaTableOptions = Options.Create(new DeltaTableOptions { SCHEMA_NAME = schemaName });
    }

    // TODO JMG: Consider if we can hide these settings or ensure they are readonly in DatabricksWarehouseSettings,
    // otherwise external developers can manipulate them even after we created the manager
    public DatabricksSettings Settings { get; }

    public string SchemaName => DeltaTableOptions.Value.SCHEMA_NAME;

    /// <summary>
    /// Executes all the SQL scripts in the respective folder migration_scripts.
    /// This creates a schema (formerly known as database), tables and constraints.
    /// See more here https://docs.databricks.com/lakehouse/data-objects.html.
    /// </summary>
    public async Task CreateSchemaAsync()
    {
        await ExecuteSqlScriptsAsync();
    }

    public async Task DropSchemaAsync()
    {
        var sqlStatement = @$"DROP SCHEMA {SchemaName} CASCADE";
        await ExecuteSqlAsync(sqlStatement);
    }

    public Task InsertAsync<T>(string tableName, IReadOnlyCollection<string> row)
    {
        return InsertAsync<T>(tableName, new[] { row });
    }

    /// <summary>
    /// Inserts rows into a table. The rows are specified as a list of lists of strings. Example:
    /// INSERT INTO myschema.mytable (name, address, number) VALUES ('someString', 'someOtherString', 1.234), ('anotherString', 'anotherOtherString', 2.345);
    /// </summary>
    /// <param name="tableName">Name of table</param>
    /// <param name="rows">Rows to be inserted in table. Note: that strings should have single quotes around them.
    /// </param>
    public Task InsertAsync<T>(string tableName, IReadOnlyCollection<IReadOnlyCollection<string?>> rows)
    {
        var fieldInfos = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static);
        var columnsNames = string.Join(", ", fieldInfos.Select(x => x.GetValue(null)).Cast<string>());
        var values = string.Join(", ", rows.Select(row => $"({string.Join(", ", row.Select(val => val == null ? "NULL" : $"{val}"))})"));
        var sqlStatement = $@"INSERT INTO {SchemaName}.{tableName} ({columnsNames}) VALUES {values}";
        return ExecuteSqlAsync(sqlStatement);
    }

    public Task EmptyAsync(string tableName)
    {
        var sqlStatement = $@"DELETE FROM {SchemaName}.{tableName}";
        return ExecuteSqlAsync(sqlStatement);
    }

    private async Task ExecuteSqlScriptsAsync()
    {
        var sqlScripts = Directory.EnumerateFiles("./migration_sql_scripts", "*.sql");

        // We only want to split on GO when it is on a line by itself
        string[] delimiters = { "\r\nGO\r\n", "\nGO\n", "\rGO\r" };

        foreach (var sqlScript in sqlScripts)
        {
            var sqlString = await File.ReadAllTextAsync(sqlScript);
            var sqlStatements = sqlString.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            foreach (var sqlStatement in sqlStatements)
            {
                var sql = Replacements(sqlStatement);
                await ExecuteSqlAsync(sql);
            }
        }
    }

    private string Replacements(string sqlStatement)
    {
        return sqlStatement
            .Replace("{OUTPUT_DATABASE_NAME}", SchemaName)
            .Replace("{TEST}", "--");
    }

    private async Task ExecuteSqlAsync(string sqlStatement)
    {
        sqlStatement = sqlStatement.Trim();

        if (string.IsNullOrEmpty(sqlStatement))
            return;

        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"50s", // Make the operation synchronous
            statement = sqlStatement,
            warehouse_id = Settings.WarehouseId,
        };
        var state = string.Empty;
        var jsonResponse = string.Empty;
        var retryCount = 0;
        do
        {
            var httpResponse = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

            if (!httpResponse.IsSuccessStatusCode)
                throw new DatabricksSqlException($"Unable to execute SQL statement on Databricks. Status code: {httpResponse.StatusCode}");

            jsonResponse = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None, };
            var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonResponse, settings) ??
                             throw new InvalidOperationException();

            state = jsonObject["status"]?["state"]?.ToString() ?? throw new InvalidOperationException("Unable to retrieve 'state' from the responseJsonObject");

            /*
            Added this retry logic because sometimes for some unknown reason sql statements that came before the current one seem to not be completed even though the state is SUCCEEDED.
            This can be solved by adding a sleep of 200ms. This is not ideal but it works for now.
            */
            retryCount++;
            if (state != "SUCCEEDED")
                Thread.Sleep(200);
        }
        while (retryCount < 3 && state != "SUCCEEDED");

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
