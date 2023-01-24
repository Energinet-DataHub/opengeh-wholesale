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

using System.Text;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application.SettlementReport;
using Energinet.DataHub.Wholesale.Application.SettlementReport.Model;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.WebApi;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.WebApi;

[Collection(nameof(WholesaleWebApiCollectionFixture))]
public class SettlementReportControllerTests :
    WebApiTestBase<WholesaleWebApiFixture>,
    IClassFixture<WholesaleWebApiFixture>,
    IClassFixture<WebApiFactory>,
    IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebApiFactory _factory;

    public SettlementReportControllerTests(
        WholesaleWebApiFixture wholesaleWebApiFixture,
        WebApiFactory factory,
        ITestOutputHelper testOutputHelper)
        : base(wholesaleWebApiFixture, testOutputHelper)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_ReturnsSettlementReport(
        Mock<ISettlementReportApplicationService> mock,
        string settlementReportContent)
    {
        // Arrange
        var batchId = Guid.NewGuid();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(settlementReportContent));
        var settlementReport = new SettlementReportDto(stream);
        mock
            .Setup(service => service.GetSettlementReportAsync(batchId))
            .ReturnsAsync(settlementReport);
        _factory.SettlementReportApplicationServiceMock = mock;

        // Act
        var actualContent = await _client.GetStringAsync($"/v2.3/settlementreport?batchId={batchId.ToString()}");

        // Assert
        actualContent.Should().Be(settlementReportContent);
    }
}
