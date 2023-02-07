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
using Energinet.DataHub.Wholesale.IntegrationTests.Fixtures.TestCommon.Fixture.WebApi;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixtures.WebApi;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.WebApi;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.WebApi;

[Collection(nameof(WholesaleWebApiCollectionFixture))]
public class SwaggerTests :
    WebApiTestBase<WholesaleWebApiFixture>,
    IClassFixture<WholesaleWebApiFixture>,
    IClassFixture<WebApiFactory>,
    IAsyncLifetime
{
    private readonly HttpClient _client;

    public SwaggerTests(
        WholesaleWebApiFixture wholesaleWebApiFixture,
        WebApiFactory factory,
        ITestOutputHelper testOutputHelper)
        : base(wholesaleWebApiFixture, testOutputHelper)
    {
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

    [Fact]
    public async Task Given_WebAPI_When_GettingOpenApiSpec_Then_ReturnsSpecificationInJson()
    {
        // Arrange
        const string expectedOpenApiSpecUrl = "/swagger/v1/swagger.json";

        // Act
        var actualResponse = await _client.GetAsync(expectedOpenApiSpecUrl);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var actualContent = await actualResponse.Content.ReadAsStringAsync();
        var actualContentJObject = JObject.Parse(actualContent);
        actualContentJObject.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_WebAPI_When_GettingSwaggerUi_Then_ReturnsHtmlPage()
    {
        // Act
        var actualResponse = await _client.GetAsync("/swagger/index.html");

        // Assert
        actualResponse.Content.Headers.ContentType!.ToString().Should().StartWith("text/html");
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
