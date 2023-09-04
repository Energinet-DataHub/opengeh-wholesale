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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Events.Application.Options;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Energinet.DataHub.Wholesale.Events.Application.Workers;
using Energinet.DataHub.Wholesale.Events.IntegrationTests.Fixture;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.IntegrationTests.Infrastructure.AggregatedTimeSeriesRequests;

public class AggregatedTimeSeriesRequestsTests : IClassFixture<ServiceBusSenderFixture>
{
    private readonly ServiceBusSenderFixture _sender;

    public AggregatedTimeSeriesRequestsTests(ServiceBusSenderFixture fixture)
    {
        _sender = fixture;
    }


    [Fact]
    public async Task Can_Receive_aggregated_time_series_request_service_bus_message()
    {
        var handlerMock = new Mock<AggregatedTimeSeriesRequestHandler>();
        var loggerMock = new Mock<ILogger<AggregatedTimeSeriesRequestHandler>>();

        var sut = new AggregatedTimeSeriesServiceBusWorker(
            handlerMock.Object,
            loggerMock.Object,
            "HEJ CONNECTION STRING");

        await sut.StartAsync(CancellationToken.None).ConfigureAwait(false);

        await _sender.PublishAsync("kenneth", new byte[10]);

        handlerMock.Verify(handler => handler.ProcessAsync(CancellationToken.None), Times.Once);
    }
}
