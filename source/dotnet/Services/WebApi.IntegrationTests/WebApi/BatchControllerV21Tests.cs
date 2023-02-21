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
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.Batches.Model;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Fixture.WebApi;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.WebApi;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi;

[Collection(nameof(WholesaleWebApiCollectionFixture))]
public class BatchControllerV21Tests :
    WebApiTestBase<WholesaleWebApiFixture>,
    IClassFixture<WholesaleWebApiFixture>,
    IClassFixture<WebApiFactory>,
    IAsyncLifetime
{
    private readonly WebApiFactory _factory;
    private readonly HttpClient _client;

    public BatchControllerV21Tests(
        WholesaleWebApiFixture wholesaleWebApiFixture,
        WebApiFactory factory,
        ITestOutputHelper testOutputHelper)
        : base(wholesaleWebApiFixture, testOutputHelper)
    {
        _factory = factory;
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

    [Theory]
    [AutoMoqData]
    public async Task SearchAsync_WhenCalled_AlwaysReturnsOk(
        Mock<IBatchApplicationService> mock,
        IEnumerable<BatchDto> batchDtos)
    {
        // Arrange
        var minExecutionTime = new DateTimeOffset(2022, 01, 02, 1, 2, 3, 50, TimeSpan.Zero);
        var maxExecutionTime = minExecutionTime + TimeSpan.FromMinutes(33);
        var periodStart = DateTimeOffset.Now.Date.AddDays(-3);
        var periodEnd = DateTimeOffset.Now.Date.AddDays(3);
        var batchSearchDto = new BatchSearchDtoV2(minExecutionTime, maxExecutionTime, periodStart, periodEnd, BatchState.Completed);
        mock.Setup(service => service.SearchAsync(batchSearchDto))
            .ReturnsAsync(batchDtos);
        _factory.BatchApplicationServiceMock = mock;

        // Act
        var response = await _client.PostAsJsonAsync("/v2.1/batch/Search", batchSearchDto, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
