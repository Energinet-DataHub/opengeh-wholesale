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

using Energinet.DataHub.Wholesale.Domain.Base;
using Energinet.DataHub.Wholesale.Infrastructure.EventDispatching;

namespace Energinet.DataHub.Wholesale.Infrastructure.Persistence.DomainEvents;

public class DomainEventRepository : IDomainEventRepository
{
    private readonly IDatabaseContext _databaseContext;

    public DomainEventRepository(IDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public IReadOnlyCollection<IDomainEvent> GetAllDomainEvents()
    {
        var domainEvents = _databaseContext.ChangeTracker
            .Entries<Entity>()
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        return domainEvents;
    }

    public void ClearAllDomainEvents()
    {
        _databaseContext.ChangeTracker
            .Entries<Entity>()
            .ToList()
            .ForEach(e => e.Entity.ClearDomainEvents());
    }
}
