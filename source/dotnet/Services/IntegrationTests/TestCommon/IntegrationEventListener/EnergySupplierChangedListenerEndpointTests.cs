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
using Energinet.DataHub.EnergySupplying.IntegrationEvents;
using Energinet.DataHub.Wholesale.IntegrationEventListener;
using Energinet.DataHub.Wholesale.IntegrationEventListener.MarketParticipant;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.FunctionApp;
using Google.Protobuf;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.IntegrationEventListener;

public sealed class EnergySupplierChangedListenerEndpointTests
    : IntegrationEventListenerEndpointTestBase<EnergySupplierChangedListenerEndpoint, EnergySupplierChangedDto>
{
    public EnergySupplierChangedListenerEndpointTests(
        IntegrationEventListenerFunctionAppFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
    }

    protected override string EventHubMessageType => "EnergySupplierChanged";

    protected override string ServiceBusMessageType => "EnergySupplierChanged";

    protected override ServiceBusSender IntegrationEventTopicSender =>
        Fixture.IntegrationEventsTopic.SenderClient;

    protected override ServiceBusReceiver IntegrationEventDeadLetterReceiver =>
        Fixture.EnergySupplierChangedDeadLetterReceiver;

    protected override byte[] CreateIntegrationEventData()
    {
        var energySupplierChanged = new EnergySupplierChanged
        {
            Id = Guid.NewGuid().ToString(),
            EffectiveDate = "2022-07-04T08:05:30Z",
            GsrnNumber = Random.Shared.Next(1, 100000).ToString(),
            AccountingpointId = Random.Shared.Next(1, 100000).ToString(),
            EnergySupplierGln = Random.Shared.Next(1, 100000).ToString(),
        };
        return energySupplierChanged.ToByteArray();
    }
}
