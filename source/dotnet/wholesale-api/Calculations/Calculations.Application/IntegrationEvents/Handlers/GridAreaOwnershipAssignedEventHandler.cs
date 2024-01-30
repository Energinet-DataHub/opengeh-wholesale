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

using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.MarketParticipant.Infrastructure.Model.Contracts;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.GridArea;
using NodaTime.Serialization.Protobuf;

namespace Energinet.DataHub.Wholesale.Calculations.Application.IntegrationEvents.Handlers;

public class GridAreaOwnershipAssignedEventHandler : IIntegrationEventHandler
{
    private readonly IGridAreaOwnerRepository _gridAreaOwnerRepository;

    public string EventTypeToHandle => nameof(GridAreaOwnershipAssigned);

    public GridAreaOwnershipAssignedEventHandler(IGridAreaOwnerRepository gridAreaOwnerRepository)
    {
        _gridAreaOwnerRepository = gridAreaOwnerRepository;
    }

    public void Handle(IntegrationEvent integrationEvent)
    {
        var message = (GridAreaOwnershipAssigned)integrationEvent.Message;

        _gridAreaOwnerRepository.Add(
            message.GridAreaCode,
            message.ActorNumber,
            message.ValidFrom.ToInstant(),
            message.SequenceNumber);
    }
}
