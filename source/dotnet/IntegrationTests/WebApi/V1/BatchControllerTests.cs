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
using System.Text.Json;
using Energinet.DataHub.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.IntegrationTests.Core.Fixtures.WebApi;
using Energinet.DataHub.Wholesale.IntegrationTests.Core.TestCommon.WebApi;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.IntegrationTests.WebApi.V1;

[IntegrationTest]
[Collection(nameof(WholesaleWebApiCollectionFixture))]
public class BatchControllerTests :
    WebApiTestBase<WholesaleWebApiFixture>,
    IClassFixture<WholesaleWebApiFixture>,
    IClassFixture<WebApiFactory>,
    IAsyncLifetime
{
    private const string BaseUrl = "/v1/batch";

    private readonly HttpClient _client;

    public BatchControllerTests(
        WholesaleWebApiFixture wholesaleWebApiFixture,
        WebApiFactory factory,
        ITestOutputHelper testOutputHelper)
        : base(wholesaleWebApiFixture, testOutputHelper)
    {
        _client = factory.CreateClient();
        factory.ReconfigureJwtTokenValidatorMock(isValid: true);
    }

    public Task InitializeAsync()
    {
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer xxx");
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateAsync_WhenCalled_AlwaysReturnsOk()
    {
        // Arrange
        var batchRequest = new BatchRequestDto
        {
            ProcessType = WholesaleProcessType.BalanceFixing,
            GridAreaCodes = new List<string> { "805" },
        };
        var body = new StringContent(JsonSerializer.Serialize(batchRequest));

        // Act
        var response = await _client.PostAsync(BaseUrl, body, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
