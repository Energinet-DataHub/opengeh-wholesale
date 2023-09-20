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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.RequestCalculationResult;
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

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.RequestCalculationResult;

public class RequestCalculationResultQueriesTests : IClassFixture<DatabricksSqlStatementApiFixture>
{
    private const string BatchId = "019703e7-98ee-45c1-b343-0cbf185a47d9";
    private const string FirstQuantity = "1.111";
    private const string SecondQuantity = "2.222";
    private const string ThirdQuantity = "3.333";
    private const string FourthQuantity = "4.444";
    private readonly DatabricksSqlStatementApiFixture _fixture;

    public RequestCalculationResultQueriesTests(DatabricksSqlStatementApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_RequestFromGridOperatorTotalProduction_ReturnsResult(
        Mock<ILogger<DatabricksSqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock)
    {
        // Arrange
        var gridAreaFilter = "101";
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        var sqlStatementClient = _fixture.CreateSqlStatementClient(loggerMock, new Mock<ILogger<SqlStatementClient>>());
        var request = CreateRequest(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);
        var sut = new RequestCalculationResultQueries(sqlStatementClient, deltaTableOptions, requestCalculationResultQueriesLoggerMock.Object);

        // Act
        var actual = await sut.GetAsync(request);

        // Assert
        actual.Should().NotBeNull();
        actual!.GridArea.Should().Be(gridAreaFilter);
        actual.PeriodStart.Should().Be(startOfPeriodFilter);
        actual.PeriodEnd.Should().Be(endOfPeriodFilter);
        actual.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        actual.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantity, ThirdQuantity, FourthQuantity);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_RequestFromGridOperatorTotalProductionInWrongPeriod_ReturnsNoResults(
        Mock<ILogger<DatabricksSqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock)
    {
        // Arrange
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        var sqlStatementClient = _fixture.CreateSqlStatementClient(loggerMock, new Mock<ILogger<SqlStatementClient>>());

        var request = CreateRequest(
            startOfPeriod: Instant.FromUtc(2020, 1, 1, 1, 1),
            endOfPeriod: Instant.FromUtc(2021, 1, 2, 1, 1));
        var sut = new RequestCalculationResultQueries(sqlStatementClient, deltaTableOptions, requestCalculationResultQueriesLoggerMock.Object);

        // Act
        var actual = await sut.GetAsync(request);

        // Assert
        actual.Should().BeNull();
    }

    private CalculationResultQuery CreateRequest(
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
        const string firstHour = "2022-01-01T01:00:00.000Z";
        const string secondHour = "2022-01-01T02:00:00.000Z";
        const string thirdHour = "2022-01-01T03:00:00.000Z";
        const string gridAreaA = "101";

        var row1 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: firstHour, gridArea: gridAreaA, quantity: FirstQuantity);
        var row2 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: secondHour, gridArea: gridAreaA, quantity: SecondQuantity);

        var row3 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, time: secondHour, gridArea: gridAreaA, quantity: ThirdQuantity, batchExecutionTimeStart: "2022-03-12T03:00:00.000Z");
        var row4 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, time: thirdHour, gridArea: gridAreaA, quantity: FourthQuantity, batchExecutionTimeStart: "2022-03-12T03:00:00.000Z");

        // mix up the order of the rows
        var rows = new List<IEnumerable<string>> { row1, row2, row3, row4 };
        await _fixture.DatabricksSchemaManager.EmptyAsync(options.Value.ENERGY_RESULTS_TABLE_NAME);
        await _fixture.DatabricksSchemaManager.InsertAsync<EnergyResultColumnNames>(options.Value.ENERGY_RESULTS_TABLE_NAME, rows);
    }
}
