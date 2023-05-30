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

using System.Net;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Application;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Common.Models;
using Energinet.DataHub.Wholesale.Components.DatabricksClient;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NodaTime;
using Xunit;
using Xunit.Categories;
using static Moq.Protected.ItExpr;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResultClient;

[UnitTest]
public class CalculationResultClientTests
{
    private readonly string[] _someGridAreas = { "123", "456", };

    private readonly DatabricksOptions _someDatabricksOptions = new()
    {
        DATABRICKS_WAREHOUSE_ID = "anyDatabricksId",
        DATABRICKS_WORKSPACE_URL = "https://anyDatabricksUrl",
        DATABRICKS_WORKSPACE_TOKEN = "myToken",
    };

    private readonly Instant _somePeriodStart = Instant.FromUtc(2021, 3, 1, 10, 15);
    private readonly Instant _somePeriodEnd = Instant.FromUtc(2021, 3, 31, 10, 15);
    private readonly DatabricksSqlResponse _cancelledDatabricksSqlResponse = DatabricksSqlResponse.CreateAsCancelled();
    private readonly DatabricksSqlResponse _pendingDatabricksSqlResponse = DatabricksSqlResponse.CreateAsPending();
    private readonly DatabricksSqlResponse _succeededDatabricksSqlResponse = DatabricksSqlResponse.CreateAsSucceeded(TableTestHelper.CreateTableForSettlementReport(3));
    private readonly DatabricksSqlResponse _succeededDatabricksSqlResponseWithZeroRows = DatabricksSqlResponse.CreateAsSucceeded(TableTestHelper.CreateTableForSettlementReport(0));
    private readonly DatabricksSqlResponse _failedDatabricksSqlResponse = DatabricksSqlResponse.CreateAsFailed();

    [Theory]
    [InlineAutoMoqData]
    public async Task GetSettlementReportResultAsync_WhenHttpStatusCodeNotOK_ThrowsDatabricksSqlException(
        [Frozen] Mock<IDatabricksSqlResponseParser> databricksSqlResponseParserMock,
        [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
        [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, });
        var httpClient = new HttpClient(mockMessageHandler.Object);

        var sut = new CalculationResults.Infrastructure.CalculationResultClient.SqlStatementClient(httpClient, mockOptions.Object, databricksSqlResponseParserMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<DatabricksSqlException>(() =>
            sut.GetSettlementReportResultAsync(_someGridAreas, ProcessType.BalanceFixing, _somePeriodStart, _somePeriodEnd, null));
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetSettlementReportResultAsync_WhenDatabricksReturnsFailed_ThrowsDatabricksSqlException(
        [Frozen] Mock<IDatabricksSqlResponseParser> databricksSqlResponseParserMock,
        [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
        [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });
        var httpClient = new HttpClient(mockMessageHandler.Object);
        databricksSqlResponseParserMock.Setup(p => p.Parse(It.IsAny<string>())).Returns(_failedDatabricksSqlResponse);

        var sut = new CalculationResults.Infrastructure.CalculationResultClient.SqlStatementClient(httpClient, mockOptions.Object, databricksSqlResponseParserMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<DatabricksSqlException>(() => sut.GetSettlementReportResultAsync(_someGridAreas, ProcessType.BalanceFixing, _somePeriodStart, _somePeriodEnd, null));
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetSettlementReportResultAsync_WhenDatabricksKeepsReturningPending_ThrowDatabricksSqlException(
        [Frozen] Mock<IDatabricksSqlResponseParser> databricksSqlResponseParserMock,
        [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
        [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, });
        var httpClient = new HttpClient(mockMessageHandler.Object);
        databricksSqlResponseParserMock.Setup(p => p.Parse(It.IsAny<string>())).Returns(_pendingDatabricksSqlResponse);

        var sut = new CalculationResults.Infrastructure.CalculationResultClient.SqlStatementClient(httpClient, mockOptions.Object, databricksSqlResponseParserMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<DatabricksSqlException>(() =>
            sut.GetSettlementReportResultAsync(_someGridAreas, ProcessType.BalanceFixing, _somePeriodStart, _somePeriodEnd, null));
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetSettlementReportResultAsync_WhenDatabricksReturnsSucceeded_ReturnsExpectedNumberOfRows(
        [Frozen] Mock<IDatabricksSqlResponseParser> databricksSqlResponseParserMock,
        [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
        [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, });
        var httpClient = new HttpClient(mockMessageHandler.Object);
        databricksSqlResponseParserMock.Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(_succeededDatabricksSqlResponse);

        var sut = new CalculationResults.Infrastructure.CalculationResultClient.SqlStatementClient(httpClient, mockOptions.Object, databricksSqlResponseParserMock.Object);

        // Act
        var actual = await sut.GetSettlementReportResultAsync(_someGridAreas, ProcessType.BalanceFixing, _somePeriodStart, _somePeriodEnd, null);

        // Assert
        actual.Count().Should().Be(_succeededDatabricksSqlResponse.Table!.RowCount);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task
        GetSettlementReportResultAsync_WhenDatabricksReturnsPendingAndThenSucceeded_ReturnsExpectedResponse(
            [Frozen] Mock<IDatabricksSqlResponseParser> databricksSqlResponseParserMock,
            [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
            [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, });
        var httpClient = new HttpClient(mockMessageHandler.Object);
        databricksSqlResponseParserMock.SetupSequence(p => p.Parse(It.IsAny<string>()))
            .Returns(_pendingDatabricksSqlResponse).Returns(_succeededDatabricksSqlResponse);

        var sut = new CalculationResults.Infrastructure.CalculationResultClient.SqlStatementClient(httpClient, mockOptions.Object, databricksSqlResponseParserMock.Object);

        // Act
        var actual = await sut.GetSettlementReportResultAsync(_someGridAreas, ProcessType.BalanceFixing, _somePeriodStart, _somePeriodEnd, null);

        // Assert
        actual.Count().Should().Be(_succeededDatabricksSqlResponse.Table!.RowCount);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task
        GetSettlementReportResultAsync_WhenDatabricksReturnsCancelledAndThenSucceeded_ReturnsExpectedResponse(
            [Frozen] Mock<IDatabricksSqlResponseParser> databricksSqlResponseParserMock,
            [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
            [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, });
        var httpClient = new HttpClient(mockMessageHandler.Object);
        databricksSqlResponseParserMock.SetupSequence(p => p.Parse(It.IsAny<string>()))
            .Returns(_cancelledDatabricksSqlResponse).Returns(_succeededDatabricksSqlResponse);

        var sut = new CalculationResults.Infrastructure.CalculationResultClient.SqlStatementClient(httpClient, mockOptions.Object, databricksSqlResponseParserMock.Object);

        // Act
        var actual = await sut.GetSettlementReportResultAsync(_someGridAreas, ProcessType.BalanceFixing, _somePeriodStart, _somePeriodEnd, null);

        // Assert
        actual.Count().Should().Be(_succeededDatabricksSqlResponse.Table!.RowCount);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetSettlementReportResultAsync_WhenSuccessfulResponse_ReturnsExpectedNumberOfRows(
        [Frozen] Mock<IDatabricksSqlResponseParser> databricksSqlResponseParserMock,
        [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
        [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var httpClient = new HttpClient(mockMessageHandler.Object);
        databricksSqlResponseParserMock.Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(_succeededDatabricksSqlResponse);

        var sut = new CalculationResults.Infrastructure.CalculationResultClient.SqlStatementClient(httpClient, mockOptions.Object, databricksSqlResponseParserMock.Object);

        // Act
        var actual = await sut.GetSettlementReportResultAsync(_someGridAreas, ProcessType.BalanceFixing, _somePeriodStart, _somePeriodEnd, null);

        // Assert
        actual.Count().Should().Be(_succeededDatabricksSqlResponse.Table!.RowCount);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetSettlementReportResultAsync_WhenHttpEndpointReturnsRealSampleData_ReturnsExpectedData(
        [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
        [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        const int expectedRowCount = 96;
        const decimal expectedFirstQuantity = 0.000m;
        const decimal expectedLastQuantity = 1.235m;
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = GetValidHttpResponseContent(), });
        var httpClient = new HttpClient(mockMessageHandler.Object);
        var sut = new CalculationResults.Infrastructure.CalculationResultClient.SqlStatementClient(httpClient, mockOptions.Object, new DatabricksSqlResponseParser()); // here we use the real parser

        // Act
        var actual = await sut.GetSettlementReportResultAsync(_someGridAreas, ProcessType.BalanceFixing, _somePeriodStart, _somePeriodEnd, null);

        // Assert
        var actualArray = actual as SettlementReportResultRow[] ?? actual.ToArray();
        actualArray.Length.Should().Be(expectedRowCount);
        actualArray.First().Quantity.Should().Be(expectedFirstQuantity);
        actualArray.Last().Quantity.Should().Be(expectedLastQuantity);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetSettlementReportResultAsync_WhenNoRelevantData_ReturnsZeroRows(
        [Frozen] Mock<IDatabricksSqlResponseParser> databricksSqlResponseParserMock,
        [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
        [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, });
        var httpClient = new HttpClient(mockMessageHandler.Object);
        databricksSqlResponseParserMock.Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(_succeededDatabricksSqlResponseWithZeroRows);
        var sut = new CalculationResults.Infrastructure.CalculationResultClient.SqlStatementClient(httpClient, mockOptions.Object, databricksSqlResponseParserMock.Object);

        // Act
        var actual = await sut.GetSettlementReportResultAsync(_someGridAreas, ProcessType.BalanceFixing, _somePeriodStart, _somePeriodEnd, null);

        // Assert
        actual.Count().Should().Be(0);
    }

    private static StringContent GetValidHttpResponseContent()
    {
        var stream = EmbeddedResources.GetStream("Infrastructure.CalculationResultClient.CalculationResult.json");
        using var reader = new StreamReader(stream);
        return new StringContent(reader.ReadToEnd());
    }
}
