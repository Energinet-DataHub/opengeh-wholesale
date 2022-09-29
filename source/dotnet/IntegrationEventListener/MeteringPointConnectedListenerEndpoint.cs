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

using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.MeteringPoints.IntegrationEvents.Connect;
using Energinet.DataHub.Wholesale.IntegrationEventListener.Common;
using Energinet.DataHub.Wholesale.IntegrationEventListener.MeteringPoints;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Wholesale.IntegrationEventListener
{
    public class MeteringPointConnectedListenerEndpoint
    {
        private const string FunctionName = nameof(MeteringPointConnectedListenerEndpoint);

        private readonly MeteringPointConnectedDtoFactory _meteringPointConnectedDtoFactory;
        private readonly IJsonSerializer _jsonSerializer;

        public MeteringPointConnectedListenerEndpoint(
            MeteringPointConnectedDtoFactory meteringPointConnectedDtoFactory,
            IJsonSerializer jsonSerializer)
        {
            _meteringPointConnectedDtoFactory = meteringPointConnectedDtoFactory;
            _jsonSerializer = jsonSerializer;
        }

        [Function(FunctionName)]
        [EventHubOutput(
            "%" + EnvironmentSettingNames.IntegrationEventsEventHubName + "%",
            Connection = EnvironmentSettingNames.IntegrationEventsEventHubConnectionString)]
        public string Run(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.IntegrationEventsTopicName + "%",
                "%" + EnvironmentSettingNames.MeteringPointConnectedSubscriptionName + "%",
                Connection = EnvironmentSettingNames.IntegrationEventConnectionListenerString)]
            byte[] message)
        {
            var meteringPointCreatedDto = _meteringPointConnectedDtoFactory
                .Create(MeteringPointConnected.Parser.ParseFrom(message));

            return _jsonSerializer.Serialize(meteringPointCreatedDto);
        }
    }
}
