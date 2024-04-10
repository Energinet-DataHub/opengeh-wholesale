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
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Fixture.WebApi;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.WebApi;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi;

public class SwaggerTests : WebApiTestBase
{
    public SwaggerTests(
        WholesaleWebApiFixture wholesaleWebApiFixture,
        WebApiFactory factory,
        ITestOutputHelper testOutputHelper)
        : base(wholesaleWebApiFixture, factory, testOutputHelper)
    {
    }

    [Theory]
    [InlineData("v3")]
    public async Task UrlIsApiVersionSwaggerJson_WhenGet_ResponseIsOKAndContainsJsonAndOpenAPIv3(string apiVersion)
    {
        // Arrange
        var url = $"swagger/{apiVersion}/swagger.json";

        // Act
        var actualResponse = await Client.GetAsync(url);

        // Assert
        using var assertionScope = new AssertionScope();
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Contain("\"openapi\": \"3.");
    }

    [Fact]
    public async Task UrlIsSwaggerUIDefault_WhenGet_ResponseIsOKAndContainsHtml()
    {
        // Arrange
        var url = "swagger";

        // Act
        var actualResponse = await Client.GetAsync(url);

        // Assert
        using var assertionScope = new AssertionScope();
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponse.Content.Headers.ContentType!.MediaType.Should().Be("text/html");
    }
}
