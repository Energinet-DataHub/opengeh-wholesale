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

    [Theory]
    [InlineAutoMoqData]
    public async Task Can_Receive_aggregated_time_series_request_service_bus_message(
        Mock<IAggregatedTimeSeriesRequestHandler> handlerMock,
        Mock<ILogger<AggregatedTimeSeriesRequestHandler>> loggerMock)
    {
        var sut = new AggregatedTimeSeriesServiceBusWorker(
            handlerMock.Object,
            loggerMock.Object,
            _sender.ServiceBusOptions);

        await sut.StartAsync(CancellationToken.None).ConfigureAwait(false);

        await _sender.PublishAsync("EventName", new byte[10]);
        // time to await service bus to notify handler about a new message.
        Thread.Sleep(1000);
        handlerMock.Verify(handler => handler.ProcessAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
