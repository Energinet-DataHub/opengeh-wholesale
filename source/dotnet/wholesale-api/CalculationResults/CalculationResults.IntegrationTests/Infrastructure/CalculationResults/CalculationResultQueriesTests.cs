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
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.CalculationResults;

public class CalculationResultQueriesTests : IAsyncLifetime
{
    private const string BatchId = "019703e7-98ee-45c1-b343-0cbf185a47d9";
    private const string FirstQuantity = "1.111";
    private const string SecondQuantity = "2.222";
    private const string ThirdQuantity = "3.333";
    private const string FourthQuantity = "4.444";
    private const string FifthQuantity = "5.555";
    private const string SixthQuantity = "6.666";
    private readonly DatabricksSqlStatementApiFixture _fixture;
    private readonly ILogger<DatabricksSqlStatusResponseParser> _loggerResponseParserStub;
    private readonly ILogger<SqlStatementClient> _loggerSqlClientStub;
    private readonly ILogger<CalculationResultQueries> _loggerCalculationResultQueriesStub;

    public CalculationResultQueriesTests(Mock<ILogger<DatabricksSqlStatusResponseParser>> loggerResponseParserStub, Mock<ILogger<SqlStatementClient>> loggerSqlClientStub, Mock<ILogger<CalculationResultQueries>> loggerCalculationResultQueriesStub)
    {
        _fixture = new DatabricksSqlStatementApiFixture();
        _loggerResponseParserStub = loggerResponseParserStub.Object;
        _loggerSqlClientStub = loggerSqlClientStub.Object;
        _loggerCalculationResultQueriesStub = loggerCalculationResultQueriesStub.Object;
    }

    public Task InitializeAsync()
    {
        return _fixture.DatabricksSchemaManager.CreateSchemaAsync();
    }

    public Task DisposeAsync()
    {
        return _fixture.DatabricksSchemaManager.DropSchemaAsync();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_ReturnsExpectedCalculationResult(Mock<IBatchesClient> batchesClientMock, BatchDto batch)
    {
        // Arrange
        const int expectedResultCount = 3;
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        batch = batch with { BatchId = Guid.Parse(BatchId) };
        var sqlStatementClient = _fixture.CreateSqlStatementClient(_loggerResponseParserStub, _loggerSqlClientStub);
        batchesClientMock.Setup(b => b.GetAsync(It.IsAny<Guid>())).ReturnsAsync(batch);
        var sut = new CalculationResultQueries(sqlStatementClient, batchesClientMock.Object, deltaTableOptions, _loggerCalculationResultQueriesStub);

        // Act
        var actual = await sut.GetAsync(batch.BatchId).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(expectedResultCount);
        actual.SelectMany(a => a.TimeSeriesPoints)
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantity, SecondQuantity, ThirdQuantity, FourthQuantity, FifthQuantity, FifthQuantity, SixthQuantity, SixthQuantity);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_RequestFromGridOperatorTotalProduction_ReturnsResult(Mock<IBatchesClient> batchesClientMock)
    {
        // Arrange
        const string gridAreaFilter = "101";
        const TimeSeriesType timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        var sqlStatementClient = _fixture.CreateSqlStatementClient(_loggerResponseParserStub, _loggerSqlClientStub);
        var request = CreateRequest(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);
        var sut = new CalculationResultQueries(sqlStatementClient, batchesClientMock.Object, deltaTableOptions, _loggerCalculationResultQueriesStub);

        // Act
        var actual = await sut.GetAsync(request).ToListAsync();

        // Assert
        actual.Should().NotBeEmpty();
        actual.Count.Should().Be(1);
        actual.Should().OnlyContain(
            result => result.GridArea.Equals(gridAreaFilter)
              && result.PeriodStart == startOfPeriodFilter
              && result.PeriodEnd == endOfPeriodFilter
              && result.TimeSeriesType.Equals(timeSeriesTypeFilter));
        actual.Select(x => x.TimeSeriesPoints).Count().Should().Be(1);
        actual.SelectMany(a => a.TimeSeriesPoints)
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FifthQuantity, SixthQuantity);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_RequestFromGridOperatorTotalProductionInWrongPeriod_ReturnsNoResults(Mock<IBatchesClient> batchesClientMock)
    {
        // Arrange
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        var sqlStatementClient = _fixture.CreateSqlStatementClient(_loggerResponseParserStub, _loggerSqlClientStub);

        var request = CreateRequest(
            startOfPeriod: Instant.FromUtc(2020, 1, 1, 1, 1),
            endOfPeriod: Instant.FromUtc(2021, 1, 2, 1, 1));
        var sut = new CalculationResultQueries(sqlStatementClient, batchesClientMock.Object, deltaTableOptions, _loggerCalculationResultQueriesStub);

        // Act
        var actual = await sut.GetAsync(request).ToListAsync();

        // Assert
        actual.Should().BeEmpty();
    }

    private static CalculationResultQuery CreateRequest(
        TimeSeriesType? timeSeriesType = null,
        Instant? startOfPeriod = null,
        Instant? endOfPeriod = null,
        string gridArea = "101")
    {
        return new CalculationResultQuery(
            TimeSeriesType: timeSeriesType ?? TimeSeriesType.Production,
            StartOfPeriod: startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
            EndOfPeriod: endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0),
            GridArea: gridArea);
    }

    private async Task AddCreatedRowsInArbitraryOrderAsync(IOptions<DeltaTableOptions> options)
    {
        const string firstCalculationResultId = "aaaaaaaa-386f-49eb-8b56-63fae62e4fc7";
        const string secondCalculationResultId = "bbbbbbbb-b58b-4190-a873-eded0ed50c20";
        const string thirdCalculationResultId = "cccccccc-b58b-4190-a873-eded0ed50c20";
        const string firstHour = "2022-01-01T01:00:00.000Z";
        const string secondHour = "2022-01-01T02:00:00.000Z";
        const string gridAreaA = "301";
        const string gridAreaB = "101";
        const string gridAreaC = "501";

        var row1 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: firstHour, gridArea: gridAreaA, quantity: FirstQuantity);
        var row2 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: secondHour, gridArea: gridAreaA, quantity: SecondQuantity);

        var row3 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, time: firstHour, gridArea: gridAreaB, quantity: ThirdQuantity);
        var row4 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, time: secondHour, gridArea: gridAreaB, quantity: FourthQuantity);

        var row5 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, time: firstHour, gridArea: gridAreaC, quantity: FifthQuantity);
        var row6 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, time: secondHour, gridArea: gridAreaC, quantity: SixthQuantity);

        var row7 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, time: firstHour, gridArea: gridAreaB, quantity: FifthQuantity, batchExecutionTimeStart: "2022-03-12T03:00:00.000Z");
        var row8 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, time: secondHour, gridArea: gridAreaB, quantity: SixthQuantity, batchExecutionTimeStart: "2022-03-12T03:00:00.000Z");

        // mix up the order of the rows
        var rows = new List<IEnumerable<string>> { row3, row5, row1, row2, row6, row4, row7, row8 };
        await _fixture.DatabricksSchemaManager.InsertAsync<EnergyResultColumnNames>(options.Value.ENERGY_RESULTS_TABLE_NAME, rows);
    }
}
