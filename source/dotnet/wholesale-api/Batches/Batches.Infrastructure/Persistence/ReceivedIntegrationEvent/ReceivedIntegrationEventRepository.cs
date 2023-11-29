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

using Energinet.DataHub.Wholesale.Batches.Application.IntegrationEvents;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence.ReceivedIntegrationEvent;

public class ReceivedIntegrationEventRepository : IReceivedIntegrationEventRepository
{
    private readonly IDatabaseContext _context;

    public ReceivedIntegrationEventRepository(IDatabaseContext context)
    {
        _context = context;
    }

    public Task CreateAsync(Guid id, string eventType)
    {
        var task = _context.ReceivedIntegrationEvents.AddAsync(new Application.IntegrationEvents.ReceivedIntegrationEvent(
            id,
            eventType));

        return task.AsTask();
    }

    public Task<bool> ExistsAsync(Guid id)
    {
        return _context.ReceivedIntegrationEvents.AnyAsync(e => e.Id == id);
    }
}
