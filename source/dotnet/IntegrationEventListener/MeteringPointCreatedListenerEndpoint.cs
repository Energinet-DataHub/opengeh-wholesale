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

using Energinet.DataHub.Core.App.FunctionApp.Middleware.IntegrationEventContext;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Energinet.DataHub.Wholesale.Infrastructure.Core.MessagingExtensions;
using Energinet.DataHub.Wholesale.Infrastructure.MeteringPoints;
using Energinet.DataHub.Wholesale.IntegrationEventListener.Common;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Wholesale.IntegrationEventListener
{
    public class MeteringPointCreatedListenerEndpoint
    {
        private const string FunctionName = nameof(MeteringPointCreatedListenerEndpoint);
        private readonly MessageExtractor<MeteringPointCreated> _messageExtractor;
        private readonly IIntegrationEventContext _integrationEventContext;
        private readonly IJsonSerializer _jsonSerializer;

        public MeteringPointCreatedListenerEndpoint(
            MessageExtractor<MeteringPointCreated> messageExtractor,
            IIntegrationEventContext integrationEventContext,
            IJsonSerializer jsonSerializer)
        {
            _messageExtractor = messageExtractor;
            _integrationEventContext = integrationEventContext;
            _jsonSerializer = jsonSerializer;
        }

        [Function(FunctionName)]
        [EventHubOutput(
            "%" + EnvironmentSettingNames.MasterDataEventHubName + "%",
            Connection = EnvironmentSettingNames.MasterDataEventHubConnectionString)]
        public async Task<string> RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.MeteringPointCreatedTopicName + "%",
                "%" + EnvironmentSettingNames.MeteringPointCreatedSubscriptionName + "%",
                Connection = EnvironmentSettingNames.IntegrationEventConnectionListenerString)]
            byte[] message)
        {
            var meteringPointCreatedEvent = (MeteringPointCreatedEvent)await _messageExtractor
                .ExtractAsync(message)
                .ConfigureAwait(false);

            return _jsonSerializer.Serialize(new
            {
                _integrationEventContext.EventMetadata.MessageType,
                _integrationEventContext.EventMetadata.OperationTimestamp,
                Message = meteringPointCreatedEvent,
            });
        }
    }
}
