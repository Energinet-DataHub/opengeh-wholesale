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
using System.Text;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application.SettlementReport;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Fixture.WebApi;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.WebApi;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi.V3;

public sealed class SettlementReportTests : WebApiTestBase
{
    public SettlementReportTests(
        WholesaleWebApiFixture wholesaleWebApiFixture,
        WebApiFactory factory,
        ITestOutputHelper testOutputHelper)
        : base(wholesaleWebApiFixture, factory, testOutputHelper)
    {
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task HTTP_GET_V3_ReturnsHttpStatusCodeOkAtExpectedUrl(
        Mock<ISettlementReportApplicationService> settlementReportApplicationService)
    {
        // arrange
        const string gridAreaCode = "001";
        var batchId = Guid.NewGuid();

        var url = $"/v3/SettlementReport?batchId={batchId}&gridAreaCode={gridAreaCode}";

        const HttpStatusCode expectedHttpStatusCode = HttpStatusCode.OK;

        settlementReportApplicationService
            .Setup(service => service.GetSettlementReportAsync(batchId, gridAreaCode, Stream.Null));

        Factory.SettlementReportApplicationServiceMock = settlementReportApplicationService;

        // act
        var actual = await Client.GetAsync(url);

        // assert
        actual.StatusCode.Should().Be(expectedHttpStatusCode);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task HTTP_GET_V3_ReturnsExpectedContent(
        Mock<ISettlementReportApplicationService> settlementReportApplicationService)
    {
        // arrange
        const string gridAreaCode = "001";
        var batchId = Guid.NewGuid();

        var url = $"/v3/SettlementReport?batchId={batchId}&gridAreaCode={gridAreaCode}";

        const string expectedContent = "F33B866D-D97A-42B3-9DFF-5BD1EC28885A";

        settlementReportApplicationService
            .Setup(service => service.GetSettlementReportAsync(batchId, gridAreaCode, It.IsAny<Stream>()))
            .Callback<Guid, string, Stream>((_, _, outputStream) => outputStream.Write(Encoding.UTF8.GetBytes(expectedContent)));

        Factory.SettlementReportApplicationServiceMock = settlementReportApplicationService;

        // act
        var actual = await Client.GetAsync(url);

        // assert
        var actualStream = await actual.Content.ReadAsStreamAsync();
        var actualBytes = new byte[actualStream.Length];
        _ = await actualStream.ReadAsync(actualBytes);
        var actualContent = Encoding.UTF8.GetString(actualBytes);

        actualContent.Should().Be(expectedContent);
    }
}
