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

using Energinet.DataHub.Core.App.Common.Abstractions.IntegrationEventContext;
using Energinet.DataHub.Core.JsonSerialization;

namespace Energinet.DataHub.Wholesale.Application.MeteringPoints
{
    public class MeteringPointCreatedEventHandler : IMeteringPointCreatedEventHandler
    {
        private readonly IIntegrationEventContext _integrationEventContext;
        private readonly IJsonSerializer _jsonSerializer;

        public MeteringPointCreatedEventHandler(
            IIntegrationEventContext integrationEventContext,
            IJsonSerializer jsonSerializer)
        {
            _integrationEventContext = integrationEventContext;
            _jsonSerializer = jsonSerializer;
        }

        public string Handle(MeteringPointCreatedEvent meteringPointCreatedEvent)
        {
            if (_integrationEventContext.TryReadMetadata(out var eventMetadata))
            {
                return _jsonSerializer.Serialize(new
                {
                    eventMetadata.MessageType,
                    eventMetadata.OperationTimestamp,
                    Message = meteringPointCreatedEvent,
                });
            }

            throw new InvalidOperationException($"Could not read metadata for integration event in {nameof(MeteringPointCreatedEventHandler)}.");
        }
    }
}
