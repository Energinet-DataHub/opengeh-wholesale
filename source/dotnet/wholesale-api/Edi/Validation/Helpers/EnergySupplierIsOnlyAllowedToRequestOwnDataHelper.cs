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

namespace Energinet.DataHub.Wholesale.Edi.Validation.Helpers;

public static class EnergySupplierIsOnlyAllowedToRequestOwnDataHelper
{
    private static readonly ValidationError _invalidEnergySupplierField = new("Feltet EnergySupplier skal være udfyldt med et valid GLN/EIC nummer når en elleverandør anmoder om data / EnergySupplier must be submitted with a valid GLN/EIC number when an energy supplier requests data", "E16");
    private static readonly ValidationError _notEqualToRequestedBy = new("Elleverandør i besked stemmer ikke overenes med elleverandør i header / Energy supplier in message does not correspond with energy supplier in header", "E16");

    public static Task<IList<ValidationError>> ValidateAsync(string requestedForActorRole, string requestedForActorNumber, string energySupplierId)
    {
        if (requestedForActorRole != DataHubNames.ActorRole.EnergySupplier)
             return Task.FromResult(NoError);

        if (string.IsNullOrEmpty(energySupplierId))
            return Task.FromResult(InvalidEnergySupplierError);

        if (!IsValidEnergySupplierIdFormat(energySupplierId))
            return Task.FromResult(InvalidEnergySupplierError);

        if (!RequestedByIdEqualsEnergySupplier(requestedForActorNumber, energySupplierId))
            return Task.FromResult(NotEqualToRequestedByError);

        return Task.FromResult(NoError);
    }

    private static bool IsValidEnergySupplierIdFormat(string energySupplierId)
    {
        return ActorNumberValidationHelper.IsValidGlnNumber(energySupplierId) || ActorNumberValidationHelper.IsValidEicNumber(energySupplierId);
    }

    private static bool RequestedByIdEqualsEnergySupplier(string requestedByActorId, string energySupplierId)
    {
        return requestedByActorId.Equals(energySupplierId, StringComparison.OrdinalIgnoreCase);
    }

    private static IList<ValidationError> NoError => new List<ValidationError>();

    private static IList<ValidationError> InvalidEnergySupplierError => new List<ValidationError> { _invalidEnergySupplierField };

    private static IList<ValidationError> NotEqualToRequestedByError => new List<ValidationError> { _notEqualToRequestedBy };
}
