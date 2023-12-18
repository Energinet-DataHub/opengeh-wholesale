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
    private readonly ServiceBusIntegrationEventSenderFixture _fixture;

    public ReceiveIntegrationEventServiceBusWorkerTests(ServiceBusIntegrationEventSenderFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineAutoData]
    public async Task ProcessAsync_WhenAnIntegrationEventIsReceived_HandlerProccessesIt(
        ServiceCollection services,
        Mock<ILogger<ReceiveIntegrationEventServiceBusWorker>> loggerMock)
    {
        // Arrange
        var expectedMessageId = Guid.NewGuid();
        var expectedSubject = "Subject";
        var messageHasBeenReceivedEvent = new AutoResetEvent(false);

        var subscriberSpy = new SubscriberSpy(messageHasBeenReceivedEvent);
        services.AddScoped<ISubscriber>(_ => subscriberSpy);

        var sut = new ReceiveIntegrationEventServiceBusWorker(
            loggerMock.Object,
            _fixture.ServiceBusOptions,
            _fixture.ServiceBusClient,
            services.BuildServiceProvider());

        // Act
        await sut.StartAsync(CancellationToken.None);
        await _fixture.PublishAsync("test message", expectedMessageId.ToString(), expectedSubject);

        // Assert
        using var assertionScope = new AssertionScope();
        var messageHasBeenReceived = subscriberSpy.MessageHasBeenReceivedEvent.WaitOne(timeout: TimeSpan.FromSeconds(1));
        messageHasBeenReceived.Should().BeTrue();
        subscriberSpy.ActualSubject.Should().Be(expectedSubject);
        subscriberSpy.ActualMessageId.Should().Be(expectedMessageId);
    }

    public class SubscriberSpy : ISubscriber
    {
        public SubscriberSpy(AutoResetEvent messageHasBeenReceivedEvent)
        {
            MessageHasBeenReceivedEvent = messageHasBeenReceivedEvent;
        }

        public AutoResetEvent MessageHasBeenReceivedEvent { get; }

        public Guid ActualMessageId { get; private set; }

        public string? ActualSubject { get; private set; }

        public Task HandleAsync(IntegrationEventServiceBusMessage message)
        {
            ActualMessageId = message.MessageId;
            ActualSubject = message.Subject;

            MessageHasBeenReceivedEvent.Set();

            return Task.CompletedTask;
        }
    }
}
