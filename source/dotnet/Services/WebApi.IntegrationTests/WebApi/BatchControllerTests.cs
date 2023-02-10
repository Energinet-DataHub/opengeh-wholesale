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
using Energinet.DataHub.Wholesale.Application.ProcessStep;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixtures.TestCommon.Fixture.WebApi;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixtures.TestHelpers;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixtures.WebApi;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.WebApi;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.WebApi;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.WebApi;

[Collection(nameof(WholesaleWebApiCollectionFixture))]
public class BatchControllerTests :
    WebApiTestBase<WholesaleWebApiFixture>,
    IClassFixture<WholesaleWebApiFixture>,
    IClassFixture<WebApiFactory>,
    IAsyncLifetime
{
    private readonly WebApiFactory _factory;
    private readonly HttpClient _client;

    public BatchControllerTests(
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
    public async Task CreateAsync_WhenCalled_AlwaysReturnsOk(
        Mock<IBatchApplicationService> mock,
        Guid batchId)
    {
        // Arrange
        var batchRequest = CreateBatchRequestDto();
        mock.Setup(service => service.CreateAsync(batchRequest))
            .ReturnsAsync(batchId);
        _factory.BatchApplicationServiceMock = mock;

        // Act
        var actual = await _client.PostAsJsonAsync("/v2/batch", batchRequest, CancellationToken.None);

        // Assert
        actual.StatusCode.Should().Be(HttpStatusCode.OK);
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
        var batchSearchDto = new BatchSearchDto(minExecutionTime, maxExecutionTime);
        mock.Setup(service => service.SearchAsync(batchSearchDto))
            .ReturnsAsync(batchDtos);
        _factory.BatchApplicationServiceMock = mock;

        // Act
        var response = await _client.PostAsJsonAsync("/v2/batch/Search", batchSearchDto, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [AutoMoqData]
    public async Task GetAsync_WhenCalled_ReturnsOk(
        Mock<IBatchApplicationService> mock,
        BatchDto batchDto)
    {
        // Arrange
        mock.Setup(service => service.GetAsync(batchDto.BatchId))
            .ReturnsAsync(batchDto);
        _factory.BatchApplicationServiceMock = mock;

        // Act
        var response = await _client.GetAsync($"/v2/batch?batchId={batchDto.BatchId.ToString()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("2023-01-31T23:00Z")]
    [InlineData("2023-01-31T22:59:59Z")]
    [InlineData("2023-01-31T22:59:59.9999999Z")]
    [InlineData("2023-01-31")]
    public async Task CreateAsync_WhenCalledWithInvalidPeriodEnd_ReturnsBadRequest(string periodEndString)
    {
        // Arrange
        const string baseUrl = "/v2/batch";
        var periodStart = DateTimeOffset.Parse("2022-12-31T23:00:00Z");
        var periodEnd = DateTimeOffset.Parse(periodEndString);
        var batchRequest = new BatchRequestDto(
            ProcessType.BalanceFixing,
            new List<string> { "805" },
            periodStart,
            periodEnd);
        // Act
        var response = await _client.PostAsJsonAsync(baseUrl, batchRequest, CancellationToken.None);
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static BatchRequestDto CreateBatchRequestDto()
    {
        var period = Periods.January_EuropeCopenhagen_1ms;
        var batchRequest = new BatchRequestDto(
            ProcessType.BalanceFixing,
            new List<string> { "805" },
            period.PeriodStart,
            period.PeriodEnd);
        return batchRequest;
    }
}
