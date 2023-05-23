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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using Energinet.DataHub.Wholesale.Components.DatabricksClient;
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
    private readonly string[] _gridAreas = { "123", "456", };
    private readonly DatabricksOptions _anyDatabricksOptions = new() { DATABRICKS_WAREHOUSE_ID = "anyDatabricksId", DATABRICKS_WORKSPACE_URL = "https://anyDatabricksUrl", DATABRICKS_WORKSPACE_TOKEN = "myToken" };
    private readonly Instant _validPeriodStart = Instant.FromUtc(2021, 3, 1, 10, 15);
    private readonly Instant _validPeriodEnd = Instant.FromUtc(2021, 3, 31, 10, 15);

    [Theory]
    [InlineAutoMoqData]
    public async Task GetSettlementReportResultAsync_XXXXXXXXXXXXXXX(
        [Frozen]Mock<IDatabricksSqlResponseParser> databricksSqlResponseParserMock,
        [Frozen]Mock<HttpMessageHandler> mockMessageHandler,
        [Frozen]Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        mockOptions.Setup(o => o.Value).Returns(_anyDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = GetValidHttpResponseContent(),
            });
        var httpClient = new HttpClient(mockMessageHandler.Object);
        // var dbResponse = new DatabricksSqlResponse("PENDING", null);
        // databricksSqlResponseParserMock.Setup(p => p.Parse(It.IsAny<string>())).Returns(dbResponse)
        var sut = new CalculationResults.Infrastructure.CalculationResultClient.CalculationResultClient(httpClient, mockOptions.Object, databricksSqlResponseParserMock.Object);

        // Act
        var actual = await sut.GetSettlementReportResultAsync(_gridAreas, ProcessType.BalanceFixing, _validPeriodStart, _validPeriodEnd, null);

        // Assert
    }

    private static StringContent GetValidHttpResponseContent()
    {
        var stream = EmbeddedResources.GetStream("Infrastructure.CalculationResultClient.CalculationResult.json");
        using var reader = new StreamReader(stream);
        return new StringContent(reader.ReadToEnd());
    }
}
