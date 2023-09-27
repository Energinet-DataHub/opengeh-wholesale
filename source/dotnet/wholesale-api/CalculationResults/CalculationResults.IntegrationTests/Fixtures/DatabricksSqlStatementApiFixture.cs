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
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Microsoft.Extensions.Http;
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
        DatabricksOptionsMock = CreateDatabricksSqlStatementOptionsMock(DatabricksSchemaManager);
    }

    public DatabricksSchemaManager DatabricksSchemaManager { get; }

    private Mock<IOptions<Core.Databricks.SqlStatementExecution.AppSettings.DatabricksSqlStatementOptions>> DatabricksOptionsMock { get; }

    public async Task InitializeAsync()
    {
        await DatabricksSchemaManager.CreateSchemaAsync();
    }

    public async Task DisposeAsync()
    {
        await DatabricksSchemaManager.DropSchemaAsync();
    }

    public IDatabricksSqlStatementClient CreateSqlStatementClient(Mock<ILogger<SqlStatusResponseParser>> loggerMock, Mock<ILogger<DatabricksSqlStatementClient>> loggerMock2)
    {
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var sqlChunkResponseParser = new SqlChunkResponseParser();
        var sqlStatementClient = new DatabricksSqlStatementClient(
            httpClientFactoryMock.Object,
            DatabricksOptionsMock.Object,
            new SqlResponseParser(
                new SqlStatusResponseParser(loggerMock.Object, sqlChunkResponseParser),
                sqlChunkResponseParser,
                new SqlChunkDataResponseParser()),
            loggerMock2.Object);
        return sqlStatementClient;
    }

    private static Mock<IOptions<Core.Databricks.SqlStatementExecution.AppSettings.DatabricksSqlStatementOptions>> CreateDatabricksSqlStatementOptionsMock(DatabricksSchemaManager databricksSchemaManager)
    {
        var databricksOptionsMock = new Mock<IOptions<Core.Databricks.SqlStatementExecution.AppSettings.DatabricksSqlStatementOptions>>();
        databricksOptionsMock
            .Setup(o => o.Value)
            .Returns(new Core.Databricks.SqlStatementExecution.AppSettings.DatabricksSqlStatementOptions
            {
                WorkspaceUrl = databricksSchemaManager.Settings.WorkspaceUrl,
                WorkspaceToken = databricksSchemaManager.Settings.WorkspaceAccessToken,
                WarehouseId = databricksSchemaManager.Settings.WarehouseId,
            });

        return databricksOptionsMock;
    }
}
