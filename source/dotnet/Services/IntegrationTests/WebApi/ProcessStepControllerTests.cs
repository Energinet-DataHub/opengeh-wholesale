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
using Energinet.DataHub.Wholesale.Application.ProcessStep;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixtures.WebApi;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.WebApi;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.WebApi;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.WebApi;

[Collection(nameof(WholesaleWebApiCollectionFixture))]
public class ProcessStepControllerTests :
    WebApiTestBase<WholesaleWebApiFixture>,
    IClassFixture<WholesaleWebApiFixture>,
    IClassFixture<WebApiFactory>,
    IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebApiFactory _factory;

    public ProcessStepControllerTests(
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
    [InlineAutoMoqData]
    public async Task ExpectedUrl_Should_ReturnOk(
        Mock<IProcessStepApplicationService> mock,
        ProcessStepActorsRequest request,
        WholesaleActorDto wholesaleActorDto)
    {
        // Arrange
        mock
            .Setup(service => service.GetActorsAsync(request))
            .ReturnsAsync(new[] { wholesaleActorDto });
        _factory.ProcessStepApplicationServiceMock = mock;

        // Act
        const string expectedUrl = "/v2.3/processstepresult";
        var actualContent = await _client.PostAsJsonAsync(expectedUrl, request);

        // Assert
        actualContent.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
