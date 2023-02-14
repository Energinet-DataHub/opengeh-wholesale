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
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Fixture.WebApi;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.WebApi;
using FluentAssertions;
using Moq;
using Test.Core;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi.V3;

public class ProcessStepResultTests : WebApiTestBase
{
    public ProcessStepResultTests(
        WholesaleWebApiFixture wholesaleWebApiFixture,
        WebApiFactory factory,
        ITestOutputHelper testOutputHelper)
        : base(wholesaleWebApiFixture, factory, testOutputHelper)
    {
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task HTTP_GET_V3_ReturnsHttpStatusCodeOkAtExpectedUrl(
        Mock<IProcessStepApplicationService> processStepApplicationServiceMock,
        Mock<IBatchApplicationService> batchApplicationServiceMock,
        Contracts.ProcessStepResultDto result,
        BatchDto batchDto,
        ProcessStepActorsRequest request)
    {
        // Arrange
        request.SetPrivateProperty(r => r.Type, TimeSeriesType.Production);
        result.SetPrivateProperty(r => r.TimeSeriesPoints, new TimeSeriesPointDto[] { new(DateTimeOffset.Now, decimal.One, "A04") });
        processStepApplicationServiceMock
            .Setup(service => service.GetResultAsync(request.BatchId, request.GridAreaCode, TimeSeriesType.Production, "grid_area"))
            .ReturnsAsync(() => result);
        batchApplicationServiceMock.Setup(service => service.GetAsync(request.BatchId)).ReturnsAsync(batchDto);
        Factory.ProcessStepApplicationServiceMock = processStepApplicationServiceMock;
        Factory.BatchApplicationServiceMock = batchApplicationServiceMock;

        var url = $"/v3/batches/{request.BatchId}/processes/{request.GridAreaCode}/time-series-types/{request.Type}";
        var expectedHttpStatusCode = HttpStatusCode.OK;

        // Act
        var actualContent = await Client.GetAsync(url);

        // Assert
        actualContent.StatusCode.Should().Be(expectedHttpStatusCode);
    }
}
