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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.RequestCalculationResult;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.RequestCalculationResult;

public class AggregatedTimeSeriesQueriesCorrectionTests : IClassFixture<DatabricksSqlStatementApiFixture>
{
    private const string BatchId = "019703e7-98ee-45c1-b343-0cbf185a47d9";
    private const string FirstQuantity = "1.111";
    private const string FirstQuantityFirstCorrection = "1.222";
    private const string FirstQuantitySecondCorrection = "1.333";
    private const string FirstQuantityThirdCorrection = "1.444";
    private const string SecondQuantityFirstCorrection = "2.333";
    private const string SecondQuantitySecondCorrection = "2.444";
    private const string SecondQuantityThirdCorrection = "2.555";
    private const string FourthQuantityThirdCorrection = "4.555";
    private const string GridAreaCode = "301";
    private readonly DatabricksSqlStatementApiFixture _fixture;

    public AggregatedTimeSeriesQueriesCorrectionTests(DatabricksSqlStatementApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetProcessTypeOfLatestCorrectionAsync_WhenLatestCorrectionIsThirdCorrection_ReturnsThirdCorrection(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCode;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);

        var parameters = CreateQueryParameters(
            timeSeriesTypeFilter,
            startOfPeriodFilter,
            endOfPeriodFilter,
            gridAreaFilter);

        var sut = await CreateAggregatedTimeSeriesQueries(
            httpClientFactoryMock,
            loggerMock,
            addFirstCorrection: true,
            addSecondCorrection: true,
            addThirdCorrection: true);

        // Act
        var actual = await sut.GetProcessTypeOfLatestCorrectionAsync(parameters);

        // Assert
        actual.Should().Be(ProcessType.ThirdCorrectionSettlement);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetProcessTypeOfLatestCorrectionAsync_WhenLatestCorrectionIsSecondCorrection_ReturnsSecondCorrection(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCode;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);

        var parameters = CreateQueryParameters(
            timeSeriesTypeFilter,
            startOfPeriodFilter,
            endOfPeriodFilter,
            gridAreaFilter);

        var sut = await CreateAggregatedTimeSeriesQueries(
            httpClientFactoryMock,
            loggerMock,
            addFirstCorrection: true,
            addSecondCorrection: true,
            addThirdCorrection: false);

        // Act
        var actual = await sut.GetProcessTypeOfLatestCorrectionAsync(parameters);

        // Assert
        actual.Should().Be(ProcessType.SecondCorrectionSettlement);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetProcessTypeOfLatestCorrectionAsync_WhenLatestCorrectionIsFirstCorrection_ReturnsFirstCorrection(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCode;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var parameters = CreateQueryParameters(
            timeSeriesTypeFilter,
            startOfPeriodFilter,
            endOfPeriodFilter,
            gridAreaFilter);

        var sut = await CreateAggregatedTimeSeriesQueries(
            httpClientFactoryMock,
            loggerMock,
            addFirstCorrection: true,
            addSecondCorrection: false,
            addThirdCorrection: false);

        // Act
        var actual = await sut.GetProcessTypeOfLatestCorrectionAsync(parameters);

        actual.Should().Be(ProcessType.FirstCorrectionSettlement);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetProcessTypeOfLatestCorrectionAsync_WhenNoCorrectionsExists_ReturnsFirstCorrection(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCode;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);

        var parameters = CreateQueryParameters(
            timeSeriesTypeFilter,
            startOfPeriodFilter,
            endOfPeriodFilter,
            gridAreaFilter);

        var sut = await CreateAggregatedTimeSeriesQueries(
            httpClientFactoryMock,
            loggerMock,
            addFirstCorrection: false,
            addSecondCorrection: false,
            addThirdCorrection: false);

        // Act
        var actual = await sut.GetProcessTypeOfLatestCorrectionAsync(parameters);

        // Assert
        actual.Should().Be(ProcessType.FirstCorrectionSettlement);
    }

    private AggregatedTimeSeriesQueryParameters CreateQueryParameters(
        TimeSeriesType? timeSeriesType = null,
        Instant? startOfPeriod = null,
        Instant? endOfPeriod = null,
        string gridArea = "101",
        string? energySupplierId = null,
        string? balanceResponsibleId = null)
    {
        return new AggregatedTimeSeriesQueryParameters(
            TimeSeriesType: timeSeriesType ?? TimeSeriesType.Production,
            StartOfPeriod: startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
            EndOfPeriod: endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0),
            GridArea: gridArea,
            EnergySupplierId: energySupplierId,
            BalanceResponsibleId: balanceResponsibleId);
    }

    private async Task<AggregatedTimeSeriesQueries> CreateAggregatedTimeSeriesQueries(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> sqlStatusResponseParserLoggerMock,
        bool addFirstCorrection = false,
        bool addSecondCorrection = false,
        bool addThirdCorrection = false)
    {
        var sqlStatementClient = _fixture.CreateSqlStatementClient(
            httpClientFactoryMock,
            sqlStatusResponseParserLoggerMock,
            new Mock<ILogger<DatabricksSqlStatementClient>>());

        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        var queryGenerator = new AggregatedTimeSeriesSqlGenerator(deltaTableOptions);
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions, addFirstCorrection, addSecondCorrection, addThirdCorrection);

        var queries = new AggregatedTimeSeriesQueries(sqlStatementClient, queryGenerator);
        return queries;
    }

    private async Task AddCreatedRowsInArbitraryOrderAsync(IOptions<DeltaTableOptions> options, bool addFirstCorrection, bool addSecondCorrection, bool addThirdCorrection)
    {
        const string firstCalculationResultId = "aaaaaaaa-386f-49eb-8b56-63fae62e4fc7";
        const string secondCalculationResultId = "bbbbbbbb-b58b-4190-a873-eded0ed50c20";
        const string thirdCalculationResultId = "cccccccc-386f-49eb-8b56-63fae62e4fc7";

        const string firstHour = "2022-01-01T01:00:00.000Z";
        const string secondHour = "2022-01-01T02:00:00.000Z";
        const string thirdHour = "2022-01-01T03:00:00.000Z";

        var row1 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: firstHour, gridArea: GridAreaCode, quantity: FirstQuantity);

        var row1FirstCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, batchProcessType: DeltaTableProcessType.FirstCorrectionSettlement, time: firstHour, gridArea: GridAreaCode, quantity: FirstQuantityFirstCorrection);
        var row2FirstCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, batchProcessType: DeltaTableProcessType.FirstCorrectionSettlement, time: secondHour, gridArea: GridAreaCode, quantity: SecondQuantityFirstCorrection);

        var row1SecondCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, batchProcessType: DeltaTableProcessType.SecondCorrectionSettlement, time: firstHour, gridArea: GridAreaCode, quantity: FirstQuantitySecondCorrection);
        var row2SecondCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, batchProcessType: DeltaTableProcessType.SecondCorrectionSettlement, time: secondHour, gridArea: GridAreaCode, quantity: SecondQuantitySecondCorrection);

        var row1ThirdCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, batchProcessType: DeltaTableProcessType.ThirdCorrectionSettlement, time: firstHour, gridArea: GridAreaCode, quantity: FirstQuantityThirdCorrection);
        var row2ThirdCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, batchProcessType: DeltaTableProcessType.ThirdCorrectionSettlement, time: secondHour, gridArea: GridAreaCode, quantity: SecondQuantityThirdCorrection);
        var row4ThirdCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, batchProcessType: DeltaTableProcessType.ThirdCorrectionSettlement, time: thirdHour, gridArea: GridAreaCode, quantity: FourthQuantityThirdCorrection);

        var rows = new List<IReadOnlyCollection<string>>
        {
            row1,
        };

        if (addFirstCorrection)
        {
            rows.AddRange(new[]
            {
                row1FirstCorrection,
                row2FirstCorrection,
            });
        }

        if (addSecondCorrection)
        {
            rows.AddRange(new[]
            {
                row1SecondCorrection,
                row2SecondCorrection,
            });
        }

        if (addThirdCorrection)
        {
            rows.AddRange(new[]
            {
                row1ThirdCorrection,
                row2ThirdCorrection,
                row4ThirdCorrection,
            });
        }

        rows = rows.OrderBy(_ => Guid.NewGuid()).ToList();

        await _fixture.DatabricksSchemaManager.EmptyAsync(options.Value.ENERGY_RESULTS_TABLE_NAME);
        await _fixture.DatabricksSchemaManager.InsertAsync<EnergyResultColumnNames>(options.Value.ENERGY_RESULTS_TABLE_NAME, rows);
    }
}
