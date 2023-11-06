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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
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

public class RequestCalculationResultRetrieverTests : IClassFixture<DatabricksSqlStatementApiFixture>
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
    private const string GridAreaCodeD = "401";
    private const string GridAreaCodeC = "301";
    private const string GridAreaCodeB = "201";
    private const string GridAreaCodeA = "101";
    private readonly DatabricksSqlStatementApiFixture _fixture;

    public RequestCalculationResultRetrieverTests(DatabricksSqlStatementApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetRequestCalculationResultAsync_RequestFromGridOperatorTotalProduction_ReturnsCorrectResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> sqlStatusResponseParserLoggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock,
        Mock<ILogger<Application.RequestCalculationResult.RequestCalculationResultRetriever>> requestCalculationResultLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);

        var filter = CreateFilter(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        var queries = await CreateRequestCalculationResultQueries(requestCalculationResultQueriesLoggerMock, httpClientFactoryMock, sqlStatusResponseParserLoggerMock);

        var sut = new Application.RequestCalculationResult.RequestCalculationResultRetriever(requestCalculationResultLoggerMock.Object, queries);

        // Act
        var results = await sut.GetRequestCalculationResultAsync(filter, RequestedProcessType.BalanceFixing).ToListAsync();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        var actual = results.First();
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
    public async Task GetRequestCalculationResultAsync_RequestFromGridOperatorTotalProductionInWrongPeriod_ReturnsNoResults(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock,
        Mock<ILogger<Application.RequestCalculationResult.RequestCalculationResultRetriever>> requestCalculationResultLoggerMock)
    {
        // Arrange
        var filter = CreateFilter(
            startOfPeriod: Instant.FromUtc(2020, 1, 1, 1, 1),
            endOfPeriod: Instant.FromUtc(2021, 1, 2, 1, 1));
        var queries = await CreateRequestCalculationResultQueries(requestCalculationResultQueriesLoggerMock, httpClientFactoryMock, loggerMock);

        var sut = new Application.RequestCalculationResult.RequestCalculationResultRetriever(requestCalculationResultLoggerMock.Object, queries);

        // Act
        var results = await sut.GetRequestCalculationResultAsync(filter, RequestedProcessType.BalanceFixing).ToListAsync();

        // Assert
        results.Should().HaveCount(0);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetRequestCalculationResultAsync_RequestFromEnergySupplierTotalProduction_ReturnsCorrectResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock,
        Mock<ILogger<Application.RequestCalculationResult.RequestCalculationResultRetriever>> requestCalculationResultLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var energySupplierIdFilter = "4321987654321";
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var filter = CreateFilter(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            energySupplierId: energySupplierIdFilter);

        var queries = await CreateRequestCalculationResultQueries(requestCalculationResultQueriesLoggerMock, httpClientFactoryMock, loggerMock);

        var sut = new Application.RequestCalculationResult.RequestCalculationResultRetriever(requestCalculationResultLoggerMock.Object, queries);

        // Act
        var results = await sut.GetRequestCalculationResultAsync(filter, RequestedProcessType.BalanceFixing).ToListAsync();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        var actual = results.First();
        using var assertionScope = new AssertionScope();
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
    public async Task GetRequestCalculationResultAsync_RequestFromEnergySupplierTotalProductionBadId_ReturnsNoResults(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock,
        Mock<ILogger<Application.RequestCalculationResult.RequestCalculationResultRetriever>> requestCalculationResultLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var energySupplierId = "badId";
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var filter = CreateFilter(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            energySupplierId: energySupplierId);
        var queries = await CreateRequestCalculationResultQueries(requestCalculationResultQueriesLoggerMock, httpClientFactoryMock, loggerMock);

        var sut = new Application.RequestCalculationResult.RequestCalculationResultRetriever(requestCalculationResultLoggerMock.Object, queries);

        // Act
        var results = await sut.GetRequestCalculationResultAsync(filter, RequestedProcessType.BalanceFixing).ToListAsync();

        // Assert
        results.Should().HaveCount(0);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetRequestCalculationResultAsync_RequestFromBalanceResponsibleTotalProduction_ReturnsCorrectResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock,
        Mock<ILogger<Application.RequestCalculationResult.RequestCalculationResultRetriever>> requestCalculationResultLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var balanceResponsibleIdFilter = "1234567891234";
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var filter = CreateFilter(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            balanceResponsibleId: balanceResponsibleIdFilter);
        var queries = await CreateRequestCalculationResultQueries(requestCalculationResultQueriesLoggerMock, httpClientFactoryMock, loggerMock);

        var sut = new Application.RequestCalculationResult.RequestCalculationResultRetriever(requestCalculationResultLoggerMock.Object, queries);

        // Act
        var results = await sut.GetRequestCalculationResultAsync(filter, RequestedProcessType.BalanceFixing).ToListAsync();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        var actual = results.First();
        using var assertionScope = new AssertionScope();
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
    public async Task GetRequestCalculationResultAsync_RequestFromEnergySupplierPerBalanceResponsibleTotalProduction_ReturnsCorrectResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock,
        Mock<ILogger<Application.RequestCalculationResult.RequestCalculationResultRetriever>> requestCalculationResultLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var balanceResponsibleIdFilter = "1234567891234";
        var energySupplierIdFilter = "4321987654321";
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var filter = CreateFilter(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            energySupplierId: energySupplierIdFilter,
            balanceResponsibleId: balanceResponsibleIdFilter);
        var queries = await CreateRequestCalculationResultQueries(requestCalculationResultQueriesLoggerMock, httpClientFactoryMock, loggerMock);

        var sut = new Application.RequestCalculationResult.RequestCalculationResultRetriever(requestCalculationResultLoggerMock.Object, queries);

        // Act
        var results = await sut.GetRequestCalculationResultAsync(filter, RequestedProcessType.BalanceFixing).ToListAsync();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        var actual = results.First();

        using var assertionScope = new AssertionScope();
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
    public async Task GetRequestCalculationResultAsync_RequestFromGridOperatorTotalProductionFirstCorrectionSettlement_ReturnsCorrectResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock,
        Mock<ILogger<Application.RequestCalculationResult.RequestCalculationResultRetriever>> requestCalculationResultLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var filter = CreateFilter(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);
        var queries = await CreateRequestCalculationResultQueries(requestCalculationResultQueriesLoggerMock, httpClientFactoryMock, loggerMock);

        var sut = new Application.RequestCalculationResult.RequestCalculationResultRetriever(requestCalculationResultLoggerMock.Object, queries);

        // Act
        var results = await sut.GetRequestCalculationResultAsync(filter, RequestedProcessType.FirstCorrection).ToListAsync();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        var actual = results.First();

        using var assertionScope = new AssertionScope();
        actual!.GridArea.Should().Be(gridAreaFilter);
        actual.PeriodStart.Should().Be(startOfPeriodFilter);
        actual.PeriodEnd.Should().Be(endOfPeriodFilter);
        actual.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        actual.ProcessType.Should().Be(ProcessType.FirstCorrectionSettlement);
        actual.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantityFirstCorrection, SecondQuantityFirstCorrection)
            ;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetRequestCalculationResultAsync_RequestFromGridOperatorTotalProductionSecondCorrectionSettlement_ReturnsCorrectResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock,
        Mock<ILogger<Application.RequestCalculationResult.RequestCalculationResultRetriever>> requestCalculationResultLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var filter = CreateFilter(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);
        var queries = await CreateRequestCalculationResultQueries(requestCalculationResultQueriesLoggerMock, httpClientFactoryMock, loggerMock);

        var sut = new Application.RequestCalculationResult.RequestCalculationResultRetriever(requestCalculationResultLoggerMock.Object, queries);

        // Act
        var results = await sut.GetRequestCalculationResultAsync(filter, RequestedProcessType.SecondCorrection).ToListAsync();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        var actual = results.First();

        using var assertionScope = new AssertionScope();
        actual!.GridArea.Should().Be(gridAreaFilter);
        actual.PeriodStart.Should().Be(startOfPeriodFilter);
        actual.PeriodEnd.Should().Be(endOfPeriodFilter);
        actual.ProcessType.Should().Be(ProcessType.SecondCorrectionSettlement);
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
    public async Task GetRequestCalculationResultAsync_RequestFromGridOperatorTotalProductionThirdCorrectionSettlement_ReturnsCorrectResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock,
        Mock<ILogger<Application.RequestCalculationResult.RequestCalculationResultRetriever>> requestCalculationResultLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var filter = CreateFilter(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);
        var queries = await CreateRequestCalculationResultQueries(requestCalculationResultQueriesLoggerMock, httpClientFactoryMock, loggerMock);

        var sut = new Application.RequestCalculationResult.RequestCalculationResultRetriever(requestCalculationResultLoggerMock.Object, queries);

        // Act
        var results = await sut.GetRequestCalculationResultAsync(filter, RequestedProcessType.ThirdCorrection).ToListAsync();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        var actual = results.First();

        using var assertionScope = new AssertionScope();
        actual!.GridArea.Should().Be(gridAreaFilter);
        actual.PeriodStart.Should().Be(startOfPeriodFilter);
        actual.PeriodEnd.Should().Be(endOfPeriodFilter);
        actual.ProcessType.Should().Be(ProcessType.ThirdCorrectionSettlement);
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
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock,
        Mock<ILogger<Application.RequestCalculationResult.RequestCalculationResultRetriever>> requestCalculationResultLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 3, 0, 0);
        var request = CreateFilter(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);
        var queries = await CreateRequestCalculationResultQueries(requestCalculationResultQueriesLoggerMock, httpClientFactoryMock, loggerMock);

        var sut = new Application.RequestCalculationResult.RequestCalculationResultRetriever(requestCalculationResultLoggerMock.Object, queries);

        // Act
        var results = await sut.GetRequestCalculationResultAsync(request, RequestedProcessType.BalanceFixing).ToListAsync();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        var actual = results.First();

        using var assertionScope = new AssertionScope();
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
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock,
        Mock<ILogger<Application.RequestCalculationResult.RequestCalculationResultRetriever>> requestCalculationResultLoggerMock)
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var request = CreateFilter(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);
        var queries = await CreateRequestCalculationResultQueries(requestCalculationResultQueriesLoggerMock, httpClientFactoryMock, loggerMock);

        var sut = new Application.RequestCalculationResult.RequestCalculationResultRetriever(requestCalculationResultLoggerMock.Object, queries);

        // Act
        var results = await sut.GetRequestCalculationResultAsync(request, RequestedProcessType.BalanceFixing).ToListAsync();

        // Assert
        results.Should().HaveCount(0);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetRequestCalculationResultAsync_LatestCorrectionSettlementIsThirdCorrection_ReturnsThirdCorrection(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock,
        Mock<ILogger<Application.RequestCalculationResult.RequestCalculationResultRetriever>> requestCalculationResultLoggerMock)
    {
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var filter = CreateFilter(
            timeSeriesTypeFilter,
            startOfPeriodFilter,
            endOfPeriodFilter,
            gridAreaFilter);
        var queries = await CreateRequestCalculationResultQueries(requestCalculationResultQueriesLoggerMock, httpClientFactoryMock, loggerMock);

        var sut = new Application.RequestCalculationResult.RequestCalculationResultRetriever(requestCalculationResultLoggerMock.Object, queries);

        // Act
        var results = await sut.GetRequestCalculationResultAsync(filter, RequestedProcessType.LatestCorrection).ToListAsync();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        var actual = results.First();

        actual!.ProcessType.Should().Be(ProcessType.ThirdCorrectionSettlement);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetRequestCalculationResultAsync_LatestCorrectionSettlementIsSecondCorrection_ReturnsSecondCorrection(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock,
        Mock<ILogger<Application.RequestCalculationResult.RequestCalculationResultRetriever>> requestCalculationResultLoggerMock)
    {
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);

        var filter = CreateFilter(
            timeSeriesTypeFilter,
            startOfPeriodFilter,
            endOfPeriodFilter,
            gridAreaFilter);
        var queries = await CreateRequestCalculationResultQueries(requestCalculationResultQueriesLoggerMock, httpClientFactoryMock, loggerMock, addThirdCorrection: false);

        var sut = new Application.RequestCalculationResult.RequestCalculationResultRetriever(requestCalculationResultLoggerMock.Object, queries);

        // Act
        var results = await sut.GetRequestCalculationResultAsync(filter, RequestedProcessType.LatestCorrection).ToListAsync();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        var actual = results.First();
        actual!.ProcessType.Should().Be(ProcessType.SecondCorrectionSettlement);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetRequestCalculationResultAsync_LatestCorrectionSettlementIsFirstCorrection_ReturnsFirstCorrection(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock,
        Mock<ILogger<Application.RequestCalculationResult.RequestCalculationResultRetriever>> requestCalculationResultLoggerMock)
    {
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var filter = CreateFilter(
            timeSeriesTypeFilter,
            startOfPeriodFilter,
            endOfPeriodFilter,
            gridAreaFilter);
        var queries = await CreateRequestCalculationResultQueries(requestCalculationResultQueriesLoggerMock, httpClientFactoryMock, loggerMock, addSecondCorrection: false, addThirdCorrection: false);

        var sut = new Application.RequestCalculationResult.RequestCalculationResultRetriever(requestCalculationResultLoggerMock.Object, queries);

        // Act
        var results = await sut.GetRequestCalculationResultAsync(filter, RequestedProcessType.LatestCorrection).ToListAsync();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        var actual = results.First();
        actual!.ProcessType.Should().Be(ProcessType.FirstCorrectionSettlement);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetRequestCalculationResultAsync_NoCorrectionsExists_ReturnsNoResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock,
        Mock<ILogger<Application.RequestCalculationResult.RequestCalculationResultRetriever>> requestCalculationResultLoggerMock)
    {
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var filter = CreateFilter(
            timeSeriesTypeFilter,
            startOfPeriodFilter,
            endOfPeriodFilter,
            gridAreaFilter);
        var queries = await CreateRequestCalculationResultQueries(requestCalculationResultQueriesLoggerMock, httpClientFactoryMock, loggerMock, addFirstCorrection: false, addSecondCorrection: false, addThirdCorrection: false);

        var sut = new Application.RequestCalculationResult.RequestCalculationResultRetriever(requestCalculationResultLoggerMock.Object, queries);

        // Act
        var results = await sut.GetRequestCalculationResultAsync(filter, RequestedProcessType.LatestCorrection).ToListAsync();

        results.Should().HaveCount(0);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetRequestCalculationResultAsync_WhenRequestFromEnergySupplierPerBalanceResponsibleTotalProduction_ReturnsTwoResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock,
        Mock<ILogger<Application.RequestCalculationResult.RequestCalculationResultRetriever>> requestCalculationResultLoggerMock)
    {
        // Arrange
        var balanceResponsibleIdFilter = "1234567891234";
        var energySupplierIdFilter = "4321987654321";
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var filter = CreateFilter(
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            energySupplierId: energySupplierIdFilter,
            balanceResponsibleId: balanceResponsibleIdFilter);

        var queries = await CreateRequestCalculationResultQueries(requestCalculationResultQueriesLoggerMock, httpClientFactoryMock, loggerMock, addFirstCorrection: false, addSecondCorrection: false, addThirdCorrection: false);

        var sut = new Application.RequestCalculationResult.RequestCalculationResultRetriever(requestCalculationResultLoggerMock.Object, queries);

        // Act
        var actual = await sut.GetRequestCalculationResultAsync(filter, RequestedProcessType.BalanceFixing).ToListAsync();

        // Assert
        actual.Should().NotBeNull();
        actual.Should().HaveCount(2);
        actual.Select(result => result.GridArea).Should().Equal(GridAreaCodeB, GridAreaCodeC);
        actual.Should().AllSatisfy(energyResult => energyResult.TimeSeriesType.Should().Be(timeSeriesTypeFilter));
        actual.Should().AllSatisfy(energyResult => energyResult.PeriodStart.Should().Be(startOfPeriodFilter));
        actual.Should().AllSatisfy(energyResult => energyResult.PeriodEnd.Should().Be(endOfPeriodFilter));
        actual.Should().AllSatisfy(energyResult => energyResult.EnergySupplierId.Should().Be(energySupplierIdFilter));
        actual.Should().AllSatisfy(energyResult => energyResult.BalanceResponsibleId.Should().Be(balanceResponsibleIdFilter));
    }

    private EnergyResultFilter CreateFilter(
        TimeSeriesType? timeSeriesType = null,
        Instant? startOfPeriod = null,
        Instant? endOfPeriod = null,
        string? gridArea = null,
        string? energySupplierId = null,
        string? balanceResponsibleId = null)
    {
        return new EnergyResultFilter(
            TimeSeriesType: timeSeriesType ?? TimeSeriesType.Production,
            StartOfPeriod: startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
            EndOfPeriod: endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0),
            GridArea: gridArea,
            EnergySupplierId: energySupplierId,
            BalanceResponsibleId: balanceResponsibleId);
    }

    private async Task<RequestCalculationResultQueries> CreateRequestCalculationResultQueries(
        Mock<ILogger<RequestCalculationResultQueries>> requestCalculationResultQueriesLoggerMock,
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> sqlStatusResponseParserLoggerMock,
        bool addFirstCorrection = true,
        bool addSecondCorrection = true,
        bool addThirdCorrection = true)
    {
        var sqlStatementClient = _fixture.CreateSqlStatementClient(
            httpClientFactoryMock,
            sqlStatusResponseParserLoggerMock,
            new Mock<ILogger<DatabricksSqlStatementClient>>());

        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions, addFirstCorrection, addSecondCorrection, addThirdCorrection);

        var queries = new RequestCalculationResultQueries(sqlStatementClient, deltaTableOptions, requestCalculationResultQueriesLoggerMock.Object);
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
        const string fourthHour = "2022-01-01T04:00:00.000Z";

        const string secondDay = "2022-01-02T00:00:00.000Z";
        const string thirdDay = "2022-01-03T00:00:00.000Z";

        const string energySupplier = "4321987654321";
        const string balanceResponsibleId = "1234567891234";
        var row1 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: firstHour, gridArea: GridAreaCodeC, quantity: FirstQuantity);
        var row2 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: secondHour, gridArea: GridAreaCodeC, quantity: SecondQuantity);

        var row3 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, time: secondHour, gridArea: GridAreaCodeC, quantity: ThirdQuantity, batchExecutionTimeStart: "2022-03-12T03:00:00.000Z");
        var row4 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, time: thirdHour, gridArea: GridAreaCodeC, quantity: FourthQuantity, batchExecutionTimeStart: "2022-03-12T03:00:00.000Z");

        var row5 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: firstHour, gridArea: GridAreaCodeC, quantity: FirstQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndGridArea, energySupplierId: energySupplier);
        var row6 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, time: secondHour, gridArea: GridAreaCodeC, quantity: SecondQuantity, aggregationLevel: DeltaTableAggregationLevel.BalanceResponsibleAndGridArea, balanceResponsibleId: balanceResponsibleId);

        var row7 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: secondHour, gridArea: GridAreaCodeC, quantity: ThirdQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea, balanceResponsibleId: balanceResponsibleId, energySupplierId: energySupplier);
        var row8 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: thirdHour, gridArea: GridAreaCodeB, quantity: FourthQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea, balanceResponsibleId: balanceResponsibleId, energySupplierId: energySupplier);

        var row9 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: firstHour, gridArea: GridAreaCodeA, quantity: FirstQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea);
        var row10 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: fourthHour, gridArea: GridAreaCodeB, quantity: FourthQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea, balanceResponsibleId: balanceResponsibleId, energySupplierId: energySupplier);

        var row1FirstCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, batchProcessType: DeltaTableProcessType.FirstCorrectionSettlement, time: firstHour, gridArea: GridAreaCodeC, quantity: FirstQuantityFirstCorrection);
        var row2FirstCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, batchProcessType: DeltaTableProcessType.FirstCorrectionSettlement, time: secondHour, gridArea: GridAreaCodeC, quantity: SecondQuantityFirstCorrection);

        var row1SecondCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, batchProcessType: DeltaTableProcessType.SecondCorrectionSettlement, time: firstHour, gridArea: GridAreaCodeC, quantity: FirstQuantitySecondCorrection);
        var row2SecondCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, batchProcessType: DeltaTableProcessType.SecondCorrectionSettlement, time: secondHour, gridArea: GridAreaCodeC, quantity: SecondQuantitySecondCorrection);

        var row1ThirdCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, batchProcessType: DeltaTableProcessType.ThirdCorrectionSettlement, time: firstHour, gridArea: GridAreaCodeC, quantity: FirstQuantityThirdCorrection);
        var row2ThirdCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, batchProcessType: DeltaTableProcessType.ThirdCorrectionSettlement, time: secondHour, gridArea: GridAreaCodeC, quantity: SecondQuantityThirdCorrection);
        var row4ThirdCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, batchProcessType: DeltaTableProcessType.ThirdCorrectionSettlement, time: thirdHour, gridArea: GridAreaCodeC, quantity: FourthQuantityThirdCorrection);

        var row1SecondDay = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: secondDay, gridArea: GridAreaCodeC, quantity: FirstQuantity);
        var row1ThirdDay = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: thirdDay, gridArea: GridAreaCodeC, quantity: SecondQuantity);

        // mix up the order of the rows
        var rows = new List<IReadOnlyCollection<string>>
        {
            row1,
            row2,
            row3,
            row4,
            row5,
            row6,
            row7,
            row8,
            row9,
            row10,
            row1SecondDay,
            row1ThirdDay,
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
