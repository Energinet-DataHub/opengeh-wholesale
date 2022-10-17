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
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.WebApi;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.WebApi.V1;

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
        var periodStart = DateTimeOffset.Now;
        var periodEnd = periodStart.AddHours(1);
        var batchRequest = new BatchRequestDto(
            WholesaleProcessType.BalanceFixing,
            new List<string> { "805" },
            periodStart,
            periodEnd);

        // Act
        var response = await _client.PostAsJsonAsync(BaseUrl, batchRequest, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SearchAsync_WhenCalled_AlwaysReturnsOk()
    {
        // Arrange
        var minExecutionTime = new DateTimeOffset(
            2022,
            01,
            02,
            1,
            2,
            3,
            50,
            TimeSpan.Zero);
        var maxExecutionTime = minExecutionTime + TimeSpan.FromMinutes(33);
        var batchSearchDto = new BatchSearchDto(minExecutionTime, maxExecutionTime);

        // Act
        const string searchUrl = BaseUrl + "/Search";
        var response = await _client.PostAsJsonAsync(searchUrl, batchSearchDto, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
