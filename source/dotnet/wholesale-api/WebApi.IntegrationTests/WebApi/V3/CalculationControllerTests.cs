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
using System.Net.Http.Json;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Fixture.WebApi;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.WebApi;
using FluentAssertions;
using Moq;
using Test.Core;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi.V3;

public class CalculationControllerTests : WebApiTestBase
{
    public CalculationControllerTests(
        WholesaleWebApiFixture wholesaleWebApiFixture,
        WebApiFactory factory,
        ITestOutputHelper testOutputHelper)
        : base(wholesaleWebApiFixture, factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task HTTP_POST_V3_ReturnsHttpStatusCodeOkAtExpectedUrl()
    {
        // Arrange
        var expectedUrl = "/v3/calculations";
        var expectedHttpStatusCode = HttpStatusCode.OK;
        var calculationRequestDto = CreateCalculationRequestDto();

        // Act
        var actualContent = await Client.PostAsJsonAsync(expectedUrl, calculationRequestDto, CancellationToken.None);

        // Assert
        actualContent.StatusCode.Should().Be(expectedHttpStatusCode);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task HTTP_GET_V3_ReturnsHttpStatusCodeOkAtExpectedUrl(
        Mock<ICalculationsClient> mock,
        CalculationDto calculationDto)
    {
        // Arrange
        mock.Setup(service => service.GetAsync(calculationDto.CalculationId))
            .ReturnsAsync(calculationDto);
        Factory.CalculationsClientMock = mock;

        // Act
        var response = await Client.GetAsync($"/v3/calculations/{calculationDto.CalculationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HTTP_GET_V3_SearchReturnsHttpStatusCodeOkAtExpectedUrl()
    {
        // Arrange + Act
        var response = await Client.GetAsync("/v3/calculations", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static CalculationRequestDto CreateCalculationRequestDto()
    {
        var (PeriodStart, PeriodEnd, DateTimeZone) = Periods.January_EuropeCopenhagen;
        var calculationRequest = new CalculationRequestDto(
            CalculationType.BalanceFixing,
            new List<string> { "805" },
            PeriodStart,
            PeriodEnd);
        return calculationRequest;
    }
}
