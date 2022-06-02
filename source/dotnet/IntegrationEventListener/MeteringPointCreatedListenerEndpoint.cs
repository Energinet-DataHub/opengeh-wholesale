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

using Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Energinet.DataHub.Wholesale.IntegrationEventListener.Common;
using Infrastructure.Core.MessagingExtensions;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Wholesale.IntegrationEventListener
{
    public class MeteringPointCreatedListenerEndpoint
    {
        private const string FunctionName = nameof(MeteringPointCreatedListenerEndpoint);
        private readonly MessageExtractor<MeteringPointCreated> _messageExtractor;
        private readonly IMeteringPointCreatedEventHandler _meteringPointCreatedEventHandler;

        public MeteringPointCreatedListenerEndpoint(
            MessageExtractor<MeteringPointCreated> messageExtractor,
            IMeteringPointCreatedEventHandler meteringPointCreatedEventHandler)
        {
            _messageExtractor = messageExtractor;
            _meteringPointCreatedEventHandler = meteringPointCreatedEventHandler;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.MeteringPointCreatedTopicName + "%",
                "%" + EnvironmentSettingNames.MeteringPointCreatedSubscriptionName + "%",
                Connection = EnvironmentSettingNames.DataHubListenerConnectionString)]
            byte[] message)
        {
            var meteringPointCreatedEvent =
                (MeteringPointCreatedEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);
            await _meteringPointCreatedEventHandler.HandleAsync(meteringPointCreatedEvent).ConfigureAwait(false);
        }
    }
}
