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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.CalculationResults;

/// <summary>
/// We use an IClassFixture to control the life cycle of the DatabricksSqlStatementApiFixture so:
///   1. It is created and 'InitializeAsync()' is called before the first test in the test class is executed.
///      Use 'InitializeAsync()' to create any schema and seed data.
///   2. 'DisposeAsync()' is called after the last test in the test class has been executed.
///      Use 'DisposeAsync()' to drop any created schema.
/// </summary>
public class CalculationResultQueriesTests : IClassFixture<DatabricksSqlStatementApiFixture>, IAsyncLifetime
{
    private const string FirstCalculationResultId = "b55b6f74-386f-49eb-8b56-63fae62e4fc7";
    private const string SecondCalculationResultId = "c2bdceba-b58b-4190-a873-eded0ed50c20";
    private const string FirstHour = "2022-01-01T01:00:00.000Z";
    private const string SecondHour = "2022-01-01T02:00:00.000Z";
    private const string GridAreaA = "101";
    private const string GridAreaB = "201";
    private readonly DatabricksSqlStatementApiFixture _fixture;

    public CalculationResultQueriesTests(DatabricksSqlStatementApiFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.DatabricksSchemaManager.CreateSchemaAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DatabricksSchemaManager.DropSchemaAsync();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetRowsAsync_ReturnsExpectedReportRow(Mock<ILogger<DatabricksSqlResponseParser>> loggerMock, Mock<IBatchesClient> batchesClientMock, BatchDto batch)
    {
        // Arrange
        var tableName = await CreateTableWithTwoRowsAsync();
        // var batchDto = new BatchDto
        // {
        //     Id = Guid.NewGuid(),
        //     ProcessType = DefaultProcessType,
        //     PeriodStart = _defaultPeriodStart,
        //     PeriodEnd = _defaultPeriodEnd
        // };
        var sqlStatementClient = new SqlStatementClient(new HttpClient(), _fixture.DatabricksOptionsMock.Object, new DatabricksSqlResponseParser(loggerMock.Object));
        batchesClientMock.Setup(b => b.GetAsync(It.IsAny<Guid>())).ReturnsAsync(batch);
        var deltaTableOptions = CreateDeltaTableOptions(_fixture.DatabricksSchemaManager.SchemaName, tableName);
        var sut = new CalculationResultQueries(sqlStatementClient, batchesClientMock.Object, deltaTableOptions);

        // Act
        var actual = sut.GetAsync(batch.BatchId);

        // Assert
        // var actualList = actual.ToList();
        // actualList.Should().HaveCount(1);
        // actualList.First().Should().Be(expectedSettlementReportRow);
    }

    private async Task<string> CreateTableWithTwoRowsAsync()
    {
        var columnDefinitions = ResultDeltaTableHelper.GetColumnDefinitions();
        var tableName = await _fixture.DatabricksSchemaManager.CreateTableAsync(columnDefinitions);

        var row1 = _fixture.ResultDeltaTableHelper.CreateRowValues(calculationResultId: FirstCalculationResultId, time: FirstHour, gridArea: GridAreaA);
        var row2 = _fixture.ResultDeltaTableHelper.CreateRowValues(calculationResultId: SecondCalculationResultId, time: FirstHour, gridArea: GridAreaA);
        var row3 = _fixture.ResultDeltaTableHelper.CreateRowValues(calculationResultId: FirstCalculationResultId, time: SecondHour, gridArea: GridAreaA);
        var row4 = _fixture.ResultDeltaTableHelper.CreateRowValues(calculationResultId: SecondCalculationResultId, time: SecondHour, gridArea: GridAreaA);
        var row5 = _fixture.ResultDeltaTableHelper.CreateRowValues(calculationResultId: FirstCalculationResultId, time: FirstHour, gridArea: GridAreaB);
        var row6 = _fixture.ResultDeltaTableHelper.CreateRowValues(calculationResultId: FirstCalculationResultId, time: SecondHour, gridArea: GridAreaB);
        var row7 = _fixture.ResultDeltaTableHelper.CreateRowValues(calculationResultId: SecondCalculationResultId, time: SecondHour, gridArea: GridAreaB);
        var row8 = _fixture.ResultDeltaTableHelper.CreateRowValues(calculationResultId: SecondCalculationResultId, time: FirstHour, gridArea: GridAreaB);

        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, row1);
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, row2);
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, row3);
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, row4);
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, row5);
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, row6);
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, row7);
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, row8);

        return tableName;
    }

    private static IOptions<DeltaTableOptions> CreateDeltaTableOptions(string schemaName, string tableName)
    {
        return Options.Create(new DeltaTableOptions { SCHEMA_NAME = schemaName, RESULT_TABLE_NAME = tableName, });
    }
}
