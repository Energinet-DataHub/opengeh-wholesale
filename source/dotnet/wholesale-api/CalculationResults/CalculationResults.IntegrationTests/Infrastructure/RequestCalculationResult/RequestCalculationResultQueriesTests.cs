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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.RequestCalculationResult;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Energinet.DataHub.Wholesale.Common.Models;
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
    private const string FirstQuantityFirstCorrection = "1.222";
    private const string FirstQuantitySecondCorrection = "1.333";
    private const string FirstQuantityThirdCorrection = "1.444";
    private const string SecondQuantity = "2.222";
    private const string SecondQuantityFirstCorrection = "2.333";
    private const string SecondQuantitySecondCorrection = "2.444";
    private const string SecondQuantityThirdCorrection = "2.555";
    private const string ThirdQuantity = "3.333";
    private const string FourthQuantity = "4.444";
    private const string FourthQuantityThirdCorrection = "4.555";
    private const string GridAreaCode = "301";
    private readonly DatabricksSqlStatementApiFixture _fixture;

    public RequestCalculationResultQueriesTests(DatabricksSqlStatementApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_RequestFromGridOperatorTotalProduction_ReturnsResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCode;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        var sqlStatementClient = _fixture.CreateSqlStatementClient(
            httpClientFactoryMock,
            loggerMock,
            new Mock<ILogger<DatabricksSqlStatementClient>>());
        var request = CreateRequest(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);
        var sut = new RequestCalculationResultQueries(sqlStatementClient, deltaTableOptions, requestCalculationResultQueriesLoggerMock.Object);

        // Act
        var actual = await sut.GetAsync(request);

        // Assert
        using var assertionScope = new AssertionScope();
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
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock)
    {
        // Arrange
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        var sqlStatementClient = _fixture.CreateSqlStatementClient(
            httpClientFactoryMock,
            loggerMock,
            new Mock<ILogger<DatabricksSqlStatementClient>>());

        var request = CreateRequest(
            startOfPeriod: Instant.FromUtc(2020, 1, 1, 1, 1),
            endOfPeriod: Instant.FromUtc(2021, 1, 2, 1, 1));
        var sut = new RequestCalculationResultQueries(sqlStatementClient, deltaTableOptions, requestCalculationResultQueriesLoggerMock.Object);

        // Act
        var actual = await sut.GetAsync(request);

        // Assert
        actual.Should().BeNull();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_RequestFromEnergySupplierTotalProduction_ReturnsResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCode;
        var energySupplierIdFilter = "4321987654321";
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        var sqlStatementClient = _fixture.CreateSqlStatementClient(
            httpClientFactoryMock,
            loggerMock,
            new Mock<ILogger<DatabricksSqlStatementClient>>());
        var request = CreateRequest(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            energySupplierId: energySupplierIdFilter);
        var sut = new RequestCalculationResultQueries(sqlStatementClient, deltaTableOptions, requestCalculationResultQueriesLoggerMock.Object);

        // Act
        var actual = await sut.GetAsync(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual!.GridArea.Should().Be(gridAreaFilter);
        actual.PeriodStart.Should().Be(startOfPeriodFilter);
        actual.PeriodEnd.Should().Be(endOfPeriodFilter);
        actual.EnergySupplierId.Should().Be(energySupplierIdFilter);
        actual.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        actual.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantity);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_RequestFromEnergySupplierTotalProductionBadId_ReturnsNoResults(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCode;
        var energySupplierId = "badId";
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        var sqlStatementClient = _fixture.CreateSqlStatementClient(
            httpClientFactoryMock,
            loggerMock,
            new Mock<ILogger<DatabricksSqlStatementClient>>());
        var request = CreateRequest(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            energySupplierId: energySupplierId);
        var sut = new RequestCalculationResultQueries(sqlStatementClient, deltaTableOptions, requestCalculationResultQueriesLoggerMock.Object);

        // Act
        var actual = await sut.GetAsync(request);

        // Assert
        actual.Should().BeNull();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_RequestFromBalanceResponsibleTotalProduction_ReturnsResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCode;
        var balanceResponsibleIdFilter = "1234567891234";
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        var sqlStatementClient = _fixture.CreateSqlStatementClient(
            httpClientFactoryMock,
            loggerMock,
            new Mock<ILogger<DatabricksSqlStatementClient>>());
        var request = CreateRequest(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            balanceResponsibleId: balanceResponsibleIdFilter,
            processType: ProcessType.BalanceFixing);
        var sut = new RequestCalculationResultQueries(sqlStatementClient, deltaTableOptions, requestCalculationResultQueriesLoggerMock.Object);

        // Act
        var actual = await sut.GetAsync(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual!.GridArea.Should().Be(gridAreaFilter);
        actual.PeriodStart.Should().Be(startOfPeriodFilter);
        actual.PeriodEnd.Should().Be(endOfPeriodFilter);
        actual.BalanceResponsibleId.Should().Be(balanceResponsibleIdFilter);
        actual.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        actual.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(SecondQuantity);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_RequestFromEnergySupplierPerBalanceResponsibleTotalProduction_ReturnsResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCode;
        var balanceResponsibleIdFilter = "1234567891234";
        var energySupplierIdFilter = "4321987654321";
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        var sqlStatementClient = _fixture.CreateSqlStatementClient(
            httpClientFactoryMock,
            loggerMock,
            new Mock<ILogger<DatabricksSqlStatementClient>>());
        var request = CreateRequest(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            energySupplierId: energySupplierIdFilter,
            balanceResponsibleId: balanceResponsibleIdFilter);
        var sut = new RequestCalculationResultQueries(sqlStatementClient, deltaTableOptions, requestCalculationResultQueriesLoggerMock.Object);

        // Act
        var actual = await sut.GetAsync(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual!.GridArea.Should().Be(gridAreaFilter);
        actual.PeriodStart.Should().Be(startOfPeriodFilter);
        actual.PeriodEnd.Should().Be(endOfPeriodFilter);
        actual.EnergySupplierId.Should().Be(energySupplierIdFilter);
        actual.BalanceResponsibleId.Should().Be(balanceResponsibleIdFilter);
        actual.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        actual.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(ThirdQuantity);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_RequestFromGridOperatorTotalProductionFirstCorrectionSettlement_ReturnsResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCode;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var processTypeFilter = ProcessType.FirstCorrectionSettlement;
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        var sqlStatementClient = _fixture.CreateSqlStatementClient(
            httpClientFactoryMock,
            loggerMock,
            new Mock<ILogger<DatabricksSqlStatementClient>>());

        var request = CreateRequest(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            processType: processTypeFilter);
        var sut = new RequestCalculationResultQueries(sqlStatementClient, deltaTableOptions, requestCalculationResultQueriesLoggerMock.Object);

        // Act
        var actual = await sut.GetAsync(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual!.GridArea.Should().Be(gridAreaFilter);
        actual.PeriodStart.Should().Be(startOfPeriodFilter);
        actual.PeriodEnd.Should().Be(endOfPeriodFilter);
        actual.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        actual.ProcessType.Should().Be(processTypeFilter);
        actual.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantityFirstCorrection, SecondQuantityFirstCorrection)
            ;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_RequestFromGridOperatorTotalProductionSecondCorrectionSettlement_ReturnsResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCode;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var processTypeFilter = ProcessType.SecondCorrectionSettlement;
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        var sqlStatementClient = _fixture.CreateSqlStatementClient(
            httpClientFactoryMock,
            loggerMock,
            new Mock<ILogger<DatabricksSqlStatementClient>>());

        var request = CreateRequest(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            processType: processTypeFilter);

        var sut = new RequestCalculationResultQueries(sqlStatementClient, deltaTableOptions, requestCalculationResultQueriesLoggerMock.Object);

        // Act
        var actual = await sut.GetAsync(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual!.GridArea.Should().Be(gridAreaFilter);
        actual.PeriodStart.Should().Be(startOfPeriodFilter);
        actual.PeriodEnd.Should().Be(endOfPeriodFilter);
        actual.ProcessType.Should().Be(processTypeFilter);
        actual.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        actual.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantitySecondCorrection, SecondQuantitySecondCorrection)
            ;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenRequestFromGridOperatorTotalProductionThirdCorrectionSettlement_ReturnsResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCode;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var processTypeFilter = ProcessType.ThirdCorrectionSettlement;

        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        var sqlStatementClient = _fixture.CreateSqlStatementClient(
            httpClientFactoryMock,
            loggerMock,
            new Mock<ILogger<DatabricksSqlStatementClient>>());

        var request = CreateRequest(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            processType: processTypeFilter);

        var sut = new RequestCalculationResultQueries(sqlStatementClient, deltaTableOptions, requestCalculationResultQueriesLoggerMock.Object);

        // Act
        var actual = await sut.GetAsync(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual!.GridArea.Should().Be(gridAreaFilter);
        actual.PeriodStart.Should().Be(startOfPeriodFilter);
        actual.PeriodEnd.Should().Be(endOfPeriodFilter);
        actual.ProcessType.Should().Be(processTypeFilter);
        actual.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        actual.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantityThirdCorrection, SecondQuantityThirdCorrection, FourthQuantityThirdCorrection)
            ;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenRequestFromGridOperatorForOneDay_ReturnsResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCode;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 3, 0, 0);

        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        var sqlStatementClient = _fixture.CreateSqlStatementClient(
            httpClientFactoryMock,
            loggerMock,
            new Mock<ILogger<DatabricksSqlStatementClient>>());

        var request = CreateRequest(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        var sut = new RequestCalculationResultQueries(sqlStatementClient, deltaTableOptions, requestCalculationResultQueriesLoggerMock.Object);

        // Act
        var actual = await sut.GetAsync(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual!.GridArea.Should().Be(gridAreaFilter);
        actual.PeriodStart.Should().Be(startOfPeriodFilter);
        actual.PeriodEnd.Should().Be(endOfPeriodFilter);
        actual.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        actual.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantity)
            ;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenRequestFromGridOperatorStartAndEndDataAreEqual_ReturnsNoResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCode;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);

        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        var sqlStatementClient = _fixture.CreateSqlStatementClient(
            httpClientFactoryMock,
            loggerMock,
            new Mock<ILogger<DatabricksSqlStatementClient>>());

        var request = CreateRequest(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        var sut = new RequestCalculationResultQueries(sqlStatementClient, deltaTableOptions, requestCalculationResultQueriesLoggerMock.Object);

        // Act
        var actual = await sut.GetAsync(request);

        // Assert
        actual.Should().BeNull();
    }

    private EnergyResultQuery CreateRequest(
        TimeSeriesType? timeSeriesType = null,
        Instant? startOfPeriod = null,
        Instant? endOfPeriod = null,
        string gridArea = "101",
        string? energySupplierId = null,
        string? balanceResponsibleId = null,
        ProcessType? processType = null)
    {
        return new EnergyResultQuery(
            TimeSeriesType: timeSeriesType ?? TimeSeriesType.Production,
            StartOfPeriod: startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
            EndOfPeriod: endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0),
            GridArea: gridArea,
            EnergySupplierId: energySupplierId,
            BalanceResponsibleId: balanceResponsibleId,
            ProcessType: processType ?? ProcessType.BalanceFixing);
    }

    private async Task AddCreatedRowsInArbitraryOrderAsync(IOptions<DeltaTableOptions> options)
    {
        const string firstCalculationResultId = "aaaaaaaa-386f-49eb-8b56-63fae62e4fc7";
        const string secondCalculationResultId = "bbbbbbbb-b58b-4190-a873-eded0ed50c20";
        const string thirdCalculationResultId = "cccccccc-386f-49eb-8b56-63fae62e4fc7";

        const string firstHour = "2022-01-01T01:00:00.000Z";
        const string secondHour = "2022-01-01T02:00:00.000Z";
        const string thirdHour = "2022-01-01T03:00:00.000Z";

        const string secondDay = "2022-01-02T00:00:00.000Z";
        const string thirdDay = "2022-01-03T00:00:00.000Z";

        const string energySupplier = "4321987654321";
        const string balanceResponsibleId = "1234567891234";
        var row1 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: firstHour, gridArea: GridAreaCode, quantity: FirstQuantity);
        var row2 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: secondHour, gridArea: GridAreaCode, quantity: SecondQuantity);

        var row3 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, time: secondHour, gridArea: GridAreaCode, quantity: ThirdQuantity, batchExecutionTimeStart: "2022-03-12T03:00:00.000Z");
        var row4 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, time: thirdHour, gridArea: GridAreaCode, quantity: FourthQuantity, batchExecutionTimeStart: "2022-03-12T03:00:00.000Z");

        var row5 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: firstHour, gridArea: GridAreaCode, quantity: FirstQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndGridArea, energySupplierId: energySupplier);
        var row6 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, time: secondHour, gridArea: GridAreaCode, quantity: SecondQuantity, aggregationLevel: DeltaTableAggregationLevel.BalanceResponsibleAndGridArea, balanceResponsibleId: balanceResponsibleId);

        var row7 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: secondHour, gridArea: GridAreaCode, quantity: ThirdQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea, balanceResponsibleId: balanceResponsibleId, energySupplierId: energySupplier);

        var row1FirstCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, batchProcessType: DeltaTableProcessType.FirstCorrectionSettlement, time: firstHour, gridArea: GridAreaCode, quantity: FirstQuantityFirstCorrection);
        var row2FirstCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, batchProcessType: DeltaTableProcessType.FirstCorrectionSettlement, time: secondHour, gridArea: GridAreaCode, quantity: SecondQuantityFirstCorrection);

        var row1SecondCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, batchProcessType: DeltaTableProcessType.SecondCorrectionSettlement, time: firstHour, gridArea: GridAreaCode, quantity: FirstQuantitySecondCorrection);
        var row2SecondCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, batchProcessType: DeltaTableProcessType.SecondCorrectionSettlement, time: secondHour, gridArea: GridAreaCode, quantity: SecondQuantitySecondCorrection);

        var row1ThirdCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, batchProcessType: DeltaTableProcessType.ThirdCorrectionSettlement, time: firstHour, gridArea: GridAreaCode, quantity: FirstQuantityThirdCorrection);
        var row2ThirdCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, batchProcessType: DeltaTableProcessType.ThirdCorrectionSettlement, time: secondHour, gridArea: GridAreaCode, quantity: SecondQuantityThirdCorrection);
        var row4ThirdCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, batchProcessType: DeltaTableProcessType.ThirdCorrectionSettlement, time: thirdHour, gridArea: GridAreaCode, quantity: FourthQuantityThirdCorrection);

        var row1SecondDay = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: secondDay, gridArea: GridAreaCode, quantity: FirstQuantity);
        var row1ThirdDay = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: thirdDay, gridArea: GridAreaCode, quantity: SecondQuantity);

        // mix up the order of the rows
        var rows = new List<IReadOnlyCollection<string>>
        {
            row1, row2, row3, row4, row5, row6, row7,
            row1FirstCorrection, row2FirstCorrection,
            row1SecondCorrection, row2SecondCorrection,
            row1ThirdCorrection, row2ThirdCorrection, row4ThirdCorrection,
            row1SecondDay, row1ThirdDay,
        }.OrderBy(r => Guid.NewGuid()).ToList();

        await _fixture.DatabricksSchemaManager.EmptyAsync(options.Value.ENERGY_RESULTS_TABLE_NAME);
        await _fixture.DatabricksSchemaManager.InsertAsync<EnergyResultColumnNames>(options.Value.ENERGY_RESULTS_TABLE_NAME, rows);
    }
}
