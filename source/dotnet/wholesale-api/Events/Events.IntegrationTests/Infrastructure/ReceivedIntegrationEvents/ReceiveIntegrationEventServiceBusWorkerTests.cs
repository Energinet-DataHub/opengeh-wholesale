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

using AutoFixture.Xunit2;
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Subscriber;
using Energinet.DataHub.Wholesale.Events.Application.Workers;
using Energinet.DataHub.Wholesale.Events.IntegrationTests.Fixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.IntegrationTests.Infrastructure.ReceivedIntegrationEvents;

public class ReceiveIntegrationEventServiceBusWorkerTests : IClassFixture<ServiceBusIntegrationEventSenderFixture>
{
    private readonly ServiceBusIntegrationEventSenderFixture _sender;

    public ReceiveIntegrationEventServiceBusWorkerTests(ServiceBusIntegrationEventSenderFixture fixture)
    {
        _sender = fixture;
    }

    [Theory]
    [InlineAutoData]
    public async Task ProcessAsync_WhenAnIntegrationEventIsReceived_HandlerProccessesIt(
        Mock<IServiceProvider> serviceProviderMock,
        Mock<ILogger<ReceiveIntegrationEventServiceBusWorker>> loggerMock)
    {
        // Arrange
        var expectedMessageId = Guid.NewGuid();
        var expectedSubject = "Subject";
        var messageHasBeenReceivedEvent = new AutoResetEvent(false);

        var handler = new Subscriber(messageHasBeenReceivedEvent);

        serviceProviderMock
            .Setup(x => x.GetService(typeof(ISubscriber)))
            .Returns(handler);

        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory
            .Setup(x => x.CreateScope())
            .Returns(serviceScope.Object);

        serviceProviderMock
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactory.Object);

        var sut = new ReceiveIntegrationEventServiceBusWorker(
            loggerMock.Object,
            _sender.ServiceBusOptions,
            _sender.ServiceBusClient,
            serviceProviderMock.Object);

        // Act
        await sut.StartAsync(CancellationToken.None);
        await _sender.PublishAsync("test message", expectedMessageId.ToString(), expectedSubject);

        // Assert
        using var assertionScope = new AssertionScope();
        var messageHasBeenReceived = handler.MessageHasBeenReceivedEvent.WaitOne(timeout: TimeSpan.FromSeconds(1));
        messageHasBeenReceived.Should().BeTrue();
        handler.ActualSubject.Should().Be(expectedSubject);
        handler.ActualMessageId.Should().Be(expectedMessageId);
    }
}

#pragma warning disable SA1402
public class Subscriber : ISubscriber
#pragma warning restore SA1402
{
    public Subscriber(AutoResetEvent messageHasBeenReceivedEvent)
    {
        MessageHasBeenReceivedEvent = messageHasBeenReceivedEvent;
    }

    public Task HandleAsync(IntegrationEventServiceBusMessage message)
    {
        ActualMessageId = message.MessageId;
        ActualSubject = message.Subject;
        MessageHasBeenReceivedEvent.Set();
        return Task.CompletedTask;
    }

    public Guid ActualMessageId { get; set; }

    public string? ActualSubject { get; set; }

    public AutoResetEvent MessageHasBeenReceivedEvent { get; set; }
}
