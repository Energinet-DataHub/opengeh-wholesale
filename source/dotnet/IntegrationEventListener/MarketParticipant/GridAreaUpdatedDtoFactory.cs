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
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;

namespace Energinet.DataHub.Wholesale.IntegrationEventListener.MarketParticipant;

public class GridAreaUpdatedDtoFactory
{
    private readonly ICorrelationContext _correlationContext;
    private readonly IIntegrationEventContext _integrationEventContext;

    public GridAreaUpdatedDtoFactory(
        ICorrelationContext correlationContext,
        IIntegrationEventContext integrationEventContext)
    {
        _correlationContext = correlationContext;
        _integrationEventContext = integrationEventContext;
    }

    public GridAreaUpdatedDto Create(GridAreaUpdatedIntegrationEvent gridAreaUpdatedIntegrationEvent)
    {
        ArgumentNullException.ThrowIfNull(gridAreaUpdatedIntegrationEvent);

        var eventMetadata = _integrationEventContext.ReadMetadata();

        return new GridAreaUpdatedDto(
            gridAreaUpdatedIntegrationEvent.GridAreaId,
            gridAreaUpdatedIntegrationEvent.GridAreaLinkId,
            gridAreaUpdatedIntegrationEvent.Code,
            _correlationContext.Id,
            "GridAreaUpdated",
            eventMetadata.OperationTimestamp);
    }
}
