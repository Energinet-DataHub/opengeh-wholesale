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
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ListenerMock;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers.GridArea;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers.Organization;
using Energinet.DataHub.Wholesale.IntegrationEventListener;
using Energinet.DataHub.Wholesale.IntegrationEventListener.MarketParticipant;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixture.FunctionApp;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Function;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.IntegrationEventListener;

public sealed class MarketParticipantChangedListenerEndpointTests
    : IntegrationEventListenerEndpointTestBase<MarketParticipantChangedListenerEndpoint, GridAreaUpdatedDto>
{
    public MarketParticipantChangedListenerEndpointTests(
        IntegrationEventListenerFunctionAppFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task When_ReceivingUnusedMessage_Then_NothingIsSentToEventHub()
    {
        // Arrange
        using var whenAllEvent = await Fixture.EventHubListener
            .WhenAny()
            .VerifyCountAsync(1)
            .ConfigureAwait(false);

        var operationTimestamp = DateTime.UtcNow;
        var correlationId = Guid.NewGuid().ToString();
        var messageType = nameof(GridAreaUpdatedDto);

        var message = ServiceBusTestMessage.Create(
            CreateUnusedEvent(),
            operationTimestamp,
            correlationId,
            messageType);

        // Act
        await Fixture.MarketParticipantChangedTopic.SenderClient.SendMessageAsync(message);

        // Assert
        await FunctionAsserts
            .AssertHasExecutedAsync(Fixture.HostManager, nameof(MarketParticipantChangedListenerEndpoint))
            .ConfigureAwait(false);

        var allReceived = whenAllEvent.Wait(TimeSpan.FromSeconds(5));
        allReceived.Should().BeFalse();
    }

    protected override string EventHubMessageType => "GridAreaUpdated";

    protected override ServiceBusSender IntegrationEventTopicSender =>
        Fixture.MarketParticipantChangedTopic.SenderClient;

    protected override ServiceBusReceiver IntegrationEventDeadLetterReceiver =>
        Fixture.MarketParticipantChangedDeadLetterReceiver;

    protected override byte[] CreateIntegrationEventData()
    {
        var eventParser = new GridAreaUpdatedIntegrationEventParser();
        return eventParser.Parse(new GridAreaUpdatedIntegrationEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "fake_value",
            "001",
            PriceAreaCode.DK1,
            Guid.NewGuid()));
    }

    private static byte[] CreateUnusedEvent()
    {
        var eventParser = new OrganizationNameChangedIntegrationEventParser();
        return eventParser.Parse(new OrganizationNameChangedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Guid.NewGuid(),
            "fake_value"));
    }
}
