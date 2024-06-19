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

using Energinet.DataHub.Wholesale.Edi.Contracts;

namespace Energinet.DataHub.Wholesale.Edi.Validation.AggregatedTimeSeriesRequest.Rules;

public sealed class RequestedByActorRoleValidationRule : IValidationRule<DataHub.Edi.Requests.AggregatedTimeSeriesRequest>
{
    public Task<IList<ValidationError>> ValidateAsync(DataHub.Edi.Requests.AggregatedTimeSeriesRequest subject)
    {
        return Task.FromResult(subject.RequestedForActorRole switch
        {
            DataHubNames.ActorRole.MeteredDataResponsible => new List<ValidationError>(),
            DataHubNames.ActorRole.BalanceResponsibleParty => new List<ValidationError>(),
            DataHubNames.ActorRole.EnergySupplier => new List<ValidationError>(),
            DataHubNames.ActorRole.GridOperator => new List<ValidationError>
            {
                new(
                    "Rollen skal være MDR når der anmodes om beregnede energitidsserier / Role must be MDR when requesting aggregated measure data",
                    "D02"),
            },
            _ => (IList<ValidationError>)new List<ValidationError>(),
        });
    }
}
