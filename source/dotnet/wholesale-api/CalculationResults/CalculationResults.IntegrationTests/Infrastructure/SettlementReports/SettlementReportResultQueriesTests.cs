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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Energinet.DataHub.Wholesale.Common.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SettlementReports;

/// <summary>
/// We use an IClassFixture to control the life cycle of the DatabricksSqlStatementApiFixture so:
///   1. It is created and 'InitializeAsync()' is called before the first test in the test class is executed.
///      Use 'InitializeAsync()' to create any schema and seed data.
///   2. 'DisposeAsync()' is called after the last test in the test class has been executed.
///      Use 'DisposeAsync()' to drop any created schema.
/// </summary>
public class SettlementReportResultQueriesTests : IClassFixture<DatabricksSqlStatementApiFixture>, IAsyncLifetime
{
    private const ProcessType DefaultProcessType = ProcessType.BalanceFixing;
    private const string GridAreaA = "805";
    private const string GridAreaB = "111";
    private readonly string[] _gridAreaCodes = { GridAreaA, GridAreaB };
    private readonly DatabricksSqlStatementApiFixture _fixture;
    private readonly Instant _january1st = Instant.FromUtc(2022, 1, 1, 0, 0, 0);
    private readonly Instant _january5th = Instant.FromUtc(2022, 1, 5, 0, 0, 0);

    public SettlementReportResultQueriesTests(DatabricksSqlStatementApiFixture fixture)
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
    public async Task GetRowsAsync_ReturnsExpectedReportRows(Mock<ILogger<DatabricksSqlResponseParser>> loggerMock)
    {
        // Arrange
        var tableName = await CreateTable();
        var expectedSettlementReportRow = await CreateTableWithRowsFromMultipleBatches(tableName);
        var sqlStatementClient = new SqlStatementClient(new HttpClient(), _fixture.DatabricksOptionsMock.Object, new DatabricksSqlResponseParser(loggerMock.Object));
        var deltaTableOptions = CreateDeltaTableOptions(_fixture.DatabricksSchemaManager.SchemaName, tableName);
        var sut = new SettlementReportResultQueries(sqlStatementClient, deltaTableOptions);

        // Act
        var actual = await sut.GetRowsAsync(_gridAreaCodes, DefaultProcessType, _january1st, _january5th, null);

        // Assert
        actual.Should().BeEquivalentTo(expectedSettlementReportRow);
    }

    private async Task<string> CreateTable()
    {
        var columnDefinitions = ResultDeltaTableHelper.GetColumnDefinitions();
        return await _fixture.DatabricksSchemaManager.CreateTableAsync(columnDefinitions);
    }

    private async Task<List<SettlementReportResultRow>> CreateTableWithRowsFromMultipleBatches(string tableName)
    {
        const string january1st = "2022-01-01T01:00:00.000Z";
        const string january2nd = "2022-01-02T01:00:00.000Z";
        const string january3rd = "2022-01-03T01:00:00.000Z";
        const string may1st = "2022-05-01T01:00:00.000Z";
        const string june1st = "2022-06-01T01:00:00.000Z";

        // Batch 1: Balance fixing, ExecutionTime=june1st, Period: 01/01 to 02/01 (include)
        const string quantity11 = "1.100";
        const string quantity12 = "1.200";
        var batch1Row1 = ResultDeltaTableHelper.CreateRowValues(batchExecutionTimeStart: may1st, time: january1st, batchProcessType: DeltaTableProcessType.BalanceFixing, gridArea: GridAreaA, quantity: quantity11);
        var batch1Row2 = ResultDeltaTableHelper.CreateRowValues(batchExecutionTimeStart: may1st, time: january2nd, batchProcessType: DeltaTableProcessType.BalanceFixing, gridArea: GridAreaA, quantity: quantity12);

        // Batch 2: Same as batch 1, but for other grid area (include)
        const string quantity21 = "2.100";
        const string quantity22 = "2.200";
        var batch2Row1 = ResultDeltaTableHelper.CreateRowValues(batchExecutionTimeStart: may1st, time: january1st, batchProcessType: DeltaTableProcessType.BalanceFixing, gridArea: GridAreaB, quantity: quantity21);
        var batch2Row2 = ResultDeltaTableHelper.CreateRowValues(batchExecutionTimeStart: may1st, time: january2nd, batchProcessType: DeltaTableProcessType.BalanceFixing, gridArea: GridAreaB, quantity: quantity22);

        // Batch 3: Same as batch 1, but only partly covering the same period (include the uncovered part)
        const string quantity31 = "3.100";
        const string quantity32 = "3.200";
        var batch3Row1 = ResultDeltaTableHelper.CreateRowValues(batchExecutionTimeStart: may1st, time: january2nd, batchProcessType: DeltaTableProcessType.BalanceFixing, gridArea: GridAreaA, quantity: quantity31);
        var batch3Row2 = ResultDeltaTableHelper.CreateRowValues(batchExecutionTimeStart: may1st, time: january3rd, batchProcessType: DeltaTableProcessType.BalanceFixing, gridArea: GridAreaA, quantity: quantity32);

        // Batch 4: Same as batch 1, but newer and for Aggregation (exclude)
        const string quantity41 = "4.100";
        var batch4Row1 = ResultDeltaTableHelper.CreateRowValues(batchExecutionTimeStart: june1st, time: january1st, batchProcessType: DeltaTableProcessType.Aggregation, gridArea: GridAreaA, quantity: quantity41);
        const string quantity42 = "4.200";
        var batch4Row2 = ResultDeltaTableHelper.CreateRowValues(batchExecutionTimeStart: june1st, time: january2nd, batchProcessType: DeltaTableProcessType.Aggregation, gridArea: GridAreaA, quantity: quantity42);

        var rows = new List<IEnumerable<string>> { batch1Row1, batch1Row2, batch2Row1, batch2Row2, batch3Row1, batch3Row2, batch4Row1, batch4Row2, };
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, rows);

        var expectedSettlementReportRows = new List<SettlementReportResultRow>
        {
            GetSettlementReportRow(GridAreaA, january1st, quantity11),
            GetSettlementReportRow(GridAreaA, january2nd, quantity12),
            GetSettlementReportRow(GridAreaA, january3rd, quantity32),
            GetSettlementReportRow(GridAreaB, january1st, quantity21),
            GetSettlementReportRow(GridAreaB, january2nd, quantity22),
        };

        return expectedSettlementReportRows;
    }

    private static SettlementReportResultRow GetSettlementReportRow(string gridArea, string time, string quantity)
    {
        return new SettlementReportResultRow(
            gridArea,
            ProcessType.BalanceFixing,
            SqlResultValueConverters.ToInstant(time) ?? throw new Exception("Could not parse time"),
            "PT15M",
            MeteringPointType.Production,
            null,
            SqlResultValueConverters.ToDecimal(quantity) ?? throw new Exception("Could not parse time"));
    }

    private static IOptions<DeltaTableOptions> CreateDeltaTableOptions(string schemaName, string tableName)
    {
        return Options.Create(new DeltaTableOptions { SCHEMA_NAME = schemaName, RESULT_TABLE_NAME = tableName, });
    }
}
