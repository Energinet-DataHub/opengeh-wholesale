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
using Energinet.DataHub.Wholesale.Application.MeteringPoints;

namespace Energinet.DataHub.Wholesale.IntegrationEventListener.Factories
{
    public class MeteringPointCreatedDtoFactory : IMeteringPointCreatedDtoFactory
    {
        private readonly IIntegrationEventContext _integrationEventContext;

        public MeteringPointCreatedDtoFactory(IIntegrationEventContext integrationEventContext)
        {
            _integrationEventContext = integrationEventContext;
        }

        public MeteringPointCreatedDto Create(MeteringPointCreatedEvent meteringPointCreatedEvent)
        {
            if (_integrationEventContext.TryReadMetadata(out var eventMetadata))
            {
                var settlementMethod = meteringPointCreatedEvent.SettlementMethod != null ? (int)meteringPointCreatedEvent.SettlementMethod.Value : (int?)null;
                return new MeteringPointCreatedDto(
                    meteringPointCreatedEvent.MeteringPointId,
                    meteringPointCreatedEvent.GridAreaLinkId,
                    settlementMethod,
                    (int)meteringPointCreatedEvent.ConnectionState,
                    meteringPointCreatedEvent.EffectiveDate,
                    (int)meteringPointCreatedEvent.MeteringPointType,
                    eventMetadata.MessageType,
                    eventMetadata.OperationTimestamp);
            }

            throw new InvalidOperationException($"Could not read metadata for integration event in {nameof(MeteringPointCreatedDtoFactory)}.");
        }
    }
}
