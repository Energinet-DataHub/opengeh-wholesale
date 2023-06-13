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
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.Common.DatabricksClient;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SqlStatements;

public class DatabricksSqlStatementApiFixture
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";
    private static readonly DatabricksOptions _databricksOptions = new()
    {
        // TODO: use key vault
        DATABRICKS_WAREHOUSE_ID = "anyDatabricksId",
        DATABRICKS_WORKSPACE_URL = "https://anyDatabricksUrl",
        DATABRICKS_WORKSPACE_TOKEN = "myToken",
    };

    private readonly HttpClient _httpClient = CreateHttpClient();

    public async Task CreateSchemaAsync(string schemaName)
    {
        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"50s", // Make the operation synchronous
            statement = @$"CREATE SCHEMA IF NOT EXISTS {schemaName}",
            warehouse_id = _databricksOptions.DATABRICKS_WAREHOUSE_ID,
        };

        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new DatabricksSqlException($"Unable to create schema on Databricks. Status code: {response.StatusCode}");
    }

    public async Task CreateTableAsync(string schemaName, string tableName, Dictionary<string, string> columnNamesAndTypes)
    {
        var columnDefinitions = string.Join(", ", columnNamesAndTypes.Select(c => $"{c.Key} {c.Value}"));

        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"50s", // Make the operation synchronous
            statement = $@"CREATE TABLE {schemaName}.{tableName} ({columnDefinitions});",
            warehouse_id = _databricksOptions.DATABRICKS_WAREHOUSE_ID,
        };

        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new DatabricksSqlException($"Unable to create table {schemaName}.{tableName} on Databricks. Status code: {response.StatusCode}");
    }

    private static HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(_databricksOptions.DATABRICKS_WORKSPACE_URL);
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _databricksOptions.DATABRICKS_WORKSPACE_TOKEN);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        httpClient.BaseAddress = new Uri(_databricksOptions.DATABRICKS_WORKSPACE_URL);

        return httpClient;
    }
}
