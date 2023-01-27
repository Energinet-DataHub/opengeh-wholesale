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

using Energinet.DataHub.Wholesale.Domain.Actor;

namespace Energinet.DataHub.Wholesale.Infrastructure.BatchActor;

public static class MarketRoleTypeMapper
{
    // These strings represents where actors for witch results were generated are written from spark.
    // They should only be changed with changing how we write down the actors.
    private const string EnergySupplier = "energy_supplier";

    public static string Map(MarketRoleType marketRoleType)
    {
        switch (marketRoleType)
        {
            case MarketRoleType.EnergySupplier:
                return EnergySupplier;
            default:
                throw new ArgumentOutOfRangeException(nameof(marketRoleType), marketRoleType, null);
        }
    }
}
