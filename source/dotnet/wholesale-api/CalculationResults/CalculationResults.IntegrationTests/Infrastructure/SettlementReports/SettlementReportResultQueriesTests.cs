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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Energinet.DataHub.Wholesale.Common.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
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
    private readonly DatabricksSqlStatementApiFixture _fixture;
    private readonly string[] _defaultGridAreaCodes = { "805" };
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

    [Fact]
    public async Task GetRowsAsync_ReturnsExpectedReportRow()
    {
        // Arrange
        var expectedSettlementReportRow = GetDefaultSettlementReportRow();
        var tableName = await CreateTableWithDefaultRow();
        var sqlStatementClient = new SqlStatementClient(new HttpClient(), _fixture.DatabricksOptionsMock.Object, new DatabricksSqlResponseParser());
        var sut = new SettlementReportResultQueries(sqlStatementClient, CreateDeltaTableOptions(_fixture.DatabricksSchemaManager.SchemaName, tableName));

        // Act
        var actual = await sut.GetRowsAsync(_defaultGridAreaCodes, DefaultProcessType, _defaultPeriodStart, _defaultPeriodEnd, null);

        // Assert
        actual.First().Should().Be(expectedSettlementReportRow);
    }

    private async Task<string> CreateTableWithDefaultRow()
    {
        var tableName = $"TestTable_{DateTime.Now:yyyyMMddHHmmss}";
        var columnDefinition = DeltaTableSchema.Result;
        var rowValues = CreateRowValues(columnDefinition.Keys);

        await _fixture.DatabricksSchemaManager.CreateTableAsync(tableName, columnDefinition);
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, rowValues);

        return tableName;
    }

    private static IEnumerable<string> CreateRowValues(IEnumerable<string> columnNames)
    {
        var values = columnNames.Select(columnName => columnName switch
        {
            ResultColumnNames.BatchId => "'ed39dbc5-bdc5-41b9-922a-08d3b12d4538'",
            ResultColumnNames.BatchExecutionTimeStart => "'2022-03-11T03:00:00.000Z'",
            ResultColumnNames.BatchProcessType => $@"'{DeltaTableProcessType.BalanceFixing}'",
            ResultColumnNames.TimeSeriesType => $@"'{DeltaTableTimeSeriesType.Production}'",
            ResultColumnNames.GridArea => "'805'",
            ResultColumnNames.FromGridArea => "'806'",
            ResultColumnNames.BalanceResponsibleId => "'1236552000028'",
            ResultColumnNames.EnergySupplierId => "'2236552000028'",
            ResultColumnNames.Time => "'2022-05-16T03:00:00.000Z'",
            ResultColumnNames.Quantity => "1.123",
            ResultColumnNames.QuantityQuality => "'missing'",
            ResultColumnNames.AggregationLevel => "'total_ga'",
            _ => throw new ArgumentOutOfRangeException($"Unexpected column name: {columnName}."),
        });

        return values;
    }

    private static SettlementReportResultRow GetDefaultSettlementReportRow()
    {
        return new SettlementReportResultRow(
            "805",
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
