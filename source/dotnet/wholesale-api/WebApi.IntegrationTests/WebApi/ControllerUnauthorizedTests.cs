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
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Fixture.WebApi;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.WebApi;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi;

public class ControllerUnauthorizedTests :
    WebApiTestBase<WholesaleWebApiFixture>,
    IClassFixture<WholesaleWebApiFixture>,
    IClassFixture<WebApiFactory>
{
    private readonly WebApiFactory _factory;

    public ControllerUnauthorizedTests(
        WholesaleWebApiFixture wholesaleWebApiFixture,
        WebApiFactory factory,
        ITestOutputHelper testOutputHelper)
        : base(wholesaleWebApiFixture, testOutputHelper)
    {
        _factory = factory;
        _factory.ReenableAuthentication();
    }

    [Fact]
    public async Task Endpoint_WithoutAccess_ReturnsUnauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/v3/calculations");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
