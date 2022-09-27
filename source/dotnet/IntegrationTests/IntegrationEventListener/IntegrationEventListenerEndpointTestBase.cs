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
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ListenerMock;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.IntegrationEventListener.MeteringPoints;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixture;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixture.FunctionApp;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Function;
using FluentAssertions;
using NodaTime;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.IntegrationEventListener;

[Collection(nameof(IntegrationEventListenerFunctionAppCollectionFixture))]
public abstract class IntegrationEventListenerEndpointTestBase<TTargetFunction, TEventHubEventDto>
    : FunctionAppTestBase<IntegrationEventListenerFunctionAppFixture>, IAsyncLifetime
    where TEventHubEventDto : EventHubEventDtoBase
{
    protected IntegrationEventListenerEndpointTestBase(
        IntegrationEventListenerFunctionAppFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
    }

    protected abstract string EventHubMessageType { get; }

    protected abstract string ServiceBusMessageType { get; }

    protected abstract ServiceBusSender IntegrationEventTopicSender { get; }

    protected abstract ServiceBusReceiver IntegrationEventDeadLetterReceiver { get; }

    public async Task InitializeAsync()
    {
        Fixture.EventHubListener.Reset();

        // Clear dead letter queue
        await foreach (var unused in IntegrationEventDeadLetterReceiver.ReceiveMessagesAsync()) { }
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task When_ReceivingCorrectIntegrationEvent_Then_CorrectDtoIsSentToEventHub()
    {
        // Arrange
        using var whenAllEvent = await Fixture.EventHubListener
            .WhenAny()
            .VerifyCountAsync(1)
            .ConfigureAwait(false);

        var operationTimestamp = DateTime.UtcNow;
        var correlationId = Guid.NewGuid().ToString();

        var message = ServiceBusTestMessage.Create(
            CreateIntegrationEventData(),
            operationTimestamp,
            correlationId,
            ServiceBusMessageType);

        // Act
        await IntegrationEventTopicSender.SendMessageAsync(message);

        // Assert
        await FunctionAsserts
            .AssertHasExecutedAsync(Fixture.HostManager, typeof(TTargetFunction).Name)
            .ConfigureAwait(false);

        var allReceived = whenAllEvent.Wait(TimeSpan.FromSeconds(5));
        allReceived.Should().BeTrue();

        // Only one event is expected
        var receivedEvent = Fixture
            .EventHubListener
            .ReceivedEvents
            .Single()
            .Data
            .ToString();

        var actual = new JsonSerializer().Deserialize<TEventHubEventDto>(receivedEvent);
        actual.CorrelationId.Should().Be(correlationId);
    }

    [Theory]
    [InlineData("OperationCorrelationId")]
    [InlineData("OperationTimestamp")]
    public async Task When_ReceivingMessageWithoutIntegrationEventProperty_Then_MessageIsDeadLettered(string missingIntegrationEventProperty)
    {
        // Arrange
        using var whenAnyEvent = await Fixture.EventHubListener
            .WhenAny()
            .VerifyCountAsync(1)
            .ConfigureAwait(false);

        var message = ServiceBusTestMessage.Create(
            CreateIntegrationEventData(),
            DateTime.UtcNow,
            Guid.NewGuid().ToString(),
            ServiceBusMessageType);

        // Act
        message.ApplicationProperties.Remove(missingIntegrationEventProperty);
        await IntegrationEventTopicSender.SendMessageAsync(message);

        // Assert
        await FunctionAsserts
            .AssertHasExecutedAsync(Fixture.HostManager, typeof(TTargetFunction).Name)
            .ConfigureAwait(false);

        var anyReceived = whenAnyEvent.Wait(TimeSpan.FromSeconds(5));
        anyReceived.Should().BeFalse();

        var deadLetteredMessage = await IntegrationEventDeadLetterReceiver.ReceiveMessageAsync();
        deadLetteredMessage.Should().NotBeNull();
        deadLetteredMessage.CorrelationId.Should().Be(message.CorrelationId);
    }

    [Fact]
    public async Task When_ReceivingMessageWithIntegrationEventProperties_Then_PropertiesArePartOfEvent()
    {
        // Arrange
        using var whenAnyEvent = await Fixture.EventHubListener
            .WhenAny()
            .VerifyCountAsync(1)
            .ConfigureAwait(false);

        var timestamp = new DateTime(2022, 2, 1, 13, 37, 00, DateTimeKind.Utc);
        var correlationId = Guid.NewGuid().ToString();

        var message = ServiceBusTestMessage.Create(
            CreateIntegrationEventData(),
            timestamp,
            correlationId,
            ServiceBusMessageType);

        // Act
        await IntegrationEventTopicSender.SendMessageAsync(message);

        // Assert
        await FunctionAsserts
            .AssertHasExecutedAsync(Fixture.HostManager, typeof(TTargetFunction).Name)
            .ConfigureAwait(false);

        var anyReceived = whenAnyEvent.Wait(TimeSpan.FromSeconds(5));
        anyReceived.Should().BeTrue();

        var receivedEvent = Fixture
            .EventHubListener
            .ReceivedEvents
            .Single()
            .Data
            .ToString();

        var actual = new JsonSerializer().Deserialize<TEventHubEventDto>(receivedEvent);
        actual.CorrelationId.Should().Be(correlationId);
        actual.MessageType.Should().Be(EventHubMessageType);
        actual.OperationTime.Should().Be(Instant.FromDateTimeUtc(timestamp));
    }

    protected abstract byte[] CreateIntegrationEventData();
}
