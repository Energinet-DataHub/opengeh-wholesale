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
using FluentAssertions;
using Microsoft.OpenApi.Readers;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi.V3;

public class OpenApiSpecificationTests : WebApiTestBase
{
    private const string OpenApiSpecUrl = "/swagger/v3/swagger.json";

    public OpenApiSpecificationTests(
        WholesaleWebApiFixture wholesaleWebApiFixture,
        WebApiFactory factory,
        ITestOutputHelper testOutputHelper)
        : base(wholesaleWebApiFixture, factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task Given_WebAPI_When_GettingOpenApiSpec_Then_ReturnsSpecificationInJson()
    {
        // Act
        var actualResponse = await Client.GetAsync(OpenApiSpecUrl);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var actualContent = await actualResponse.Content.ReadAsStringAsync();
        var actualContentJObject = JObject.Parse(actualContent);
        actualContentJObject.Should().NotBeNull();
    }

    [Fact]
    public async Task All_Endpoints_Have_Correct_MediaType()
    {
        // Act
        var operationToContentTypeLookup =
            new Dictionary<string, string> { { "Get /v3/SettlementReport", "application/zip" } };
        var stream = await Client.GetStreamAsync(OpenApiSpecUrl);

        // Assert
        var openApiDocument = new OpenApiStreamReader().Read(stream, out _);
        foreach (var path in openApiDocument.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                foreach (var response in operation.Value.Responses)
                {
                    foreach (var content in response.Value.Content)
                    {
                        var key = $"{operation.Key} {path.Key}";

                        content.Key.Should().Be(
                            operationToContentTypeLookup.TryGetValue(key, out var contentType)
                                ? contentType
                                : "application/json");
                    }
                }
            }
        }
    }
}
