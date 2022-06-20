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
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Energinet.DataHub.Wholesale.IntegrationEventListener;
using Energinet.DataHub.Wholesale.IntegrationTests.Core.Fixtures.FunctionApp;
using Energinet.DataHub.Wholesale.IntegrationTests.Core.TestCommon.Function;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixture;
using FluentAssertions;
using Google.Protobuf;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Endpoint;

    [IntegrationTest]
    public class MeteringPointCreatedListenerEndpointTests
    {
        [Collection(nameof(WholesaleFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<WholesaleFunctionAppFixture>, IAsyncLifetime
        {
            public RunAsync(WholesaleFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
            }

            public Task InitializeAsync()
            {
                return Task.CompletedTask;
            }

            public Task DisposeAsync()
            {
                return Task.CompletedTask;
            }

            [Fact]
            public async Task When_ReceivingMeteringPointCreatedMessage_MeteringPointCreatedDtoIsSentToEventHub()
            {
                // Arrange
                Fixture.EventHubListener!.Reset();
                using var whenAllEvent = await Fixture.EventHubListener
                    .WhenAny()
                    .VerifyCountAsync(1).ConfigureAwait(false);
                var message = CreateServiceBusMessage();

                // Act
                await Fixture.MeteringPointCreatedTopic!.SenderClient.SendMessageAsync(message);

                // Assert
                await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(MeteringPointCreatedListenerEndpoint)).ConfigureAwait(false);

                var allReceived = whenAllEvent.Wait(TimeSpan.FromSeconds(5));
                allReceived.Should().BeTrue();
                var events = Fixture.EventHubListener.ReceivedEvents;
            }

            private static ServiceBusMessage CreateServiceBusMessage()
            {
                var date = new DateTime(2021, 1, 2, 3, 4, 5, DateTimeKind.Utc);
                var effectiveDate = Google.Protobuf.WellKnownTypes.
                    Timestamp.FromDateTime(
                        DateTime.SpecifyKind(
                            new DateTime(2020, 01, 01, 0, 0, 0),
                            DateTimeKind.Utc));
                var r = new Random();
                var meteringPointId = r.Next(1, 100000);
                var message = new MeteringPointCreated
                {
                    MeteringPointId = Guid.NewGuid().ToString(),
                    ConnectionState = MeteringPointCreated.Types.ConnectionState.CsNew,
                    EffectiveDate = effectiveDate,
                    GridAreaCode = Guid.NewGuid().ToString(),
                    GsrnNumber = meteringPointId.ToString(),
                    MeteringPointType = MeteringPointCreated.Types.MeteringPointType.MptConsumption,
                    MeteringMethod = MeteringPointCreated.Types.MeteringMethod.MmPhysical,
                    SettlementMethod = MeteringPointCreated.Types.SettlementMethod.SmFlex,
                };

                var byteArray = message.ToByteArray();
                var serviceBusMessage = new ServiceBusMessage(byteArray)
                {
                    CorrelationId = Guid.NewGuid().ToString().Replace("-", string.Empty),
                };
                serviceBusMessage.ApplicationProperties.Add("OperationTimestamp", date.ToUniversalTime());
                serviceBusMessage.ApplicationProperties.Add("OperationCorrelationId", "1bf1b76337f14b78badc248a3289d021");
                serviceBusMessage.ApplicationProperties.Add("MessageVersion", 1);
                serviceBusMessage.ApplicationProperties.Add("MessageType", "MeteringPointCreated");
                serviceBusMessage.ApplicationProperties.Add("EventIdentification", "2542ed0d242e46b68b8b803e93ffbf7b");
                return serviceBusMessage;
            }
        }
    }
