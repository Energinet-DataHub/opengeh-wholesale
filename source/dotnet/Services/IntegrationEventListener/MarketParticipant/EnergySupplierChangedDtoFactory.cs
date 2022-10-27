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
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.EnergySupplying.IntegrationEvents;
using NodaTime.Text;

namespace Energinet.DataHub.Wholesale.IntegrationEventListener.MarketParticipant
{
    public class EnergySupplierChangedDtoFactory
    {
        private const string MessageType = "EnergySupplierChanged";

        private readonly ICorrelationContext _correlationContext;
        private readonly IIntegrationEventContext _integrationEventContext;

        public EnergySupplierChangedDtoFactory(
            ICorrelationContext correlationContext,
            IIntegrationEventContext integrationEventContext)
        {
            _correlationContext = correlationContext;
            _integrationEventContext = integrationEventContext;
        }

        public EnergySupplierChangedDto Create(EnergySupplierChanged energySupplierChanged)
        {
            ArgumentNullException.ThrowIfNull(energySupplierChanged);

            var eventMetadata = _integrationEventContext.ReadMetadata();

            return new EnergySupplierChangedDto(
                energySupplierChanged.AccountingpointId,
                energySupplierChanged.GsrnNumber,
                energySupplierChanged.EnergySupplierGln,
                InstantPattern.General.Parse(energySupplierChanged.EffectiveDate).Value,
                energySupplierChanged.Id,
                _correlationContext.Id,
                MessageType,
                eventMetadata.OperationTimestamp);
        }
    }
}
