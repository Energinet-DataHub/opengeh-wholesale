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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.EDI;
using Energinet.DataHub.Wholesale.Events.Application.Workers;
using Energinet.DataHub.Wholesale.Events.IntegrationTests.Fixture;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
    public async Task ReceiveAggregatedTimeSeriesRequest_WhenMessageHasReference_ReceivesMessage(
        Mock<IServiceProvider> serviceProviderMock,
        Mock<IAggregatedTimeSeriesRequestHandler> handlerMock,
        Mock<ILogger<AggregatedTimeSeriesServiceBusWorker>> loggerMock)
    {
        // Arrange
        var messageHasBeenReceivedEvent = new AutoResetEvent(false);
        var expectedReferenceId = Guid.NewGuid().ToString();
        // ProcessAsync is expected to trigger when a service bus message has been received.
        handlerMock
            .Setup(handler => handler.ProcessAsync(It.IsAny<ServiceBusReceivedMessage>(), expectedReferenceId, It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                messageHasBeenReceivedEvent.Set();
            });

        serviceProviderMock
            .Setup(x => x.GetService(typeof(IAggregatedTimeSeriesRequestHandler)))
            .Returns(handlerMock.Object);

        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory
            .Setup(x => x.CreateScope())
            .Returns(serviceScope.Object);

        serviceProviderMock
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactory.Object);

        var sut = new AggregatedTimeSeriesServiceBusWorker(
            serviceProviderMock.Object,
            loggerMock.Object,
            _sender.ServiceBusOptions,
            _sender.ServiceBusClient);

        // Act
        await sut.StartAsync(CancellationToken.None);
        await _sender.PublishAsync("Hello World", expectedReferenceId);

        // Assert
        var messageHasBeenReceived = messageHasBeenReceivedEvent.WaitOne(timeout: TimeSpan.FromSeconds(1));
        messageHasBeenReceived.Should().BeTrue();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ReceiveAggregatedTimeSeriesRequest_WhenMessageIsMissingReference_DoesNotReceivesMessage(
        Mock<IServiceProvider> serviceProviderMock,
        Mock<IAggregatedTimeSeriesRequestHandler> handlerMock,
        Mock<ILogger<AggregatedTimeSeriesServiceBusWorker>> loggerMock)
    {
        // Arrange
        var messageHasBeenReceivedEvent = new AutoResetEvent(false);
        var expectedReferenceId = Guid.NewGuid().ToString();
        // ProcessAsync is expected to trigger when a service bus message has been received.
        handlerMock
            .Setup(handler => handler.ProcessAsync(It.IsAny<ServiceBusReceivedMessage>(), expectedReferenceId, It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                messageHasBeenReceivedEvent.Set();
            });

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IAggregatedTimeSeriesRequestHandler)))
            .Returns(handlerMock.Object);

        var sut = new AggregatedTimeSeriesServiceBusWorker(
            serviceProviderMock.Object,
            loggerMock.Object,
            _sender.ServiceBusOptions,
            _sender.ServiceBusClient);

        // Act
        await sut.StartAsync(CancellationToken.None);
        await _sender.PublishAsync("Hello World");

        // Assert
        var messageHasBeenReceived = messageHasBeenReceivedEvent.WaitOne(timeout: TimeSpan.FromSeconds(1));
        messageHasBeenReceived.Should().BeFalse();
    }
}
