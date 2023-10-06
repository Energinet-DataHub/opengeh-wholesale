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
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Abstractions;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Configuration;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Constants;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;

/// <summary>
/// The interface 'IAsyncLifetime' forces the implementation of two methods.
///   1. 'InitializeAsync()' which is called before the first test in the test class is executed.
///   2. 'DisposeAsync()' which is called after the last test in the test class has been executed.
/// </summary>
public class DatabricksSqlStatementApiFixture : IAsyncLifetime
{
    public DatabricksSqlStatementApiFixture()
    {
        var integrationTestConfiguration = new IntegrationTestConfiguration();
        DatabricksSchemaManager = new DatabricksSchemaManager(integrationTestConfiguration.DatabricksSettings, "wholesale");
        DatabricksSqlStatementOptionsMock = CreateDatabricksOptionsMock(DatabricksSchemaManager);
    }

    public DatabricksSchemaManager DatabricksSchemaManager { get; }

    private Mock<IOptions<DatabricksSqlStatementOptions>> DatabricksSqlStatementOptionsMock { get; }

    public async Task InitializeAsync()
    {
        await DatabricksSchemaManager.CreateSchemaAsync();
    }

    public async Task DisposeAsync()
    {
        await DatabricksSchemaManager.DropSchemaAsync();
    }

    public IDatabricksSqlStatementClient CreateSqlStatementClient(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<DatabricksSqlStatementClient>> loggerMock2)
    {
        var databricksSqlChunkResponseParser = new SqlChunkResponseParser();

        httpClientFactoryMock.Setup(f => f.CreateClient(HttpClientNameConstants.Databricks))
            .Returns(CreateHttpClient(DatabricksSqlStatementOptionsMock.Object.Value));

        var sqlStatementClient = new DatabricksSqlStatementClient(
            httpClientFactoryMock.Object,
            DatabricksSqlStatementOptionsMock.Object,
            new SqlResponseParser(
                new SqlStatusResponseParser(loggerMock.Object, databricksSqlChunkResponseParser),
                databricksSqlChunkResponseParser,
                new SqlChunkDataResponseParser()),
            loggerMock2.Object);
        return sqlStatementClient;
    }

    private static Mock<IOptions<DatabricksSqlStatementOptions>> CreateDatabricksOptionsMock(DatabricksSchemaManager databricksSchemaManager)
    {
        var databricksOptionsMock = new Mock<IOptions<DatabricksSqlStatementOptions>>();
        databricksOptionsMock
            .Setup(o => o.Value)
            .Returns(new DatabricksSqlStatementOptions
            {
                WorkspaceUrl = databricksSchemaManager.Settings.WorkspaceUrl,
                WorkspaceToken = databricksSchemaManager.Settings.WorkspaceAccessToken,
                WarehouseId = databricksSchemaManager.Settings.WarehouseId,
            });

        return databricksOptionsMock;
    }

    private static HttpClient CreateHttpClient(DatabricksSqlStatementOptions databricksOptions)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(databricksOptions.WorkspaceUrl),
        };

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", databricksOptions.WorkspaceToken);

        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

        return httpClient;
    }
}
