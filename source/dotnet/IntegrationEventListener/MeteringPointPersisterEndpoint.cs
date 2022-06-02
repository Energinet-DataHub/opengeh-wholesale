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

using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;

namespace Energinet.DataHub.Wholesale.IntegrationEventListener
{
    public class MeteringPointPersisterEndpoint
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        public const string FunctionName = nameof(MeteringPointPersisterEndpoint);
        private readonly MessageExtractor<MeteringPointCreated> _messageExtractor;
        private readonly IMeteringPointPersister _meteringPointCreatedEventHandler;

        public MeteringPointPersisterEndpoint(
            MessageExtractor<MeteringPointCreated> messageExtractor,
            IMeteringPointPersister meteringPointCreatedEventHandler)
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
            [NotNull] byte[] message)
        {
            var meteringPointCreatedEvent =
                (MeteringPointCreatedEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);

            await _meteringPointCreatedEventHandler
                .PersistAsync(meteringPointCreatedEvent)
                .ConfigureAwait(false);
        }
    }
}
