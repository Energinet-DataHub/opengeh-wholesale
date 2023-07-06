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
    private const string DefaultGridArea = "805";
    private const string SomeOtherGridArea = "111";
    private const ProcessType DefaultProcessType = ProcessType.BalanceFixing;
    private readonly DatabricksSqlStatementApiFixture _fixture;
    private readonly string[] _defaultGridAreaCodes = { DefaultGridArea };
    private readonly Instant _defaultPeriodStart = Instant.FromUtc(2022, 5, 16, 1, 0, 0);
    private readonly Instant _defaultPeriodEnd = Instant.FromUtc(2022, 5, 17, 1, 0, 0);

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
    public async Task GetRowsAsync_ReturnsExpectedReportRow(Mock<ILogger<DatabricksSqlStatusResponseParser>> loggerMock)
    {
        // Arrange
        var expectedSettlementReportRow = GetDefaultSettlementReportRow();
        var tableName = await CreateTableWithTwoRowsAsync();
        var deltaTableOptions = CreateDeltaTableOptions(_fixture.DatabricksSchemaManager.SchemaName, tableName);
        var sqlStatementClient = _fixture.CreateSqlStatementClient(loggerMock);
        var sut = new SettlementReportResultQueries(sqlStatementClient, deltaTableOptions);

        // Act
        var actual = await sut.GetRowsAsync(_defaultGridAreaCodes, DefaultProcessType, _defaultPeriodStart, _defaultPeriodEnd, null);

        // Assert
        var actualList = actual.ToList();
        actualList.Should().HaveCount(1);
        actualList.First().Should().Be(expectedSettlementReportRow);
    }

    private async Task<string> CreateTableWithTwoRowsAsync()
    {
        var columnDefinitions = ResultDeltaTableHelper.GetColumnDefinitions();
        var tableName = await _fixture.DatabricksSchemaManager.CreateTableAsync(columnDefinitions);

        var row1 = _fixture.ResultDeltaTableHelper.CreateRowValues(gridArea: DefaultGridArea);
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, row1);

        var row2 = _fixture.ResultDeltaTableHelper.CreateRowValues(gridArea: SomeOtherGridArea);
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, row2);

        return tableName;
    }

    private static SettlementReportResultRow GetDefaultSettlementReportRow()
    {
        return new SettlementReportResultRow(
            DefaultGridArea,
            ProcessType.BalanceFixing,
            Instant.FromUtc(2022, 5, 16, 3, 0, 0),
            "PT15M",
            MeteringPointType.Production,
            null,
            1.123m);
    }

    private static IOptions<DeltaTableOptions> CreateDeltaTableOptions(string schemaName, string tableName)
    {
        return Options.Create(new DeltaTableOptions { SCHEMA_NAME = schemaName, RESULT_TABLE_NAME = tableName, });
    }
}
