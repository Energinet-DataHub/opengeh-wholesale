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

namespace Energinet.DataHub.Wholesale.Domain.ActorAggregate;

public class BatchActors
{
    private readonly ActorRelation[] _actorRelations;

    public BatchActors(ActorRelation[] actorRelations)
    {
        _actorRelations = actorRelations;
    }

    public Actor[] GetEnergySuppliers()
    {
        return _actorRelations.Select(relation => new Actor(relation.EnergySupplierGln)).Distinct().ToArray();
    }

    public Actor[] GetBalanceResponsibleParties()
    {
        return _actorRelations.Select(relation => new Actor(relation.BalanceResponsibleGln)).Distinct().ToArray();
    }
}
