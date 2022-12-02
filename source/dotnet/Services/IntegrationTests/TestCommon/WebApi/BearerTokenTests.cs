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
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.WebApi;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.WebApi;

[Collection(nameof(BearerTokenTests))]
public class BearerTokenTests :
    WebApiTestBase<WholesaleWebApiFixture>,
    IClassFixture<WholesaleWebApiFixture>,
    IClassFixture<WebApiFactory>
{
    private const string BaseUrl = "/v2/batch";
    private const string JwtBearerHttpHeader = "Authorization";
    private const string JwtBearerToken = "Bearer eyZZZ";

    private readonly WebApiFactory _factory;

    public BearerTokenTests(
        WholesaleWebApiFixture wholesaleWebApiFixture,
        WebApiFactory factory,
        ITestOutputHelper testOutputHelper)
        : base(wholesaleWebApiFixture, testOutputHelper)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Request_WhenMissingBearerToken_Returns401Unauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Remove(JwtBearerHttpHeader);

        // Act
        var response = await client.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Request_WhenInvalidBearerToken_Returns401Unauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(JwtBearerHttpHeader, JwtBearerToken);

        // Act
        var response = await client.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
