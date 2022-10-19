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
using Energinet.DataHub.MeteringPoints.IntegrationEvents.CreateMeteringPoint;
using Energinet.DataHub.Wholesale.IntegrationEventListener;
using Energinet.DataHub.Wholesale.IntegrationEventListener.MeteringPoints;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.FunctionApp;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.IntegrationEventListener;

public sealed class MeteringPointCreatedListenerEndpointTests
    : IntegrationEventListenerEndpointTestBase<MeteringPointCreatedListenerEndpoint, MeteringPointCreatedDto>
{
    public MeteringPointCreatedListenerEndpointTests(
        IntegrationEventListenerFunctionAppFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
    }

    protected override string EventHubMessageType => "MeteringPointCreated";

    protected override string ServiceBusMessageType => "MeteringPointCreated";

    protected override ServiceBusSender IntegrationEventTopicSender
        => Fixture.IntegrationEventsTopic.SenderClient;

    protected override ServiceBusReceiver IntegrationEventDeadLetterReceiver =>
        Fixture.MeteringPointCreatedDeadLetterReceiver;

    protected override byte[] CreateIntegrationEventData()
    {
        var meteringPointId = Random.Shared.Next(1, 100000);
        var meteringPointCreated = new MeteringPointCreated
        {
            MeteringPointId = Guid.NewGuid().ToString(),
            ConnectionState = MeteringPointCreated.Types.ConnectionState.CsNew,
            EffectiveDate = Timestamp.FromDateTime(DateTime.UtcNow),
            GridAreaCode = Guid.NewGuid().ToString(),
            GsrnNumber = meteringPointId.ToString(),
            MeteringPointType = MeteringPointCreated.Types.MeteringPointType.MptConsumption,
            MeteringMethod = MeteringPointCreated.Types.MeteringMethod.MmPhysical,
            SettlementMethod = MeteringPointCreated.Types.SettlementMethod.SmFlex,
        };

        return meteringPointCreated.ToByteArray();
    }
}
