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

using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ListenerMock;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers.GridArea;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers.Organization;
using Energinet.DataHub.Wholesale.IntegrationEventListener;
using Energinet.DataHub.Wholesale.IntegrationEventListener.MarketParticipant;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixture;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixture.FunctionApp;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Function;
using FluentAssertions;
using NodaTime.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.IntegrationEventListener;

public class MarketParticipantChangedListenerEndpointTests
{
    [Collection(nameof(IntegrationEventListenerFunctionAppCollectionFixture))]
    public class RunAsync : FunctionAppTestBase<IntegrationEventListenerFunctionAppFixture>, IAsyncLifetime
    {
        public RunAsync(IntegrationEventListenerFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture, testOutputHelper)
        {
        }

        public Task InitializeAsync()
        {
            Fixture.EventHubListener.Reset();
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task When_ReceivingGridAreaUpdatedMessage_GridAreaUpdatedDtoIsSentToEventHub()
        {
            // Arrange
            using var whenAllEvent = await Fixture.EventHubListener
                .WhenAny()
                .VerifyCountAsync(1)
                .ConfigureAwait(false);

            var operationTimestamp = DateTime.UtcNow;
            var correlationId = Guid.NewGuid().ToString();
            var gridAreaLinkId = Guid.NewGuid();

            var message = ServiceBusTestMessage.Create(
                CreateGridAreaUpdatedEvent(gridAreaLinkId),
                operationTimestamp,
                correlationId);

            // Act
            await Fixture.MarketParticipantChangedTopic.SenderClient.SendMessageAsync(message);

            // Assert
            await FunctionAsserts
                .AssertHasExecutedAsync(Fixture.HostManager, nameof(MarketParticipantChangedListenerEndpoint))
                .ConfigureAwait(false);

            var allReceived = whenAllEvent.Wait(TimeSpan.FromSeconds(5));
            allReceived.Should().BeTrue();

            // Only one event is expected
            var actual = new JsonSerializer().Deserialize<GridAreaUpdatedDto>(
                Fixture.EventHubListener
                    .ReceivedEvents.Single()
                    .Data.ToString());

            actual.GridAreaLinkId.Should().Be(gridAreaLinkId);
            actual.CorrelationId.Should().Be(correlationId);
        }

        [Fact]
        public async Task When_ReceivingUnusedMessage_NothingIsSentToEventHub()
        {
            // Arrange
            using var whenAllEvent = await Fixture.EventHubListener
                .WhenAny()
                .VerifyCountAsync(1)
                .ConfigureAwait(false);

            var operationTimestamp = DateTime.UtcNow;
            var correlationId = Guid.NewGuid().ToString();

            var message = ServiceBusTestMessage.Create(
                CreateOtherEvent(),
                operationTimestamp,
                correlationId);

            // Act
            await Fixture.MarketParticipantChangedTopic.SenderClient.SendMessageAsync(message);

            // Assert
            await FunctionAsserts
                .AssertHasExecutedAsync(Fixture.HostManager, nameof(MarketParticipantChangedListenerEndpoint))
                .ConfigureAwait(false);

            var allReceived = whenAllEvent.Wait(TimeSpan.FromSeconds(5));
            allReceived.Should().BeFalse();
        }

        private static byte[] CreateGridAreaUpdatedEvent(Guid gridAreaLinkId)
        {
            var eventParser = new GridAreaUpdatedIntegrationEventParser();
            return eventParser.Parse(new GridAreaUpdatedIntegrationEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "fake_value",
                "001",
                PriceAreaCode.DK1,
                gridAreaLinkId));
        }

        private static byte[] CreateOtherEvent()
        {
            var eventParser = new OrganizationNameChangedIntegrationEventParser();
            return eventParser.Parse(new OrganizationNameChangedIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                Guid.NewGuid(),
                "fake_value"));
        }
    }
}
