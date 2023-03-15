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
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Fixture.WebApi;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.WebApi;
using Energinet.DataHub.Wholesale.WebApi.V3;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi.V3;

public class ProcessStepBalanceResponsiblePartyTests : WebApiTestBase
{
    public ProcessStepBalanceResponsiblePartyTests(
        WholesaleWebApiFixture wholesaleWebApiFixture,
        WebApiFactory factory,
        ITestOutputHelper testOutputHelper)
        : base(wholesaleWebApiFixture, factory, testOutputHelper)
    {
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task HTTP_GET_V3_ReturnsHttpStatusCodeOkAtExpectedUrl(
        Guid batchId,
        string gridAreaCode,
        TimeSeriesType timeSeriesType)
    {
        // Arrange
        var expectedUrl = $"/v3/batches/{batchId}/processes/{gridAreaCode}/time-series-types/{timeSeriesType}/balance-responsible-parties";
        var expectedHttpStatusCode = HttpStatusCode.OK;

        // Act
        var actualContent = await Client.GetAsync(expectedUrl);

        // Assert
        actualContent.StatusCode.Should().Be(expectedHttpStatusCode);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task HTTP_GET_V3_ReturnsExpectedActorInJson(
        Mock<IProcessStepApplicationService> applicationServiceMock,
        ProcessStepActorsRequest request,
        WholesaleActorDto expectedActor)
    {
        // Arrange
        var url = $"/v3/batches/{request.BatchId}/processes/{request.GridAreaCode}/time-series-types/{request.Type}/balance-responsible-parties";

        applicationServiceMock
            .Setup(service => service.GetBalanceResponsiblePartiesAsync(request.BatchId, request.GridAreaCode, request.Type)).ReturnsAsync(() => new[] { expectedActor });
        Factory.ProcessStepApplicationServiceMock = applicationServiceMock;

        // Act
        var actualContent = await Client.GetAsync(url);

        // Assert
        var actualActors = await actualContent.Content.ReadFromJsonAsync<List<ActorDto>>();
        actualActors!.Single().Should().BeEquivalentTo(expectedActor);
    }
}
