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

using System.Globalization;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using FluentAssertions;
using FluentAssertions.Execution;
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
    private const string BatchId = "019703e7-98ee-45c1-b343-0cbf185a47d9";
    private const string FirstQuantity = "1.111";
    private const string SecondQuantity = "2.222";
    private const string ThirdQuantity = "3.333";
    private const string FourthQuantity = "4.444";
    private const string FifthQuantity = "5.555";
    private const string SixthQuantity = "6.666";
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
    public async Task GetAsync_ReturnsExpectedCalculationResult(
        Mock<ILogger<DatabricksSqlStatusResponseParser>> loggerMock,
        Mock<ICalculationsClient> batchesClientMock,
        CalculationDto calculation)
    {
        // Arrange
        const int expectedResultCount = 3;
        var tableName = await CreateTableWithRowsInArbitraryOrderAsync();
        calculation = calculation with { CalculationId = Guid.Parse(BatchId) };
        var sqlStatementClient = _fixture.CreateSqlStatementClient(loggerMock);
        batchesClientMock.Setup(b => b.GetAsync(It.IsAny<Guid>())).ReturnsAsync(calculation);
        var deltaTableOptions = CreateDeltaTableOptions(_fixture.DatabricksSchemaManager.SchemaName, tableName);
        var sut = new CalculationResultQueries(sqlStatementClient, batchesClientMock.Object, deltaTableOptions);

        // Act
        var actual = await sut.GetAsync(calculation.CalculationId).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(expectedResultCount);
        actual.SelectMany(a => a.TimeSeriesPoints)
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantity, SecondQuantity, ThirdQuantity, FourthQuantity, FifthQuantity, SixthQuantity);
    }

    private async Task<string> CreateTableWithRowsInArbitraryOrderAsync()
    {
        var columnDefinitions = ResultDeltaTableHelper.GetColumnDefinitions();
        var tableName = await _fixture.DatabricksSchemaManager.CreateTableAsync(columnDefinitions);
        const string firstCalculationResultId = "b55b6f74-386f-49eb-8b56-63fae62e4fc7";
        const string secondCalculationResultId = "c2bdceba-b58b-4190-a873-eded0ed50c20";
        const string thirdCalculationResultId = "d2bdceba-b58b-4190-a873-eded0ed50c20";
        const string firstHour = "2022-01-01T01:00:00.000Z";
        const string secondHour = "2022-01-01T02:00:00.000Z";
        const string gridAreaA = "301";
        const string gridAreaB = "101";
        const string gridAreaC = "501";

        var row1 = _fixture.ResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: firstHour, gridArea: gridAreaA, quantity: FirstQuantity);
        var row2 = _fixture.ResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: secondHour, gridArea: gridAreaA, quantity: SecondQuantity);

        var row3 = _fixture.ResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, time: firstHour, gridArea: gridAreaB, quantity: ThirdQuantity);
        var row4 = _fixture.ResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, time: secondHour, gridArea: gridAreaB, quantity: FourthQuantity);

        var row5 = _fixture.ResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, time: firstHour, gridArea: gridAreaC, quantity: FifthQuantity);
        var row6 = _fixture.ResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, time: secondHour, gridArea: gridAreaC, quantity: SixthQuantity);

        // mix up the order of the rows
        var rows = new List<IEnumerable<string>> { row3, row5, row1, row2, row6, row4, };
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, rows);

        return tableName;
    }

    private static IOptions<DeltaTableOptions> CreateDeltaTableOptions(string schemaName, string tableName)
    {
        return Options.Create(new DeltaTableOptions { SCHEMA_NAME = schemaName, RESULT_TABLE_NAME = tableName, });
    }
}
