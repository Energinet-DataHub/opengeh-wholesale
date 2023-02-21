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
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Fixture.WebApi;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.WebApi;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi.V3;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi.V2;

public class SettlementReportControllerTests : WebApiTestBase
{
    public SettlementReportControllerTests(
        WholesaleWebApiFixture wholesaleWebApiFixture,
        WebApiFactory factory,
        ITestOutputHelper testOutputHelper)
        : base(wholesaleWebApiFixture, factory, testOutputHelper)
    {
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
        Factory.SettlementReportApplicationServiceMock = mock;

        // Act
        var actualContent = await Client.GetStringAsync($"/v2.3/settlementreport?batchId={batchId.ToString()}");

        // Assert
        actualContent.Should().Be(settlementReportContent);
    }
}
