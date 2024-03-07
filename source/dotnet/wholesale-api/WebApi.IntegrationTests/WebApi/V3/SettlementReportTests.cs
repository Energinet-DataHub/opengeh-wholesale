﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Fixture.WebApi;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.WebApi;
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
    public async Task HTTP_GET_V3_Download_ReturnsExpectedContent(
        Mock<ISettlementReportClient> settlementReportApplicationService)
    {
        // Arrange
        const string gridAreaCode = "567";
        const string calculationType = "BalanceFixing";
        var periodStart = DateTime.Parse("2021-01-01T00:00:00Z").ToUniversalTime();
        var periodEnd = DateTime.Parse("2021-06-15T00:00:00Z").ToUniversalTime();

        var url = "/v3/SettlementReport/Download"
                  + $"?gridAreaCodes={gridAreaCode}"
                  + $"&calculationType={calculationType}"
                  + $"&periodStart={periodStart:O}"
                  + $"&periodEnd={periodEnd:O}";

        const string expectedMockedContent = "0305C8A0-5E42-4174-85DE-B7737E8C66C4";

        settlementReportApplicationService
            .Setup(service => service.CreateCompressedSettlementReportAsync(
                It.IsAny<Func<Stream>>(),
                new[] { gridAreaCode },
                CalculationType.BalanceFixing,
                periodStart,
                periodEnd,
                null,
                null))
            .Returns<Func<Stream>, string[], CalculationType, DateTimeOffset, DateTimeOffset, string?, string?>((openStream, _, _, _, _, _, _) =>
            {
                openStream().Write(Encoding.UTF8.GetBytes(expectedMockedContent));
                return Task.CompletedTask;
            });

        Factory.SettlementReportApplicationServiceMock = settlementReportApplicationService;

        // Act
        var actual = await Client.GetAsync(url);

        // Assert
        Assert.Equal(expectedMockedContent, await actual.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task HTTP_GET_V3_Download_DkLanguage_ReturnsExpectedContent(
        Mock<ISettlementReportClient> settlementReportApplicationService)
    {
        // Arrange
        const string gridAreaCode = "567";
        const string calculationType = "BalanceFixing";
        const string language = "da-DK";
        var periodStart = DateTime.Parse("2021-01-01T00:00:00Z").ToUniversalTime();
        var periodEnd = DateTime.Parse("2021-06-15T00:00:00Z").ToUniversalTime();

        var url = "/v3/SettlementReport/Download"
                  + $"?gridAreaCodes={gridAreaCode}"
                  + $"&calculationType={calculationType}"
                  + $"&periodStart={periodStart:O}"
                  + $"&periodEnd={periodEnd:O}"
                  + $"&csvFormatLocale={language}";

        const string expectedMockedContent = "0305C8A0-5E42-4174-85DE-B7737E8C66C4";

        settlementReportApplicationService
            .Setup(service => service.CreateCompressedSettlementReportAsync(
                It.IsAny<Func<Stream>>(),
                new[] { gridAreaCode },
                CalculationType.BalanceFixing,
                periodStart,
                periodEnd,
                null,
                language))
            .Returns<Func<Stream>, string[], CalculationType, DateTimeOffset, DateTimeOffset, string?, string?>((openStream, _, _, _, _, _, _) =>
            {
                openStream().Write(Encoding.UTF8.GetBytes(expectedMockedContent));
                return Task.CompletedTask;
            });

        Factory.SettlementReportApplicationServiceMock = settlementReportApplicationService;

        // Act
        var actual = await Client.GetAsync(url);

        // Assert
        Assert.Equal(expectedMockedContent, await actual.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task HTTP_GET_V3_Download_ReturnsCorrectHeaders(
        Mock<ISettlementReportClient> settlementReportApplicationService)
    {
        // Arrange
        const string gridAreaCode = "567";
        const string calculationType = "BalanceFixing";
        var periodStart = DateTime.Parse("2021-01-01T00:00:00Z").ToUniversalTime();
        var periodEnd = DateTime.Parse("2021-06-15T00:00:00Z").ToUniversalTime();

        var url = "/v3/SettlementReport/Download"
                  + $"?gridAreaCodes={gridAreaCode}"
                  + $"&calculationType={calculationType}"
                  + $"&periodStart={periodStart:O}"
                  + $"&periodEnd={periodEnd:O}";

        var expectedFileName = $"Result_{gridAreaCode}_{periodStart:dd-MM-yyyy}_{periodEnd:dd-MM-yyyy}_D04.zip";

        settlementReportApplicationService
            .Setup(service => service.CreateCompressedSettlementReportAsync(
                It.IsAny<Func<Stream>>(),
                new[] { gridAreaCode },
                CalculationType.BalanceFixing,
                periodStart,
                periodEnd,
                null,
                null))
            .Returns<Func<Stream>, string[], CalculationType, DateTimeOffset, DateTimeOffset, string?, string?>((openStream, _, _, _, _, _, _) =>
            {
                openStream();
                return Task.CompletedTask;
            });

        Factory.SettlementReportApplicationServiceMock = settlementReportApplicationService;

        // Act
        var actual = await Client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
        Assert.Equal("application/zip", actual.Content.Headers.ContentType?.MediaType);
        Assert.Equal(expectedFileName, actual.Content.Headers.ContentDisposition?.FileName);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task HTTP_GET_V3_Download_WithAggregationCalculationType_ReturnsBadRequest(
        Mock<ISettlementReportClient> settlementReportApplicationService)
    {
        // Arrange
        const string gridAreaCode = "567";
        const string calculationType = "Aggregation";
        var periodStart = DateTime.Parse("2021-01-01T00:00:00Z").ToUniversalTime();
        var periodEnd = DateTime.Parse("2021-06-15T00:00:00Z").ToUniversalTime();

        var url = "/v3/SettlementReport/Download"
                  + $"?gridAreaCodes={gridAreaCode}"
                  + $"&calculationType={calculationType}"
                  + $"&periodStart={periodStart:O}"
                  + $"&periodEnd={periodEnd:O}";

        settlementReportApplicationService
            .Setup(service => service.CreateCompressedSettlementReportAsync(
                It.IsAny<Func<Stream>>(),
                new[] { gridAreaCode },
                CalculationType.Aggregation,
                periodStart,
                periodEnd,
                null,
                null))
            .Returns<Func<Stream>, string[], CalculationType, DateTimeOffset, DateTimeOffset, string?, string?>((_, _, _, _, _, _, _) =>
                throw new BusinessValidationException("Tested Validation Exception"));

        Factory.SettlementReportApplicationServiceMock = settlementReportApplicationService;

        // Act
        var actual = await Client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
    }
}
