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
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Energinet.DataHub.Wholesale.IntegrationEventListener;
using Energinet.DataHub.Wholesale.IntegrationEventListener.Extensions;
using Energinet.DataHub.Wholesale.IntegrationEventListener.MeteringPoints;
using Energinet.DataHub.Wholesale.IntegrationTests.Core;
using Energinet.DataHub.Wholesale.IntegrationTests.Core.Fixtures.FunctionApp;
using Energinet.DataHub.Wholesale.IntegrationTests.Core.TestCommon.Function;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixture;
using FluentAssertions;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.IntegrationEventListener;

public class MeteringPointConnectedListenerEndpointTests
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
        public async Task When_ReceivingMeteringPointConnectedMessage_MeteringPointConnectedDtoIsSentToEventHub()
        {
            // Arrange
            using var whenAllEvent = await Fixture.EventHubListener
                .WhenAny()
                .VerifyCountAsync(1)
                .ConfigureAwait(false);

            var effectiveDate = Timestamp.FromDateTime(
                DateTime.SpecifyKind(
                    new DateTime(2020, 01, 01, 0, 0, 0),
                    DateTimeKind.Utc));

            var meteringPointConnectedEvent = CreateMeteringPointConnectedEvent(effectiveDate);
            var operationTimestamp = new DateTime(2021, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var message = ServiceBusTestMessage.Create(meteringPointConnectedEvent.ToByteArray(), operationTimestamp);
            var jsonSerializer = new JsonSerializer();

            // Act
            await Fixture.MeteringPointConnectedTopic.SenderClient.SendMessageAsync(message);

            // Assert
            await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(MeteringPointConnectedListenerEndpoint)).ConfigureAwait(false);

            var allReceived = whenAllEvent.Wait(TimeSpan.FromSeconds(5));
            allReceived.Should().BeTrue();

            // Only one event is expected
            var actual = jsonSerializer.Deserialize<MeteringPointConnectedDto>(
                Fixture.EventHubListener
                    .ReceivedEvents.Single()
                    .Data.ToString());

            actual.CorrelationId.Should().Be(message.CorrelationId);
            actual.EffectiveDate.Should().Be(effectiveDate.ToInstant());
        }

        private static MeteringPointConnected CreateMeteringPointConnectedEvent(Timestamp effectiveDate)
        {
            var r = new Random();
            var meteringPointId = r.Next(1, 100000);
            return new MeteringPointConnected
            {
                MeteringpointId = Guid.NewGuid().ToString(),
                EffectiveDate = effectiveDate,
                GsrnNumber = meteringPointId.ToString(),
            };
        }
    }
}
