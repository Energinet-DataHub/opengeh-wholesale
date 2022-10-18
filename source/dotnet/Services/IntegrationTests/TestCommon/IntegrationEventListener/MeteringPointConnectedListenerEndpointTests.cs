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
using Energinet.DataHub.MeteringPoints.IntegrationEvents.Connect;
using Energinet.DataHub.Wholesale.IntegrationEventListener;
using Energinet.DataHub.Wholesale.IntegrationEventListener.MeteringPoints;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.FunctionApp;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.IntegrationEventListener;

public sealed class MeteringPointConnectedListenerEndpointTests
    : IntegrationEventListenerEndpointTestBase<MeteringPointConnectedListenerEndpoint, MeteringPointConnectedDto>
{
    public MeteringPointConnectedListenerEndpointTests(
        IntegrationEventListenerFunctionAppFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
    }

    protected override string EventHubMessageType => "MeteringPointConnected";

    protected override string ServiceBusMessageType => "MeteringPointConnected";

    protected override ServiceBusSender IntegrationEventTopicSender
        => Fixture.IntegrationEventsTopic.SenderClient;

    protected override ServiceBusReceiver IntegrationEventDeadLetterReceiver =>
        Fixture.MeteringPointConnectedDeadLetterReceiver;

    protected override byte[] CreateIntegrationEventData()
    {
        var meteringPointId = Random.Shared.Next(1, 100000);
        var meteringPointConnected = new MeteringPointConnected
        {
            MeteringpointId = Guid.NewGuid().ToString(),
            EffectiveDate = Timestamp.FromDateTime(DateTime.UtcNow),
            GsrnNumber = meteringPointId.ToString(),
        };
        return meteringPointConnected.ToByteArray();
    }
}
