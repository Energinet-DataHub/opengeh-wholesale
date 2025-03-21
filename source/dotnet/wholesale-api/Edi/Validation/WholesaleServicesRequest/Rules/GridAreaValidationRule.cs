﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Wholesale.Calculations.Interfaces.GridArea;
using Energinet.DataHub.Wholesale.Edi.Contracts;
using Energinet.DataHub.Wholesale.Edi.Validation.Helpers;

namespace Energinet.DataHub.Wholesale.Edi.Validation.WholesaleServicesRequest.Rules;

public class GridAreaValidationRule : IValidationRule<DataHub.Edi.Requests.WholesaleServicesRequest>
{
    private readonly IGridAreaOwnerRepository _gridAreaOwnerRepository;
    private static readonly ValidationError _missingGridAreaCode = new("Netområde er obligatorisk for rollen DDM / Grid area is mandatory for the role DDM.", "D64");
    private static readonly ValidationError _invalidGridArea = new("Ugyldig netområde / Invalid gridarea", "E86");

    public GridAreaValidationRule(IGridAreaOwnerRepository gridAreaOwnerRepository)
    {
        _gridAreaOwnerRepository = gridAreaOwnerRepository;
    }

    public async Task<IList<ValidationError>> ValidateAsync(DataHub.Edi.Requests.WholesaleServicesRequest subject)
    {
        if (subject.RequestedForActorRole != DataHubNames.ActorRole.GridAccessProvider) return NoError;

        if (subject.GridAreaCodes.Count == 0)
            return MissingGridAreaCodeError;

        foreach (var gridAreaCode in subject.GridAreaCodes)
        {
            if (!await GridAreaValidationHelper.IsGridAreaOwnerAsync(_gridAreaOwnerRepository, gridAreaCode, subject.RequestedForActorNumber).ConfigureAwait(false))
                return InvalidGridAreaError;
        }

        return NoError;
    }

    private static IList<ValidationError> NoError => new List<ValidationError>();

    private static IList<ValidationError> MissingGridAreaCodeError => new List<ValidationError> { _missingGridAreaCode };

    private static IList<ValidationError> InvalidGridAreaError => new List<ValidationError> { _invalidGridArea };
}
