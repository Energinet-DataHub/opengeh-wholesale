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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;
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
public class DatabricksSqlStatementApiFixture
{
    public DatabricksSqlStatementApiFixture()
    {
        var integrationTestConfiguration = new IntegrationTestConfiguration();
        DatabricksSchemaManager = new DatabricksSchemaManager(integrationTestConfiguration.DatabricksSettings, "wholesale");
        DatabricksOptionsMock = CreateDatabricksOptionsMock(DatabricksSchemaManager);
    }

    public DatabricksSchemaManager DatabricksSchemaManager { get; }

    private Mock<IOptions<Core.Databricks.SqlStatementExecution.Internal.AppSettings.DatabricksOptions>> DatabricksOptionsMock { get; }

    public SqlStatementClient CreateSqlStatementClient(ILogger<DatabricksSqlStatusResponseParser> loggerMock, ILogger<SqlStatementClient> loggerMock2)
    {
        var databricksSqlChunkResponseParser = new DatabricksSqlChunkResponseParser();
        var sqlStatementClient = new SqlStatementClient(
            new HttpClient(),
            DatabricksOptionsMock.Object,
            new DatabricksSqlResponseParser(
                new DatabricksSqlStatusResponseParser(loggerMock, databricksSqlChunkResponseParser),
                databricksSqlChunkResponseParser,
                new DatabricksSqlChunkDataResponseParser()),
            loggerMock2);
        return sqlStatementClient;
    }

    private static Mock<IOptions<Core.Databricks.SqlStatementExecution.Internal.AppSettings.DatabricksOptions>> CreateDatabricksOptionsMock(DatabricksSchemaManager databricksSchemaManager)
    {
        var databricksOptionsMock = new Mock<IOptions<Core.Databricks.SqlStatementExecution.Internal.AppSettings.DatabricksOptions>>();
        databricksOptionsMock
            .Setup(o => o.Value)
            .Returns(new Core.Databricks.SqlStatementExecution.Internal.AppSettings.DatabricksOptions
            {
                WorkspaceUrl = databricksSchemaManager.Settings.WorkspaceUrl,
                WorkspaceToken = databricksSchemaManager.Settings.WorkspaceAccessToken,
                WarehouseId = databricksSchemaManager.Settings.WarehouseId,
            });

        return databricksOptionsMock;
    }
}
